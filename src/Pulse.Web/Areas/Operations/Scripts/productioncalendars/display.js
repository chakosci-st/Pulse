$(document).ready(function () {
    var weekdayLabels = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    var monthNames = {
        '01': 'January',
        '02': 'February',
        '03': 'March',
        '04': 'April',
        '05': 'May',
        '06': 'June',
        '07': 'July',
        '08': 'August',
        '09': 'September',
        '10': 'October',
        '11': 'November',
        '12': 'December'
    };

    var state = {
        allRows: [],
        filteredRows: [],
        table: null,
        viewMode: 'table'
    };

    function getQuarterLabel(value) {
        switch ((value || '').toString().padStart(2, '0')) {
            case '01':
                return '1st Quarter';
            case '02':
                return '2nd Quarter';
            case '03':
                return '3rd Quarter';
            default:
                return '4th Quarter';
        }
    }

    function getMonthLabel(value) {
        return monthNames[(value || '').toString().padStart(2, '0')] || 'Unknown Month';
    }

    function escapeHtml(value) {
        return $('<div />').text(value == null ? '' : value).html();
    }

    function parseDateOnly(value) {
        var safeValue = (value || '').toString().substring(0, 10);
        var parts = safeValue.split('-');
        if (parts.length !== 3) {
            return null;
        }

        return new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));
    }

    function formatIsoDate(date) {
        if (!date) {
            return '';
        }

        return date.getFullYear() + '-' + String(date.getMonth() + 1).padStart(2, '0') + '-' + String(date.getDate()).padStart(2, '0');
    }

    function normalizeRow(item) {
        var fiscalDate = parseDateOnly(item.fiscalDate);
        var calendarYear = (item.calendarYear || id || '').toString();
        var quarterCode = (item.calendarQuarter || '').toString().padStart(2, '0');
        var monthCode = (item.calendarMonth || '').toString().padStart(2, '0');
        var workWeekCode = (item.calendarWorkWeek || '').toString().padStart(2, '0');
        var weekdayIndex = fiscalDate ? fiscalDate.getDay() : -1;

        return {
            fiscalDateValue: fiscalDate,
            fiscalDateDisplay: formatIsoDate(fiscalDate),
            calendarYear: calendarYear,
            calendarQuarter: quarterCode,
            quarterLabel: getQuarterLabel(quarterCode),
            calendarMonth: monthCode,
            monthLabel: getMonthLabel(monthCode),
            calendarWorkWeek: workWeekCode,
            workWeekLabel: 'WW' + calendarYear + workWeekCode,
            weekdayIndex: weekdayIndex,
            weekdayLabel: weekdayIndex >= 0 ? weekdayLabels[weekdayIndex] : '',
            dayNumber: fiscalDate ? fiscalDate.getDate() : '',
            searchText: [
                formatIsoDate(fiscalDate),
                'WW' + calendarYear + workWeekCode,
                getMonthLabel(monthCode),
                getQuarterLabel(quarterCode),
                weekdayIndex >= 0 ? weekdayLabels[weekdayIndex] : ''
            ].join(' ').toLowerCase()
        };
    }

    function compareRows(left, right) {
        var leftTime = left.fiscalDateValue ? left.fiscalDateValue.getTime() : 0;
        var rightTime = right.fiscalDateValue ? right.fiscalDateValue.getTime() : 0;
        return leftTime - rightTime;
    }

    function populateFilterOptions(rows) {
        var quarterSelect = $('#productionCalendarQuarterFilter');
        var monthSelect = $('#productionCalendarMonthFilter');
        var quarterMap = {};
        var monthMap = {};

        quarterSelect.find('option:not(:first)').remove();
        monthSelect.find('option:not(:first)').remove();

        rows.forEach(function (row) {
            quarterMap[row.calendarQuarter] = row.quarterLabel;
            monthMap[row.calendarMonth] = row.monthLabel;
        });

        Object.keys(quarterMap).sort().forEach(function (key) {
            quarterSelect.append($('<option />', { value: key, text: quarterMap[key] }));
        });

        Object.keys(monthMap).sort().forEach(function (key) {
            monthSelect.append($('<option />', { value: key, text: monthMap[key] }));
        });
    }

    function initializeTable(rows) {
        state.table = $('#productioncalendarTable').DataTable({
            processing: true,
            responsive: true,
            serverSide: false,
            paging: false,
            searching: false,
            info: true,
            ordering: true,
            order: [[0, 'asc']],
            scrollCollapse: true,
            scrollY: '50vh',
            data: rows,
            dom: 'Brtip',
            language: {
                emptyTable: 'No data found.',
                processing: '<div>Loading data please wait...</div>'
            },
            buttons: [
                {
                    extend: 'copy',
                    text: '<i class="bi bi-files"></i>',
                    className: 'btn btn-custom-dt',
                    titleAttr: 'Copy'
                },
                {
                    extend: 'csv',
                    text: '<i class="bi bi-filetype-csv"></i>',
                    className: 'btn btn-custom-dt',
                    titleAttr: 'Export as CSV'
                },
                {
                    extend: 'excel',
                    text: '<i class="bi bi-file-earmark-excel"></i>',
                    className: 'btn btn-custom-dt',
                    titleAttr: 'Export as Excel'
                },
                {
                    extend: 'pdf',
                    text: '<i class="bi bi-file-earmark-pdf"></i>',
                    className: 'btn btn-custom-dt',
                    titleAttr: 'Export as PDF'
                },
                {
                    extend: 'print',
                    text: '<i class="bi bi-printer"></i>',
                    className: 'btn btn-custom-dt',
                    titleAttr: 'Print'
                }
            ],
            columns: [
                { data: 'fiscalDateDisplay' },
                { data: 'workWeekLabel' },
                { data: 'monthLabel' },
                { data: 'quarterLabel' }
            ],
            initComplete: function () {
                $('.dt-buttons').appendTo('.production-calendar-toolbar__dt-buttons');
                $('.dt-info').appendTo('.card-tools-productioncalendarTable-size');
            }
        });
    }

    function updateTable(rows) {
        state.table.clear();
        state.table.rows.add(rows);
        state.table.draw();
        $('.dt-info').appendTo('.card-tools-productioncalendarTable-size');
    }

    function buildWeekRows(rows) {
        var weeks = {};

        rows.forEach(function (row) {
            var key = row.workWeekLabel;
            if (!weeks[key]) {
                weeks[key] = {
                    workWeekLabel: row.workWeekLabel,
                    sortValue: row.fiscalDateValue ? row.fiscalDateValue.getTime() : 0,
                    days: new Array(7).fill(null)
                };
            }

            weeks[key].days[row.weekdayIndex] = row;
            if (row.fiscalDateValue && row.fiscalDateValue.getTime() < weeks[key].sortValue) {
                weeks[key].sortValue = row.fiscalDateValue.getTime();
            }
        });

        return Object.keys(weeks).map(function (key) {
            return weeks[key];
        }).sort(function (left, right) {
            return left.sortValue - right.sortValue;
        });
    }

    function buildCalendarHtml(rows) {
        if (!rows.length) {
            return '<div class="production-calendar-empty">No production calendar entries match the current filters.</div>';
        }

        var years = {};
        var html = '';

        rows.forEach(function (row) {
            years[row.calendarYear] = years[row.calendarYear] || {};
            years[row.calendarYear][row.calendarQuarter] = years[row.calendarYear][row.calendarQuarter] || {};
            years[row.calendarYear][row.calendarQuarter][row.calendarMonth] = years[row.calendarYear][row.calendarQuarter][row.calendarMonth] || [];
            years[row.calendarYear][row.calendarQuarter][row.calendarMonth].push(row);
        });

        Object.keys(years).sort().forEach(function (yearKey) {
            var quarterKeys = Object.keys(years[yearKey]).sort();
            html += '<section class="production-calendar-year">';
            html += '<div class="production-calendar-year__header">';
            html += '<h4 class="production-calendar-year__title">Year ' + escapeHtml(yearKey) + '</h4>';
            html += '<div class="production-calendar-year__meta">' + quarterKeys.length + ' quarter' + (quarterKeys.length === 1 ? '' : 's') + '</div>';
            html += '</div>';
            html += '<div class="production-calendar-quarters">';

            quarterKeys.forEach(function (quarterKey) {
                var monthKeys = Object.keys(years[yearKey][quarterKey]).sort();
                html += '<section class="production-calendar-quarter">';
                html += '<div class="production-calendar-quarter__header">';
                html += '<h5 class="production-calendar-quarter__title">' + escapeHtml(getQuarterLabel(quarterKey)) + '</h5>';
                html += '<div class="production-calendar-quarter__meta">' + monthKeys.length + ' month' + (monthKeys.length === 1 ? '' : 's') + '</div>';
                html += '</div>';
                html += '<div class="production-calendar-month-grid">';

                monthKeys.forEach(function (monthKey) {
                    var monthRows = years[yearKey][quarterKey][monthKey].slice().sort(compareRows);
                    var weekRows = buildWeekRows(monthRows);
                    html += '<section class="production-calendar-month">';
                    html += '<div class="production-calendar-month__header">';
                    html += '<h6 class="production-calendar-month__title">' + escapeHtml(getMonthLabel(monthKey)) + '</h6>';
                    html += '<div class="production-calendar-month__meta">' + monthRows.length + ' day' + (monthRows.length === 1 ? '' : 's') + '</div>';
                    html += '</div>';
                    html += '<div class="production-calendar-month__body">';
                    html += '<table class="production-calendar-grid">';
                    html += '<thead><tr><th class="is-week-col">WorkWeek</th>';
                    weekdayLabels.forEach(function (weekday) {
                        html += '<th>' + escapeHtml(weekday) + '</th>';
                    });
                    html += '</tr></thead><tbody>';

                    weekRows.forEach(function (weekRow) {
                        html += '<tr>';
                        html += '<td class="is-week-col"><div class="production-calendar-workweek"><strong>' + escapeHtml(weekRow.workWeekLabel) + '</strong><span>Week row</span></div></td>';
                        weekRow.days.forEach(function (day) {
                            if (!day) {
                                html += '<td><div class="production-calendar-daycell is-empty"></div></td>';
                                return;
                            }

                            html += '<td><div class="production-calendar-daycell">';
                            html += '<div class="production-calendar-daycell__date">' + escapeHtml(day.dayNumber) + '</div>';
                            html += '<div class="production-calendar-daycell__label">' + escapeHtml(day.fiscalDateDisplay) + '</div>';
                            html += '</div></td>';
                        });
                        html += '</tr>';
                    });

                    html += '</tbody></table>';
                    html += '</div></section>';
                });

                html += '</div></section>';
            });

            html += '</div></section>';
        });

        return html;
    }

    function updateSummary(rows) {
        var uniqueWeeks = {};
        var quarterFilter = $('#productionCalendarQuarterFilter').val() || '';
        var monthFilter = $('#productionCalendarMonthFilter').val() || '';
        var summaryText;

        rows.forEach(function (row) {
            uniqueWeeks[row.workWeekLabel] = true;
        });

        summaryText = rows.length + ' day' + (rows.length === 1 ? '' : 's') + ' across ' + Object.keys(uniqueWeeks).length + ' workweek' + (Object.keys(uniqueWeeks).length === 1 ? '' : 's') + '.';

        if (quarterFilter) {
            summaryText += ' Filtered to ' + getQuarterLabel(quarterFilter) + '.';
        }

        if (monthFilter) {
            summaryText += ' Month: ' + getMonthLabel(monthFilter) + '.';
        }

        $('#productionCalendarSummary').text(rows.length ? summaryText : 'No production calendar entries match the current filters.');
        $('#productionCalendarCount span').text(rows.length + ' day' + (rows.length === 1 ? '' : 's'));
    }

    function updateEmptyState() {
        var isEmpty = state.filteredRows.length === 0;

        $('#productionCalendarEmptyState').toggle(isEmpty);
        $('#productionCalendarTablePane').toggleClass('is-active', !isEmpty && state.viewMode === 'table');
        $('#productionCalendarCalendarPane').toggleClass('is-active', !isEmpty && state.viewMode === 'calendar');
    }

    function applyFilters() {
        var searchValue = ($('#productionCalendarSearch').val() || '').trim().toLowerCase();
        var quarterValue = $('#productionCalendarQuarterFilter').val() || '';
        var monthValue = $('#productionCalendarMonthFilter').val() || '';

        state.filteredRows = state.allRows.filter(function (row) {
            if (quarterValue && row.calendarQuarter !== quarterValue) {
                return false;
            }

            if (monthValue && row.calendarMonth !== monthValue) {
                return false;
            }

            if (searchValue && row.searchText.indexOf(searchValue) === -1) {
                return false;
            }

            return true;
        });

        updateTable(state.filteredRows);
        $('#productionCalendarCalendar').html(buildCalendarHtml(state.filteredRows));
        updateSummary(state.filteredRows);
        updateEmptyState();
    }

    function setViewMode(viewMode) {
        state.viewMode = viewMode === 'calendar' ? 'calendar' : 'table';
        $('.project-view-mode-btn').removeClass('active');
        $('.project-view-mode-btn[data-view-mode="' + state.viewMode + '"]').addClass('active');
        $('#productionCalendarActiveViewLabel').html(state.viewMode === 'calendar'
            ? '<i class="bi bi-calendar3-week"></i><span>Calendar View</span>'
            : '<i class="bi bi-table"></i><span>Table View</span>');
        updateEmptyState();

        if (state.viewMode === 'table' && state.table) {
            state.table.columns.adjust().draw(false);
        }
    }

    function bindEvents() {
        $('#productionCalendarSearch').on('input', applyFilters);
        $('#productionCalendarQuarterFilter, #productionCalendarMonthFilter').on('change', applyFilters);
        $('.project-view-mode-btn').on('click', function () {
            setViewMode($(this).data('viewMode'));
        });
    }

    function loadCalendarData() {
        $.ajax({
            url: getApiRootPath() + '/api/productionCalendars/GetByYear?year=' + id,
            type: 'GET',
            contentType: 'application/json'
        }).done(function (response) {
            state.allRows = ((response && response.data) || []).map(normalizeRow).sort(compareRows);
            populateFilterOptions(state.allRows);
            initializeTable(state.allRows);
            bindEvents();
            applyFilters();
        }).fail(function () {
            $('#productionCalendarSummary').text('Failed to load the production calendar.');
            $('#productionCalendarCalendar').html('<div class="production-calendar-empty">Failed to load the production calendar.</div>');
            $('#productionCalendarEmptyState').hide();
        });
    }

    loadCalendarData();
});