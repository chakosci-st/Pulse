function getFirstSaturday(year) {
    var date = new Date(year, 0, 1);
    var day = date.getDay();
    var offset = (6 - day + 7) % 7;
    date.setDate(1 + offset);
    return date;
}

function formatIsoDate(date) {
    return date.getFullYear() + '-' + String(date.getMonth() + 1).padStart(2, '0') + '-' + String(date.getDate()).padStart(2, '0');
}

function parseDateOnly(value) {
    var safeValue = (value || '').toString().substring(0, 10);
    var parts = safeValue.split('-');
    if (parts.length !== 3) {
        return null;
    }

    return new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));
}

function getMonthLabel(monthCode) {
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

    return monthNames[monthCode] || monthCode;
}

function getQuarterLabel(quarterCode) {
    switch ((quarterCode || '').toString().padStart(2, '0')) {
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

function getPattern(januaryWorkWeeks) {
    return parseInt(januaryWorkWeeks, 10) === 5
        ? [5, 4, 4, 4, 4, 5, 4, 4, 5, 4, 4, 5]
        : [4, 4, 5, 4, 4, 5, 4, 4, 5, 4, 4, 5];
}

function refreshDatePicker(year) {
    if (/^\d{4}$/.test(year) && year >= 1999 && year <= 2100) {
        var start = new Date(year, 0, 1);
        var end = new Date(year, 0, 31);
        var firstSaturday = getFirstSaturday(year);

        $('#Week1End').datepicker('setStartDate', start);
        $('#Week1End').datepicker('setEndDate', end);
        $('#Week1End').prop('disabled', false);

        if (!$('#Week1End').val()) {
            $('#Week1End').val(formatIsoDate(firstSaturday));
        }
    } else {
        $('#Week1End').datepicker('setStartDate', null);
        $('#Week1End').datepicker('setEndDate', null);
        $('#Week1End').val('');
        $('#Week1End').prop('disabled', true);
    }
}

function generatePreviewRows(year, week1EndValue, januaryWorkWeeks) {
    var week1End = parseDateOnly(week1EndValue);
    var fiscalDate = new Date(year, 0, 1);
    var fiscalDateEnd = new Date(year, 11, 31);
    var workweek = 1;
    var monthdate = 1;
    var currentMonthWeekCount = 1;
    var weekdayLabels = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    var pattern = getPattern(januaryWorkWeeks);
    var rows = [];

    if (!week1End) {
        return rows;
    }

    while (fiscalDate <= fiscalDateEnd) {
        if (fiscalDate.getDay() === 0 && fiscalDate > week1End) {
            if (workweek < 51) {
                workweek++;
            } else {
                workweek = 52;
            }

            if (currentMonthWeekCount >= pattern[Math.min(monthdate - 1, pattern.length - 1)]) {
                monthdate = Math.min(monthdate + 1, 12);
                currentMonthWeekCount = 1;
            } else {
                currentMonthWeekCount++;
            }
        }

        rows.push({
            calendarYear: year.toString(),
            calendarQuarter: String(Math.floor((monthdate - 1) / 3) + 1).padStart(2, '0'),
            calendarMonth: String(monthdate).padStart(2, '0'),
            calendarWorkWeek: String(workweek).padStart(2, '0'),
            fiscalDateDisplay: formatIsoDate(fiscalDate),
            weekdayIndex: fiscalDate.getDay(),
            dayNumber: fiscalDate.getDate(),
            workWeekLabel: 'WW' + year + String(workweek).padStart(2, '0')
        });

        fiscalDate = new Date(fiscalDate.getFullYear(), fiscalDate.getMonth(), fiscalDate.getDate() + 1);
    }

    return rows;
}

function buildPreviewHtml(rows) {
    if (!rows.length) {
        return '<div class="production-calendar-empty">Select a valid year and week 1 end date to preview the calendar.</div>';
    }

    var weekdayLabels = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    var years = {};
    var html = '';

    rows.forEach(function (row) {
        years[row.calendarYear] = years[row.calendarYear] || {};
        years[row.calendarYear][row.calendarQuarter] = years[row.calendarYear][row.calendarQuarter] || {};
        years[row.calendarYear][row.calendarQuarter][row.calendarMonth] = years[row.calendarYear][row.calendarQuarter][row.calendarMonth] || {};
        years[row.calendarYear][row.calendarQuarter][row.calendarMonth][row.workWeekLabel] = years[row.calendarYear][row.calendarQuarter][row.calendarMonth][row.workWeekLabel] || new Array(7).fill(null);
        years[row.calendarYear][row.calendarQuarter][row.calendarMonth][row.workWeekLabel][row.weekdayIndex] = row;
    });

    Object.keys(years).sort().forEach(function (yearKey) {
        var quarterKeys = Object.keys(years[yearKey]).sort();
        html += '<section class="production-calendar-year">';
        html += '<div class="production-calendar-year__header">';
        html += '<h4 class="production-calendar-year__title">Year ' + yearKey + '</h4>';
        html += '<div class="production-calendar-year__meta">' + quarterKeys.length + ' quarter' + (quarterKeys.length === 1 ? '' : 's') + '</div>';
        html += '</div>';
        html += '<div class="production-calendar-quarters">';

        quarterKeys.forEach(function (quarterKey) {
            var monthKeys = Object.keys(years[yearKey][quarterKey]).sort();
            html += '<section class="production-calendar-quarter">';
            html += '<div class="production-calendar-quarter__header">';
            html += '<h5 class="production-calendar-quarter__title">' + getQuarterLabel(quarterKey) + '</h5>';
            html += '<div class="production-calendar-quarter__meta">' + monthKeys.length + ' month' + (monthKeys.length === 1 ? '' : 's') + '</div>';
            html += '</div>';
            html += '<div class="production-calendar-month-grid">';

            monthKeys.forEach(function (monthKey) {
                var weekMap = years[yearKey][quarterKey][monthKey];
                var weekKeys = Object.keys(weekMap).sort();
                html += '<section class="production-calendar-month">';
                html += '<div class="production-calendar-month__header">';
                html += '<h6 class="production-calendar-month__title">' + getMonthLabel(monthKey) + '</h6>';
                html += '<div class="production-calendar-month__meta">' + weekKeys.length + ' workweek' + (weekKeys.length === 1 ? '' : 's') + '</div>';
                html += '</div>';
                html += '<div class="production-calendar-month__body">';
                html += '<table class="production-calendar-grid">';
                html += '<thead><tr><th class="is-week-col">WorkWeek</th>';
                weekdayLabels.forEach(function (weekdayLabel) {
                    html += '<th>' + weekdayLabel + '</th>';
                });
                html += '</tr></thead><tbody>';

                weekKeys.forEach(function (weekKey) {
                    html += '<tr>';
                    html += '<td class="is-week-col"><div class="production-calendar-workweek"><strong>' + weekKey + '</strong><span>Week row</span></div></td>';
                    weekMap[weekKey].forEach(function (day) {
                        if (!day) {
                            html += '<td><div class="production-calendar-daycell is-empty"></div></td>';
                            return;
                        }

                        html += '<td><div class="production-calendar-daycell">';
                        html += '<div class="production-calendar-daycell__date">' + day.dayNumber + '</div>';
                        html += '<div class="production-calendar-daycell__label">' + day.fiscalDateDisplay + '</div>';
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

function updatePreview() {
    var year = parseInt($('#CalendarYear').val(), 10);
    var week1EndValue = $('#Week1End').val();
    var januaryWorkWeeks = parseInt($('#JanuaryWorkWeeks').val(), 10);
    var rows;
    var q1PatternLabel;

    q1PatternLabel = januaryWorkWeeks === 5 ? '5-4-4' : '4-4-5';
    $('#productionCalendarQuarterPreview').text(q1PatternLabel);
    $('#productionCalendarWeek1Preview').text(week1EndValue || 'Not selected');
    $('#productionCalendarPatternNote span').text(
        januaryWorkWeeks === 5
            ? 'Q1 uses 5-4-4. Q2 to Q4 continue with 4-4-5.'
            : 'Q1 uses 4-4-5. Q2 to Q4 continue with 4-4-5.'
    );

    if (!year || year < 1999 || year > 2100 || !week1EndValue) {
        $('#productionCalendarPreview').html('<div class="production-calendar-empty">Select a valid year and week 1 end date to preview the calendar.</div>');
        $('#productionCalendarPreviewSummary').text('Select a valid year and week 1 end date to preview the calendar.');
        $('#productionCalendarPreviewCount span').text('0 days');
        return;
    }

    rows = generatePreviewRows(year, week1EndValue, januaryWorkWeeks);

    $('#productionCalendarPreview').html(buildPreviewHtml(rows));
    $('#productionCalendarPreviewSummary').text('Previewing ' + rows.length + ' days using Q1 pattern ' + q1PatternLabel + ' and 4-4-5 for the remaining quarters.');
    $('#productionCalendarPreviewCount span').text(rows.length + ' days');
}

function resetProductionCalendarForm() {
    var currentYear = new Date().getFullYear();
    $('#CalendarYear').val(currentYear);
    $('#JanuaryWorkWeeks').val('4');
    $('#Week1End').val('');
    refreshDatePicker(currentYear);
    updatePreview();
}

$(document).ready(function () {
    var currentYear = new Date().getFullYear();

    $('#Week1End').datepicker({
        format: 'yyyy-mm-dd',
        autoclose: true,
        todayHighlight: true,
        startDate: null,
        endDate: null,
        orientation: 'bottom',
        daysOfWeekDisabled: [0, 1, 2, 3, 4, 5]
    });

    $('#CalendarYear').on('input change', function () {
        $('#Week1End').val('');
        refreshDatePicker($(this).val());
        updatePreview();
    });

    $('#Week1End').on('change', updatePreview);
    $('#JanuaryWorkWeeks').on('change', updatePreview);

    $('#CalendarYear').val(currentYear);
    $('#JanuaryWorkWeeks').val('4');
    refreshDatePicker(currentYear);
    updatePreview();

    $('#productioncalendarForm').validate({
        rules: {
            CalendarYear: { required: true },
            Week1End: { required: true },
            JanuaryWorkWeeks: { required: true }
        },
        messages: {
            CalendarYear: { required: '' },
            Week1End: { required: '' },
            JanuaryWorkWeeks: { required: '' }
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $('[id*=-error]').hide();
        },
        invalidHandler: function () {
            toastr.warning('Please complete the calendar year, week 1 end date, and January workweeks before saving.');
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function () {
            var formData = new FormData();
            var $saveButton = $('#buttonSaveProductionCalendar');
            var productionCalendar = {
                CalendarYear: $('#CalendarYear').val(),
                Week1End: $('#Week1End').val(),
                JanuaryWorkWeeks: parseInt($('#JanuaryWorkWeeks').val(), 10)
            };

            formData.append('productioncalendar', JSON.stringify(productionCalendar));

            $.ajax({
                url: getApiRootPath() + '/api/productionCalendars',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                beforeSend: function () {
                    $saveButton.prop('disabled', true).text('Generating...');
                },
                success: function () {
                    toastr.success('Production Calendar for ' + $('#CalendarYear').val() + ' is successfully generated!');
                    resetProductionCalendarForm();
                },
                error: function (xhr) {
                    if (xhr.responseText.indexOf('ORA-00001: unique constraint (NPITRACK.PRODUCTGROUPS_PK) violated') > 0) {
                        toastr.error('Product Group code already exist!');
                    } else {
                        toastr.error('Error: ' + xhr.responseText);
                    }
                },
                complete: function () {
                    $saveButton.prop('disabled', false).text('Save');
                }
            });

            return false;
        }
    });
});