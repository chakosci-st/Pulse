var report1ApiPath = getApiRootPath();

const report1State = {
    records: [],
    filteredRecords: []
};

function report1ToNumber(value) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
}

function report1EscapeHtml(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function report1FormatDate(value) {
    if (!value) {
        return '-';
    }

    const parsed = moment(value);
    return parsed.isValid() ? parsed.format('YYYY-MM-DD') : '-';
}

function report1ComputeCompletion(record) {
    const total = report1ToNumber(record.projectCount);
    const closed = report1ToNumber(record.projectCompleteCount) + report1ToNumber(record.projectCancelCount);

    if (!total) {
        return 0;
    }

    return Math.max(0, Math.min(100, Math.round((closed / total) * 100)));
}

function report1MatchesTaskFilter(record, taskFilter) {
    if (!taskFilter) {
        return true;
    }

    if (taskFilter === 'pending') {
        return report1ToNumber(record.projectTaskPendingCount) > 0;
    }

    if (taskFilter === 'risk') {
        return report1ToNumber(record.projectTaskAtRiskCount) > 0;
    }

    if (taskFilter === 'closed-delayed') {
        return report1ToNumber(record.projectTaskClosedDelayedCount) > 0;
    }

    return true;
}

function report1BuildRow(record) {
    const ownerName = `${record.projectOwnerFirstName || ''} ${record.projectOwnerLastName || ''}`.trim() || '-';
    const statusLabel = getPulseStatusText(record.status, {
        targetDate: record.targetCompletion
    });

    return `
        <tr>
            <td class="report1-project-cell">
                <div class="report1-project-name">${report1EscapeHtml(record.projectName || '-')}</div>
                <div class="report1-project-meta">${report1EscapeHtml(record.projectNo || '-')}</div>
            </td>
            <td>${report1EscapeHtml(ownerName)}</td>
            <td>${getPulseStatusBadge(record.status, { targetDate: record.targetCompletion })}</td>
            <td>${report1EscapeHtml(record.plantCode || '-')}</td>
            <td>${report1EscapeHtml(record.categoryCode || '-')}</td>
            <td>${report1EscapeHtml(record.productCodes || '-')}</td>
            <td>${report1EscapeHtml(report1FormatDate(record.targetStart))}</td>
            <td>${report1EscapeHtml(report1FormatDate(record.targetCompletion))}</td>
            <td class="report1-number">${report1ComputeCompletion(record)}%</td>
            <td class="report1-number">${report1ToNumber(record.projectTaskPendingCount)}</td>
            <td class="report1-number">${report1ToNumber(record.projectTaskAtRiskCount)}</td>
            <td class="report1-number">${report1ToNumber(record.projectTaskClosedCount)}</td>
            <td class="report1-number">${report1ToNumber(record.projectTaskClosedDelayedCount)}</td>
        </tr>`;
}

function report1RenderSummary(records) {
    const totalPending = records.reduce(function (sum, record) {
        return sum + report1ToNumber(record.projectTaskPendingCount);
    }, 0);
    const totalRisk = records.reduce(function (sum, record) {
        return sum + report1ToNumber(record.projectTaskAtRiskCount);
    }, 0);
    const totalDelayed = records.reduce(function (sum, record) {
        return sum + report1ToNumber(record.projectTaskClosedDelayedCount);
    }, 0);

    $('#report1Summary').text(`${records.length} records ready, ${totalPending} pending tasks, ${totalRisk} at risk tasks, ${totalDelayed} closed delayed tasks.`);
    $('#report1Count').html(`<i class="bi bi-table"></i><span>${records.length} records</span>`);
}

function report1RenderTable() {
    const searchValue = ($('#report1Search').val() || '').trim().toLowerCase();
    const statusValue = ($('#report1Status').val() || '').trim().toUpperCase();
    const taskFilter = ($('#report1TaskFilter').val() || '').trim();

    const filtered = report1State.records.filter(function (record) {
        const effectiveStatus = getPulseStatusMeta(record.status, {
            targetDate: record.targetCompletion
        }).code;
        const searchText = [
            record.projectNo,
            record.projectName,
            record.projectOwnerFirstName,
            record.projectOwnerLastName,
            record.plantCode,
            record.categoryCode,
            record.productCodes
        ].join(' ').toLowerCase();

        if (searchValue && searchText.indexOf(searchValue) === -1) {
            return false;
        }

        if (statusValue && effectiveStatus !== statusValue) {
            return false;
        }

        return report1MatchesTaskFilter(record, taskFilter);
    });

    report1State.filteredRecords = filtered;
    report1RenderSummary(filtered);

    if (!filtered.length) {
        $('#report1TableBody').html('<tr><td colspan="13" class="report1-empty">No report records matched the current filters.</td></tr>');
        return;
    }

    $('#report1TableBody').html(filtered.map(report1BuildRow).join(''));
}

function report1WorkbookHtml(records) {
    const exportedAt = moment().format('YYYY-MM-DD HH:mm');
    const rowsHtml = records.map(function (record) {
        const ownerName = `${record.projectOwnerFirstName || ''} ${record.projectOwnerLastName || ''}`.trim() || '-';
        const statusLabel = getPulseStatusText(record.status, {
            targetDate: record.targetCompletion
        });

        return `
            <tr>
                <td>${report1EscapeHtml(record.projectNo || '-')}</td>
                <td>${report1EscapeHtml(record.projectName || '-')}</td>
                <td>${report1EscapeHtml(ownerName)}</td>
                <td>${report1EscapeHtml(statusLabel || '-')}</td>
                <td>${report1EscapeHtml(record.plantCode || '-')}</td>
                <td>${report1EscapeHtml(record.categoryCode || '-')}</td>
                <td>${report1EscapeHtml(record.productCodes || '-')}</td>
                <td>${report1EscapeHtml(report1FormatDate(record.targetStart))}</td>
                <td>${report1EscapeHtml(report1FormatDate(record.targetCompletion))}</td>
                <td>${report1ComputeCompletion(record)}%</td>
                <td>${report1ToNumber(record.projectTaskPendingCount)}</td>
                <td>${report1ToNumber(record.projectTaskAtRiskCount)}</td>
                <td>${report1ToNumber(record.projectTaskClosedCount)}</td>
                <td>${report1ToNumber(record.projectTaskClosedDelayedCount)}</td>
            </tr>`;
    }).join('');

    return `
        <html xmlns:o="urn:schemas-microsoft-com:office:office"
              xmlns:x="urn:schemas-microsoft-com:office:excel"
              xmlns="http://www.w3.org/TR/REC-html40">
        <head>
            <meta charset="utf-8" />
            <!--[if gte mso 9]>
            <xml>
                <x:ExcelWorkbook>
                    <x:ExcelWorksheets>
                        <x:ExcelWorksheet>
                            <x:Name>Report 1</x:Name>
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
                th { background: #0f172a; color: #ffffff; font-weight: 700; text-transform: uppercase; font-size: 9pt; letter-spacing: 0.08em; }
                th, td { border: 1px solid #cbd5e1; padding: 8px 10px; }
                tr:nth-child(even) td { background: #f8fafc; }
                .num { text-align: right; }
            </style>
        </head>
        <body>
            <div class="title">Pulse Report 1</div>
            <div class="subtitle">Project extraction generated ${report1EscapeHtml(exportedAt)}</div>
            <table>
                <thead>
                    <tr>
                        <th>Project No</th>
                        <th>Project Name</th>
                        <th>Owner</th>
                        <th>Status</th>
                        <th>Plant</th>
                        <th>Category</th>
                        <th>Products</th>
                        <th>Target Start</th>
                        <th>Target Completion</th>
                        <th>Completion %</th>
                        <th>Pending Tasks</th>
                        <th>At Risk Tasks</th>
                        <th>Closed Tasks</th>
                        <th>Closed Delayed</th>
                    </tr>
                </thead>
                <tbody>${rowsHtml}</tbody>
            </table>
        </body>
        </html>`;
}

function report1ExportExcel() {
    const records = report1State.filteredRecords || [];
    if (!records.length) {
        return;
    }

    const workbookHtml = report1WorkbookHtml(records);
    const blob = new Blob([workbookHtml], { type: 'application/vnd.ms-excel;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    const timestamp = moment().format('YYYYMMDD_HHmm');

    link.href = url;
    link.download = `pulse_report_1_${timestamp}.xls`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}

function report1FetchRecords() {
    $('#report1TableBody').html('<tr><td colspan="13" class="report1-empty">Loading report records...</td></tr>');

    return $.ajax({
        url: report1ApiPath + '/api/projects/datatables',
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({
            draw: 1,
            start: 0,
            length: -1,
            search: { value: '' },
            status: null,
            nodeType: 'roadmap',
            orderColumn: 'projectname',
            orderDir: 'asc'
        })
    }).done(function (response) {
        report1State.records = Array.isArray(response.data) ? response.data : [];
        report1RenderTable();
    }).fail(function () {
        report1State.records = [];
        report1State.filteredRecords = [];
        $('#report1Summary').text('Unable to load report records right now.');
        $('#report1Count').html('<i class="bi bi-table"></i><span>0 records</span>');
        $('#report1TableBody').html('<tr><td colspan="13" class="report1-empty">Unable to load Report 1.</td></tr>');
    });
}

$(document).ready(function () {
    $('#report1Search').on('input', report1RenderTable);
    $('#report1Status').on('change', report1RenderTable);
    $('#report1TaskFilter').on('change', report1RenderTable);
    $('#report1Export').on('click', report1ExportExcel);

    report1FetchRecords();
});