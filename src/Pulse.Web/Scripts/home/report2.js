var report2ApiPath = getApiRootPath();

const report2State = {
    report: null,
    filteredRows: [],
    frozenKeys: []
};

function report2EscapeHtml(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function report2FormatDate(value) {
    if (!value) {
        return '-';
    }

    const parsed = moment(value);
    return parsed.isValid() ? parsed.format('YYYY-MM-DD') : '-';
}

function report2FixedColumns() {
    return [
        { key: 'projectNo', label: 'Project No', className: 'report2-col-1', width: 120 },
        { key: 'projectName', label: 'Project Name', className: 'report2-col-2', width: 220 },
        { key: 'ownerName', label: 'Owner', className: 'report2-col-3', width: 160 },
        { key: 'status', label: 'Status', className: 'report2-col-4', width: 120 },
        { key: 'plantCode', label: 'Plant', className: 'report2-col-5', width: 95 },
        { key: 'categoryCode', label: 'Category', className: 'report2-col-6', width: 110 },
        { key: 'productCodes', label: 'Products', className: 'report2-col-7', width: 180 }
    ];
}

function report2GetFrozenColumns() {
    const frozenKeyMap = new Set(report2State.frozenKeys);
    return report2FixedColumns().filter(function (column) {
        return frozenKeyMap.has(column.key);
    });
}

function report2CellAttributes(column, isHeader) {
    const isFrozen = report2State.frozenKeys.indexOf(column.key) !== -1;
    const baseStyle = `min-width:${column.width}px;width:${column.width}px;`;
    const baseClasses = ['report2-fixed-col', column.className];

    if (!isFrozen) {
        return {
            className: baseClasses.join(' '),
            style: baseStyle
        };
    }

    const frozenColumns = report2GetFrozenColumns();
    const offset = frozenColumns
        .slice(0, frozenColumns.findIndex(function (item) { return item.key === column.key; }))
        .reduce(function (sum, item) { return sum + item.width; }, 0);
    const zIndex = isHeader ? 6 : 3;

    return {
        className: baseClasses.concat('report2-frozen-cell').join(' '),
        style: `left:${offset}px;${baseStyle}z-index:${zIndex};`
    };
}

function report2RenderFreezeControls() {
    const fixedColumns = report2FixedColumns();
    const frozenColumns = report2GetFrozenColumns();
    const freezeMenuHtml = [
        '<div class="report2-freeze-menu__title">Freeze visible project fields</div>',
        '<div class="report2-freeze-menu__copy">Select which project fields should stay pinned while you scroll across milestone tasks.</div>'
    ].concat(fixedColumns.map(function (column) {
        const isChecked = report2State.frozenKeys.indexOf(column.key) !== -1 ? ' checked' : '';
        return `<label class="report2-freeze-option"><input class="form-check-input report2-freeze-checkbox" type="checkbox" value="${report2EscapeHtml(column.key)}"${isChecked} /><span>${report2EscapeHtml(column.label)}</span></label>`;
    })).join('');

    $('#report2FreezeMenu').html(freezeMenuHtml);
    $('#report2FreezeToggle').html(`<span><i class="bi bi-pin-angle me-2"></i>Freeze fields</span><span class="badge text-bg-light">${frozenColumns.length}</span>`);

    $('#report2FreezeCurrent').html(frozenColumns.length
        ? frozenColumns.map(function (column) {
            return `<span class="report2-freeze-pill"><i class="bi bi-pin-angle-fill"></i>${report2EscapeHtml(column.label)}</span>`;
        }).join('')
        : '<span class="report2-freeze-pill">No frozen fields selected</span>');

    $('#report2FreezeMeta span').text(frozenColumns.length + ' frozen field' + (frozenColumns.length === 1 ? '' : 's'));
    $('#report2TableMeta span').text(frozenColumns.length ? 'Sticky project fields enabled' : 'Sticky project fields disabled');
}

function report2HandleFreezeChange() {
    const selectedKeys = $('.report2-freeze-checkbox:checked').map(function () {
        return $(this).val();
    }).get();

    report2State.frozenKeys = report2FixedColumns()
        .map(function (column) { return column.key; })
        .filter(function (key) { return selectedKeys.indexOf(key) !== -1; });

    report2RenderFreezeControls();
    report2ApplySearch();
}


function report2BuildTable(report, rows) {
    const milestones = Array.isArray(report.milestones) ? report.milestones : [];
    const fixedColumns = report2FixedColumns();

    if (!milestones.length) {
        return '<div class="report2-empty">No milestone-task monitoring columns are available.</div>';
    }

    const milestoneHeader = milestones.map(function (milestone) {
        const colspan = Array.isArray(milestone.tasks) ? milestone.tasks.length : 0;
        return `<th colspan="${colspan}" class="report2-header-milestone">${report2EscapeHtml(milestone.milestoneName || '-')}</th>`;
    }).join('');

    const taskHeader = milestones.map(function (milestone) {
        return (milestone.tasks || []).map(function (task) {
            return `<th class="report2-header-task">${report2EscapeHtml(task.taskName || '-')}</th>`;
        }).join('');
    }).join('');

    const prereqHeader = milestones.map(function (milestone) {
        return (milestone.tasks || []).map(function (task) {
            return `<th class="report2-header-prereq">${report2EscapeHtml(task.prerequisites || '-')}</th>`;
        }).join('');
    }).join('');

    const bodyRows = rows.map(function (row) {
        const fixedCells = fixedColumns.map(function (column) {
            const attributes = report2CellAttributes(column, false);
            const rawValue = row[column.key];
            const cellValue = column.key === 'projectName'
                ? `<div class="report2-project-name">${report2EscapeHtml(rawValue || '-')}</div><div class="report2-project-meta">${report2EscapeHtml(row.projectNo || '-')}</div>`
                : report2EscapeHtml(rawValue || '-');
            return `<td class="${attributes.className}" style="${attributes.style}">${cellValue}</td>`;
        }).join('');

        ////const taskCells = milestones.map(function (milestone) {
        ////    return (milestone.tasks || []).map(function (task) {
        ////        const rawValue = row.taskValues && Object.prototype.hasOwnProperty.call(row.taskValues, task.columnKey)
        ////            ? row.taskValues[task.columnKey]
        ////            : '';
        ////        return `<td class="report2-value-cell">${report2EscapeHtml(rawValue || '-')}</td>`;
        ////    }).join('');
        ////}).join('');

        const normalizedTaskValues = {};
        const taskValues = row.taskValues || {};

        Object.keys(taskValues).forEach(function (key) {
            normalizedTaskValues[String(key || '').toLowerCase()] = taskValues[key];
        });

        const taskCells = milestones.map(function (milestone) {
            return (milestone.tasks || []).map(function (task) {
                const rawValue = normalizedTaskValues[(task.columnKey || '').toLowerCase()] || '';
                return `<td class="report2-value-cell">${report2EscapeHtml(rawValue || '-')}</td>`;
            }).join('');
        }).join('');


        return `<tr>${fixedCells}${taskCells}</tr>`;
    }).join('');

    const fixedHeaderCells = fixedColumns.map(function (column) {
        const attributes = report2CellAttributes(column, true);
        return `<th rowspan="3" class="report2-header-fixed ${attributes.className}" style="${attributes.style}">${report2EscapeHtml(column.label)}</th>`;
    }).join('');

    return `
        <table class="report2-table">
            <thead>
                <tr>${fixedHeaderCells}${milestoneHeader}</tr>
                <tr>${taskHeader}</tr>
                <tr>${prereqHeader}</tr>
            </thead>
            <tbody>${bodyRows}</tbody>
        </table>`;
}

function report2RenderSummary(report, rows) {
    const milestoneCount = (report.milestones || []).length;
    const taskCount = (report.milestones || []).reduce(function (sum, milestone) {
        return sum + (milestone.tasks || []).length;
    }, 0);

    $('#report2Summary').text(`${rows.length} projects across ${milestoneCount} milestones and ${taskCount} task columns.`);
    $('#report2Count').html(`<i class="bi bi-grid-3x3"></i><span>${rows.length} rows</span>`);
}

function report2ApplySearch() {
    if (!report2State.report) {
        return;
    }

    const searchValue = ($('#report2Search').val() || '').trim().toLowerCase();
    const allRows = Array.isArray(report2State.report.rows) ? report2State.report.rows : [];
    const filteredRows = allRows.filter(function (row) {
        const searchText = [
            row.projectNo,
            row.projectName,
            row.ownerName,
            row.status,
            row.plantCode,
            row.categoryCode,
            row.productCodes
        ].join(' ').toLowerCase();

        return !searchValue || searchText.indexOf(searchValue) !== -1;
    });

    report2State.filteredRows = filteredRows;
    report2RenderSummary(report2State.report, filteredRows);

    if (!filteredRows.length) {
        $('#report2TableWrap').html('<div class="report2-empty">No monitoring records matched the current search.</div>');
        return;
    }

    $('#report2TableWrap').html(report2BuildTable(report2State.report, filteredRows));
}

function report2WorkbookHtml(report, rows) {
    const fixedColumns = report2FixedColumns();
    const milestones = report.milestones || [];
    const generatedAt = moment(report.generatedAt || new Date()).format('YYYY-MM-DD HH:mm');

    const topHeader = fixedColumns.map(function (column) {
        return `<th rowspan="3">${report2EscapeHtml(column.label)}</th>`;
    }).join('') + milestones.map(function (milestone) {
        return `<th colspan="${(milestone.tasks || []).length}">${report2EscapeHtml(milestone.milestoneName || '-')}</th>`;
    }).join('');

    const taskHeader = milestones.map(function (milestone) {
        return (milestone.tasks || []).map(function (task) {
            return `<th>${report2EscapeHtml(task.taskName || '-')}</th>`;
        }).join('');
    }).join('');

    const prereqHeader = milestones.map(function (milestone) {
        return (milestone.tasks || []).map(function (task) {
            return `<th>${report2EscapeHtml(task.prerequisites || '-')}</th>`;
        }).join('');
    }).join('');

    const bodyRows = rows.map(function (row) {
        const fixedCells = fixedColumns.map(function (column) {
            return `<td>${report2EscapeHtml(row[column.key] || '-')}</td>`;
        }).join('');
        const taskCells = milestones.map(function (milestone) {
            return (milestone.tasks || []).map(function (task) {
                const rawValue = row.taskValues && Object.prototype.hasOwnProperty.call(row.taskValues, task.columnKey)
                    ? row.taskValues[task.columnKey]
                    : '';
                return `<td>${report2EscapeHtml(rawValue || '-')}</td>`;
            }).join('');
        }).join('');
        return `<tr>${fixedCells}${taskCells}</tr>`;
    }).join('');

    return `
        <html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40">
        <head>
            <meta charset="utf-8" />
            <!--[if gte mso 9]>
            <xml>
                <x:ExcelWorkbook>
                    <x:ExcelWorksheets>
                        <x:ExcelWorksheet>
                            <x:Name>Report 2</x:Name>
                            <x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions>
                        </x:ExcelWorksheet>
                    </x:ExcelWorksheets>
                </x:ExcelWorkbook>
            </xml>
            <![endif]-->
            <style>
                body { font-family: Calibri, Arial, sans-serif; }
                .title { font-size: 18pt; font-weight: 700; color: #0f172a; }
                .subtitle { color: #475569; margin-bottom: 12px; }
                table { border-collapse: collapse; width: 100%; }
                th, td { border: 1px solid #cbd5e1; padding: 6px 8px; vertical-align: middle; }
                th { font-weight: 700; }
            </style>
        </head>
        <body>
            <div class="title">Pulse Report 2</div>
            <div class="subtitle">Project monitoring matrix generated ${report2EscapeHtml(generatedAt)}</div>
            <table>
                <thead>
                    <tr>${topHeader}</tr>
                    <tr>${taskHeader}</tr>
                    <tr>${prereqHeader}</tr>
                </thead>
                <tbody>${bodyRows}</tbody>
            </table>
        </body>
        </html>`;
}

function report2ExportExcel() {
    if (!report2State.report || !report2State.filteredRows.length) {
        return;
    }

    const workbookHtml = report2WorkbookHtml(report2State.report, report2State.filteredRows);
    const blob = new Blob([workbookHtml], { type: 'application/vnd.ms-excel;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = `pulse_report_2_${moment().format('YYYYMMDD_HHmm')}.xls`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}

function report2FetchReport() {
    $('#report2TableWrap').html('<div class="report2-empty">Loading monitoring matrix...</div>');

    return $.ajax({
        url: report2ApiPath + '/api/projects/report2/monitoring',
        type: 'GET',
        dataType: 'json'
    }).done(function (response) {
        report2State.report = response || { milestones: [], rows: [] };
        report2ApplySearch();
    }).fail(function () {
        report2State.report = null;
        report2State.filteredRows = [];
        $('#report2Summary').text('Unable to load the monitoring matrix right now.');
        $('#report2Count').html('<i class="bi bi-grid-3x3"></i><span>0 rows</span>');
        $('#report2TableWrap').html('<div class="report2-empty">Unable to load Report 2.</div>');
    });
}

$(document).ready(function () {
    report2State.frozenKeys = report2FixedColumns().map(function (column) {
        return column.key;
    });

    report2RenderFreezeControls();
    $('#report2Search').on('input', report2ApplySearch);
    $(document).on('change', '.report2-freeze-checkbox', report2HandleFreezeChange);
    $('#report2Export').on('click', report2ExportExcel);
    report2FetchReport();
});