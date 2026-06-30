

var appPath = getAppRootPath();
var apiPath = getApiRootPath();
var dashboardScopeToggle;
const groupState = {};
const dashboardAnalyticsState = {
    counter: null,
    projects: []
};

const analyticsPalette = {
    active: '#0f766e',
    hold: '#f97316',
    completed: '#2563eb',
    planned: '#94a3b8',
    overdue: '#dc2626',
    soon: '#f59e0b',
    thisMonth: '#06b6d4',
    later: '#6366f1',
    bandLow: '#cbd5e1',
    bandMid: '#7dd3fc',
    bandGood: '#38bdf8',
    bandHigh: '#0ea5e9',
    bandDone: '#0f766e'
};

async function loadDashboardScopePreference() {
    const response = await fetch(getAppRootPath() + '/Settings/Profile/DashboardScopePreference', {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Authorization': pulseJwtToken ? `Bearer ${pulseJwtToken}` : ''
        }
    });

    if (!response.ok) {
        return false;
    }

    const body = await response.json();
    return !!body.showAllUsers;
}

async function saveDashboardScopePreference(showAllUsers) {
    const response = await fetch(getAppRootPath() + '/Settings/Profile/DashboardScopePreference', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
            'Authorization': pulseJwtToken ? `Bearer ${pulseJwtToken}` : ''
        },
        body: `showAllUsers=${showAllUsers ? 'true' : 'false'}`
    });

    if (!response.ok) {
        throw new Error('Unable to save dashboard preference.');
    }

    return await response.json();
}

function isDashboardAllContent() {
    var toggle = document.getElementById('dashboardContentScopeToggle');
    return !!(toggle && toggle.checked);
}

function getDashboardCounterUrl() {
    var url = getApiRootPath() + '/api/projects/dashboard/counter';
    if (isDashboardAllContent()) {
        url += '?showAllUsers=true';
    }
    return url;
}

async function fetchRenderCounter() {

    const url = getDashboardCounterUrl();


    const response = await fetch(url, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
        }
    });

    if (!response.ok) {
        throw new Error("Failed to fetch nodes from API: " + response.status);
    }

    // IMPORTANT: call json() exactly once
    const body = await response.json();

    dashboardAnalyticsState.counter = body;

    $('.activeproducts').html(Math.round(body.activeProjects).toLocaleString('en-US'));
    $('.inprogress').html(Math.round(body.inProgress).toLocaleString('en-US'));
    $('.completedtasks').html(Math.round(body.completedTasks).toLocaleString('en-US'));
    $('.overdue').html(Math.round(body.overdue).toLocaleString('en-US'));

    renderAnalyticsDashboard();

    // console.log(nodes);

}

function toNumber(value) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
}

function percentOf(value, total) {
    if (!total) return 0;
    return (value / total) * 100;
}

function escapeHtml(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function formatCount(value) {
    return toNumber(value).toLocaleString('en-US');
}

function formatPercent(value) {
    return `${Math.round(toNumber(value))}%`;
}

function normalizeProjectStatus(status) {
    const normalized = (status || '').toUpperCase();
    if (normalized === 'ONGOING') return 'Active';
    if (normalized === 'HOLD') return 'On Hold';
    if (normalized === 'COMPLETED') return 'Completed';
    return 'Planned';
}

function computeProjectCompletion(project) {
    const total = toNumber(project.projectCount);
    const complete = toNumber(project.projectCompleteCount) + toNumber(project.projectCancelCount);

    if (!total) return 0;

    return Math.max(0, Math.min(100, (complete / total) * 100));
}

function computeProjectAnalytics(projects) {
    const today = moment().startOf('day');
    const statusCounts = {
        Active: 0,
        'On Hold': 0,
        Completed: 0,
        Planned: 0
    };
    const deadlineBuckets = {
        Overdue: 0,
        'Next 7 Days': 0,
        'Next 30 Days': 0,
        Later: 0
    };
    const completionBands = {
        '0-24%': 0,
        '25-49%': 0,
        '50-79%': 0,
        '80-99%': 0,
        '100%': 0
    };
    const ownerLoadMap = {};

    projects.forEach(function (project) {
        const normalizedStatus = normalizeProjectStatus(project.status);
        const progress = computeProjectCompletion(project);
        const ownerName = `${project.projectOwnerFirstName || ''} ${project.projectOwnerLastName || ''}`.trim() || 'Unassigned';
        const completionDate = project.targetCompletion ? moment(project.targetCompletion).startOf('day') : null;

        statusCounts[normalizedStatus] = toNumber(statusCounts[normalizedStatus]) + 1;
        ownerLoadMap[ownerName] = toNumber(ownerLoadMap[ownerName]) + 1;

        if (progress >= 100) completionBands['100%'] += 1;
        else if (progress >= 80) completionBands['80-99%'] += 1;
        else if (progress >= 50) completionBands['50-79%'] += 1;
        else if (progress >= 25) completionBands['25-49%'] += 1;
        else completionBands['0-24%'] += 1;

        if (!completionDate || normalizedStatus === 'Completed') {
            deadlineBuckets.Later += 1;
        } else {
            const daysToTarget = completionDate.diff(today, 'days');
            if (daysToTarget < 0) deadlineBuckets.Overdue += 1;
            else if (daysToTarget <= 7) deadlineBuckets['Next 7 Days'] += 1;
            else if (daysToTarget <= 30) deadlineBuckets['Next 30 Days'] += 1;
            else deadlineBuckets.Later += 1;
        }
    });

    const ownerLoad = Object.keys(ownerLoadMap)
        .map(function (ownerName) {
            return {
                name: ownerName,
                value: ownerLoadMap[ownerName]
            };
        })
        .sort(function (left, right) {
            if (right.value !== left.value) return right.value - left.value;
            return left.name.localeCompare(right.name);
        })
        .slice(0, 5);

    return {
        totalProjects: projects.length,
        statusCounts: statusCounts,
        deadlineBuckets: deadlineBuckets,
        completionBands: completionBands,
        ownerLoad: ownerLoad,
        averageCompletion: projects.length
            ? projects.reduce(function (sum, project) { return sum + computeProjectCompletion(project); }, 0) / projects.length
            : 0
    };
}

function buildDonutMarkup(total, segments, centerLabel) {
    if (!total) {
        return '<div class="chart-empty">No project distribution is available yet.</div>';
    }

    let start = 0;
    const gradientParts = segments.map(function (segment) {
        const from = start;
        const size = percentOf(segment.value, total);
        const to = from + size;
        start = to;
        return `${segment.color} ${from}% ${to}%`;
    });

    const legendHtml = segments.map(function (segment) {
        const share = formatPercent(percentOf(segment.value, total));
        return `
        <div class="legend-row">
          <span class="legend-swatch" style="background:${segment.color}"></span>
          <span>${escapeHtml(segment.label)} <small class="text-muted">${share}</small></span>
          <span class="legend-value">${formatCount(segment.value)}</span>
        </div>`;
    }).join('');

    return `
      <div class="donut-layout">
        <div class="chart-donut" style="background:conic-gradient(${gradientParts.join(', ')})">
          <div class="chart-donut-center">
            <div class="chart-donut-value">${formatCount(total)}</div>
            <div class="chart-donut-label">${escapeHtml(centerLabel)}</div>
          </div>
        </div>
        <div class="legend-list">${legendHtml}</div>
      </div>`;
}

function buildBarListMarkup(rows) {
    const maxValue = rows.reduce(function (max, row) {
        return Math.max(max, toNumber(row.value));
    }, 0);

    if (!maxValue) {
        return '<div class="chart-empty">No deadline pressure detected in the current project set.</div>';
    }

    return `
      <div class="bar-list">
        ${rows.map(function (row) {
            const width = maxValue ? percentOf(row.value, maxValue) : 0;
            return `
            <div class="bar-row">
              <div class="bar-label">${escapeHtml(row.label)}</div>
              <div class="bar-track">
                <div class="bar-fill" style="width:${width}%;background:${row.color}"></div>
              </div>
              <div class="bar-value">${formatCount(row.value)}</div>
            </div>`;
        }).join('')}
      </div>`;
}

function buildCompletionBandsMarkup(total, bands) {
    if (!total) {
        return '<div class="chart-empty">Completion bands appear once projects are loaded.</div>';
    }

    const items = [
        { label: '0-24%', value: bands['0-24%'], color: analyticsPalette.bandLow },
        { label: '25-49%', value: bands['25-49%'], color: analyticsPalette.bandMid },
        { label: '50-79%', value: bands['50-79%'], color: analyticsPalette.bandGood },
        { label: '80-99%', value: bands['80-99%'], color: analyticsPalette.bandHigh },
        { label: '100%', value: bands['100%'], color: analyticsPalette.bandDone }
    ];

    const stackedBar = items.map(function (item) {
        const width = percentOf(item.value, total);
        return `<span class="stacked-band-segment" style="width:${width}%;background:${item.color}"></span>`;
    }).join('');

    const legendHtml = items.map(function (item) {
        return `
        <div class="legend-row">
          <span class="legend-swatch" style="background:${item.color}"></span>
          <span>${escapeHtml(item.label)}</span>
          <span class="legend-value">${formatCount(item.value)}</span>
        </div>`;
    }).join('');

    return `
      <div class="stacked-band">${stackedBar}</div>
      <div class="legend-list">${legendHtml}</div>`;
}

function buildOwnerLoadMarkup(rows) {
    if (!rows.length) {
        return '<div class="chart-empty">Owner load will appear when project owners are available.</div>';
    }

    const maxValue = rows.reduce(function (max, row) {
        return Math.max(max, toNumber(row.value));
    }, 0);

    return `
      <div class="owner-chart-list">
        ${rows.map(function (row, index) {
            const width = maxValue ? percentOf(row.value, maxValue) : 0;
            const color = index === 0 ? analyticsPalette.active : '#60a5fa';
            return `
            <div class="owner-row">
              <div class="owner-name" title="${escapeHtml(row.name)}">${escapeHtml(row.name)}</div>
              <div class="bar-track">
                <div class="bar-fill" style="width:${width}%;background:${color}"></div>
              </div>
              <div class="bar-value">${formatCount(row.value)}</div>
            </div>`;
        }).join('')}
      </div>`;
}

function buildInsightsMarkup(counter, analytics) {
    const activeProjects = toNumber(counter && counter.activeProjects);
    const overdueTasks = toNumber(counter && counter.overdue);
    const inProgressTasks = toNumber(counter && counter.inProgress);
    const completedTasks = toNumber(counter && counter.completedTasks);
    const ownersTracked = analytics.ownerLoad.length;
    const highestOwner = analytics.ownerLoad[0];
    const topDeadlineBucket = Object.keys(analytics.deadlineBuckets).reduce(function (best, key) {
        if (!best || analytics.deadlineBuckets[key] > analytics.deadlineBuckets[best]) return key;
        return best;
    }, 'Later');

    const insights = [
        {
            icon: 'bi bi-exclamation-octagon text-danger',
            title: `${formatCount(overdueTasks)} overdue tasks need attention`,
            detail: overdueTasks
                ? 'Start with owners tied to the most overloaded project groups and overdue deliverables.'
                : 'No overdue task pressure is visible from the live counters right now.'
        },
        {
            icon: 'bi bi-speedometer2 text-primary',
            title: `${formatPercent(analytics.averageCompletion)} average completion across ${formatCount(analytics.totalProjects)} projects`,
            detail: analytics.totalProjects
                ? `The largest delivery bucket is ${topDeadlineBucket.toLowerCase()}, which helps prioritize review meetings.`
                : 'Project completion insights will populate after the project feed returns data.'
        },
        {
            icon: 'bi bi-people text-success',
            title: highestOwner ? `${escapeHtml(highestOwner.name)} currently owns the largest visible share` : `${formatCount(ownersTracked)} owners tracked`,
            detail: highestOwner
                ? `${formatCount(highestOwner.value)} projects are aligned to that owner, while ${formatCount(activeProjects)} projects remain active overall.`
                : `${formatCount(inProgressTasks)} tasks are in progress and ${formatCount(completedTasks)} have already been completed.`
        }
    ];

    return insights.map(function (insight) {
        return `
        <div class="insight-pill">
          <i class="${insight.icon}"></i>
          <div>
            <strong>${insight.title}</strong>
            <span>${insight.detail}</span>
          </div>
        </div>`;
    }).join('');
}

function renderAnalyticsDashboard() {
    const portfolioElement = document.getElementById('portfolioStatusChart');
    const deliveryElement = document.getElementById('deliveryWindowChart');
    const completionElement = document.getElementById('completionBandChart');
    const ownerLoadElement = document.getElementById('ownerLoadChart');
    const insightElement = document.getElementById('dashboardInsights');
    const updatedElement = document.getElementById('analyticsLastUpdated');
    const coverageElement = document.getElementById('analyticsCoverage');

    if (!portfolioElement || !deliveryElement || !completionElement || !ownerLoadElement || !insightElement) {
        return;
    }

    const projects = dashboardAnalyticsState.projects || [];
    const counter = dashboardAnalyticsState.counter || {};
    const analytics = computeProjectAnalytics(projects);

    portfolioElement.innerHTML = buildDonutMarkup(analytics.totalProjects, [
        { label: 'Active', value: analytics.statusCounts.Active, color: analyticsPalette.active },
        { label: 'On Hold', value: analytics.statusCounts['On Hold'], color: analyticsPalette.hold },
        { label: 'Completed', value: analytics.statusCounts.Completed, color: analyticsPalette.completed },
        { label: 'Planned', value: analytics.statusCounts.Planned, color: analyticsPalette.planned }
    ], 'Projects');

    deliveryElement.innerHTML = buildBarListMarkup([
        { label: 'Overdue', value: analytics.deadlineBuckets.Overdue, color: analyticsPalette.overdue },
        { label: 'Next 7 Days', value: analytics.deadlineBuckets['Next 7 Days'], color: analyticsPalette.soon },
        { label: 'Next 30 Days', value: analytics.deadlineBuckets['Next 30 Days'], color: analyticsPalette.thisMonth },
        { label: 'Later', value: analytics.deadlineBuckets.Later, color: analyticsPalette.later }
    ]);

    completionElement.innerHTML = buildCompletionBandsMarkup(analytics.totalProjects, analytics.completionBands);
    ownerLoadElement.innerHTML = buildOwnerLoadMarkup(analytics.ownerLoad);
    insightElement.innerHTML = buildInsightsMarkup(counter, analytics);

    if (updatedElement) {
        updatedElement.textContent = `Last refreshed ${moment().format('MMM D, YYYY h:mm A')}`;
    }

    if (coverageElement) {
        coverageElement.textContent = analytics.totalProjects
            ? `${formatCount(analytics.totalProjects)} projects visualized, ${formatCount(toNumber(counter.overdue))} overdue tasks, ${formatCount(toNumber(counter.inProgress))} tasks in progress.`
            : 'No project distribution loaded yet.';
    }
}

function goToActiveProjects() {
    window.location.href = '/projects/index?status=active';
}

function goToInProgressTasks() {
    window.location.href = '/projects/ProjectTasks?status=ongoing';
}

function goToCompletedTasks() {
    window.location.href = '/projects/ProjectTasks?status=completed';
}

function goToOverdue() {
    window.location.href = '/projects/ProjectTasks?status=overdue';
}

document.addEventListener('DOMContentLoaded', async function () {
    dashboardScopeToggle = document.getElementById('dashboardContentScopeToggle');
    if (dashboardScopeToggle) {
        dashboardScopeToggle.checked = await loadDashboardScopePreference();
    }

        fetchRenderCounter();

        var projectsRootMilestonesTable = $('#projectsRootMilestonesTable').DataTable({
            "processing": true, "responsive": true,
            "serverSide": true, paging: false,
            scrollY: '300px', // Set the desired height
            scrollCollapse: true,
            "ajax": {
                "url": getApiRootPath() + "/api/projects/datatables",
                "type": "POST",
                "contentType": "application/json",
                // xhrFields: { withCredentials: true }, //** REMOVED**
                "data": function (d) {
                    // Add custom filter
                    d.status = "ONGOING, COMPLETED, HOLD";
                    d.parenttype = "roadmap";
                    d.showAllUsers = isDashboardAllContent();
                    // Extract sort info
                    if (d.order && d.order.length > 0) {
                        var sortIndex = d.order[0].column; // index of sorted column
                        var sortDir = d.order[0].dir;      // 'asc' or 'desc'
                        var sortBy = d.columns[sortIndex].data; // column name/key

                        // Add to payload
                        d.sortBy = sortBy;
                        d.sortDirection = sortDir;
                    }

                    return JSON.stringify(d);
                }
            },
            "dom": ' <"search"f><"top"l>rt<"bottom"ip><"clear">',
            "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
            columnDefs: [
                { targets: [1, 2], className: 'text-center' },
                { targets: 2, className: 'text-center', orderable: false },

                { targets: [0, 2], responsivePriority: 1, },
                { targets: [1], responsivePriority: 2, },


            ],
            "initComplete": function () {

                $('.dt-paging').appendTo('.card-tools-projectsRootMilestonesTable-pagination');
                $('.dt-search').appendTo('.card-tools-projectsRootMilestonesTable-filter');
                $('.dt-length').appendTo('.card-tools-projectsRootMilestonesTable-length');

                $('#projectsRootMilestonesTable_info').appendTo('.card-tools-projectsRootMilestonesTable-size');

                //$('#projectsRootMilestonesTable_buttons').appendTo('.card-tools-projectsRootMilestonesTable-buttons');
                //datatableMyTasks.buttons().container().appendTo('.card-tools-projectsRootMilestonesTable-buttons');

            },
            "language": {
                "search": "",
                "searchPlaceholder": "Search...",
                "emptyTable": "No data found.",
                'processing': `<span class="text-muted" style="font-size:0.85rem;">
            <i class="fas fa-spinner fa-spin me-2"></i>Loading projects &amp; milestones...
          </span>`,
                paginate: {
                    previous: "«",
                    next: "»"
                }
            },
            "columns": [
                {
                    "data": "projectName",
                    "render": function (value, type, data) {
                        return `<div class="project-name"><a href="/projects/${data.projectNo}/review">${projectIcon(data.projectIcon, data.projectIconColor)}${data.projectName}</a></div><small>${data.productCodes}</small>`;
                    }
                },
                {
                    "data": "projectOwnerFirstName",
                    "render": function (value, type, data) {
                        return `${data.projectOwnerFirstName} ${data.projectOwnerLastName}`;
                    }
                },
                {
                    "data": "targetStart",
                    "render": function (value, type, data) {
                        const formattedStartDate = moment(data.targetStart).format("YYYY-MM-DD");
                        const formattedEndDate = moment(data.targetCompletion).format("YYYY-MM-DD");
                        return `<div style="font-size:0.8rem;"><i class="far fa-calendar me-1"></i>${formattedStartDate} → ${formattedEndDate}</div>`;
                    }
                },
                {
                    "data": "projectCount",
                    "render": function (value, type, data) {
                        var percentCompleted = (data.projectCompleteCount + data.projectCancelCount) > 0 ? ((data.projectCompleteCount + data.projectCancelCount) / data.projectCount) * 100 : 0;

                        roundedpercentCompleted = roundTo(percentCompleted, 2)

                        return `<div class="d-flex align-items-center gap-2">
    <div class="flex-grow-1">
        <div class="progress">
            <div class="progress-bar bg-success" style="width:${roundedpercentCompleted}%;"></div>
        </div>
    </div>
    <span style="font-size:0.8rem;">${roundedpercentCompleted}%</span>
</div>`
                            ;
                    }
                },

                {
                    "data": "status",
                    "render": function (value, type, data) {
                        return getPulseStatusBadge(value, {
                            targetDate: data.targetCompletion
                        });
                    }
                },
                {
                    "data": "nodeName",
                    "render": function (value, type, data) {
                        return `<div class="milestone-name"><i class="fas fa-flag-checkered me-1 text-primary"></i>${value}</div>`;
                    }
                },
                {
                    "data": "targetStart",
                    "render": function (value, type, data) {
                        var formattedStartDate = moment(data.projectNodeTargetStart).format("YYYY-MM-DD");
                        var formattedEndDate = moment(data.projectNodeTargetCompletion).format("YYYY-MM-DD");


                        if (data.nodeName == "Root Activity") {
                            formattedStartDate = moment(data.targetStart).format("YYYY-MM-DD");
                            formattedEndDate = moment(data.targetCompletion).format("YYYY-MM-DD");
                        }

                        return `<div style="font-size:0.8rem;"><i class="far fa-calendar me-1"></i>${formattedStartDate} → ${formattedEndDate}</div>`;
                    }
                },
                {
                    "data": "projectNodeCompleteCount",
                    "render": function (value, type, data) {
                        var percentCompleted = (data.projectNodeCompleteCount + data.projectNodeCancelCount) > 0 ? ((data.projectNodeCompleteCount + data.projectNodeCancelCount) / data.projectNodeCount) * 100 : 0;
                        const roundedpercentCompleted = roundTo(percentCompleted, 2)
                        //bg-info
                        return `<div class="d-flex align-items-center gap-2">
                                                <div class="flex-grow-1">
                                                    <div class="progress">
                                                        <div class="progress-bar bg-primary" style="width:${roundedpercentCompleted}%;"></div>
                                                    </div>
                                                </div>
                                                <span style="font-size:0.8rem;">${roundedpercentCompleted}%</span>
                                            </div>`;
                    }
                },
                {
                    "data": "projectNodeStatus",
                    "render": function (value, type, data) {
                        var effectiveStatus = value;

                        if (data.nodeName == "Root Activity") {
                            effectiveStatus = data.projectNodeOngoingCount - 1 === 0 ? "COMPLETED" : "ONGOING";
                        }

                        return getPulseStatusBadge(effectiveStatus, {
                            targetDate: data.nodeName == "Root Activity" ? data.targetCompletion : data.projectNodeTargetCompletion
                        });
                    }
                },
            ]
        });


        function RefreshDetails() {

        }






        ////// Filters
        ////$("#filter-project-status").on("change", function () {
        ////    projectsMilestonesTable.column(6).search(this.value).draw();
        ////});

    ////$("#filter-milestone-status").on("change", function () {
    ////    projectsMilestonesTable.column(10).search(this.value).draw();
    ////});

    ////$("#filter-owner").on("keyup change", function () {
    ////    const val = this.value;
    ////    // search on project owner (5) or milestone owner (9)
    ////    projectsMilestonesTable
    ////        .columns([5, 9])
    ////        .search(val)
    ////        .draw();
    ////});

    ////$("#filter-search").on("keyup change", function () {
    ////    projectsMilestonesTable
    ////        .columns([2, 7]) // project name, milestone name
    ////        .search(this.value)
    ////        .draw();
    ////});



// ==========================
// CONFIG
// ==========================


const API_URL = getApiRootPath() + '/api/projects/datatables';

// ==========================
// HELPERS
// ==========================
function formatPeriod(start, end) {
    if (!start && !end) return '';
    return (start || '') + ' \u2192 ' + (end || '');
}

function mapProjectStatusToLabel(apiStatus) {
    return getPulseStatusText(apiStatus);
}

function mapMilestoneStatusToLabel(apiStatus, targetDate) {
    return getPulseStatusText(apiStatus, { targetDate: targetDate });
}

function projectStatusBadge(apiStatus, targetDate) {
    return getPulseStatusBadge(apiStatus, { targetDate: targetDate });
}

function milestoneStatusBadge(apiStatus, targetDate) {
    return getPulseStatusBadge(apiStatus, { targetDate: targetDate });
}

function progressBar(colorClass, value) {
    const v = Math.max(0, Math.min(100, Number(value) || 0));
    return `
      <div class="d-flex align-items-center gap-2">
        <div class="flex-grow-1">
          <div class="progress">
            <div class="progress-bar ${colorClass}" style="width:${v}%;"></div>
          </div>
        </div>
        <span style="font-size:0.8rem;">${v}%</span>
      </div>`;
}

function milestoneProgressBar(value) {
    const v = Math.max(0, Math.min(100, Number(value) || 0));
    return `
      <div class="d-flex align-items-center gap-2">
        <div class="flex-grow-1">
          <div class="progress">
            <div class="progress-bar bg-primary" style="width:${v}%;"></div>
          </div>
        </div>
        <span class="milestone-progress">${v}%</span>
      </div>`;
}

function projectIcon(name, color) {
    return `<i class="${name} me-1" style="color:${color}"></i>`;
}

// ==========================
// BUILD PROJECT ROW
// ==========================
function buildProjectRow(project, index) {
    // Map your backend fields here
    const projectId = project.projectNo;
    const name = project.projectName;
    const owner = project.projectOwnerFirstName + " " + project.projectOwnerLastName;
    const statusRaw = project.status;      // ONGOING / COMPLETED / HOLD ...
    const icon = project.projectIcon;
    const iconColor = project.projectIconColor;
    const statusLabel = mapProjectStatusToLabel(statusRaw, project.targetCompletion);
    const productCodes = project.productCodes;
    var projPct = (project.projectCompleteCount + project.projectCancelCount) > 0
        ? ((project.projectCompleteCount + project.projectCancelCount) / project.projectCount) * 100
        : 0;

    const progress = Math.round(projPct);
    const startDate = moment(project.targetStart).format("YYYY-MM-DD");
    const endDate = moment(project.targetCompletion).format("YYYY-MM-DD");
    const projectKeyClass = 'project-' + projectId + '-milestones';

    return `
      <tr class="project-row"
          data-project-id="${projectId}"
          data-project-name="${name || ''}"
          data-project-owner="${owner || ''}"
          data-project-status="${statusLabel || ''}"
          data-project-progress="${progress || 0}"
          data-milestones-loaded="false">
        <td>
          <button class="btn-toggle-row" 
                  data-project-id="${projectId}"
                  data-ms-class="${projectKeyClass}">
            <i id="${projectId}-chevron" class="fas fa-chevron-right"></i>
          </button>
        </td>
        <td>
          <div class="project-name">
            <a href="/projects/${projectId}/review">${projectIcon(icon, iconColor)}${name || ''}</a>
            <small>${productCodes || ''}</small>
          </div>
        </td>
        <td>
          <div style="font-size:0.8rem;">
            <i class="far fa-calendar me-1"></i>${formatPeriod(startDate, endDate)}
          </div>
        </td>
        <td>
                    ${progressBar(
                        statusLabel.toLowerCase() === 'completed' ? 'bg-secondary' : 'bg-success',
                        progress || 0
                )}
        </td>
        <td>${owner || ''}</td>
        <td>
          ${projectStatusBadge(statusRaw, project.targetCompletion)}
        </td>
        <td class="text-muted" colspan="4" style="font-size:0.8rem;">
          <i>Click chevron to load milestones</i>
        </td>
      </tr>
    `;
}

// ==========================
// BUILD MILESTONE ROWS (for a project)
// ==========================
function buildMilestoneRows(projectId, milestones) {
    const projectKeyClass = 'project-' + projectId + '-milestones';

    if (!Array.isArray(milestones) || milestones.length === 0) {
        return {
            html: `
        <tr class="milestone-row ${projectKeyClass}">
          <td></td>
          <td colspan="9" class="text-muted" style="font-size:0.8rem;">
            <i>No milestones found for this project.</i>
          </td>
        </tr>
      `,
            avatarGroups: []
        };
    }

    const avatarGroups = [];

    const rowsHtml = milestones.map(function (m, idx) {
        const label = m.nodeName || ('Milestone ' + (idx + 1));
        const mName = m.nodeName;

        const mNodeStatus = m.projectNodeStatus;
        const mStart = moment(m.projectNodeTargetStart).format("YYYY-MM-DD");
        const mEnd = moment(m.projectNodeTargetCompletion).format("YYYY-MM-DD");
        let mStatusRaw = mNodeStatus;



        var nodePct = (m.projectNodeCompleteCount + m.projectNodeCancelCount) > 0
            ? ((m.projectNodeCompleteCount + m.projectNodeCancelCount) / m.projectNodeCount) * 100
            : 0;


        if ((m.nodeId === "__ROOTACTIVITY__") && nodePct === 100)
            mStatusRaw = "COMPLETED"

        const mStatusLbl = mapMilestoneStatusToLabel(mStatusRaw, m.projectNodeTargetCompletion);



        const mProgress = Math.round(nodePct);
        const owners = JSON.parse(m.jsonNodeOwners ?? m.jsonMembers ?? '[]');
        const avatarContainerId = `milestone-avatar-group-${projectId}-${idx}`;
        const ownerNames = owners.map(function (item) {
            return [item.firstname, item.lastname].filter(Boolean).join(' ').trim();
        }).filter(Boolean).join(' ');

        avatarGroups.push({
            container: `#${avatarContainerId}`,
            owners: owners
        });




        return `
        <tr class="milestone-row ${projectKeyClass}"
            data-milestone-name="${mName || ''}"
      data-milestone-owner="${ownerNames || ''}"
            data-milestone-status="${mStatusLbl || ''}"
            data-milestone-progress="${mProgress || 0}">
          <td></td>
          <td colspan="4">
            <div class="milestone-label mb-1">${label}</div>
            <div class="milestone-name">
              <i class="fas fa-flag-checkered me-1 text-primary"></i>${mName || ''}
            </div>
          </td>
          <td>
            ${milestoneStatusBadge(mStatusRaw, m.projectNodeTargetCompletion)}
          </td>
          <td>
            <div>
              <div class="milestone-name">${mName || ''}</div>
              <div style="font-size:0.78rem;color:#6b7280;">
                <i class="far fa-calendar me-1"></i>${formatPeriod(mStart, mEnd)}
              </div>
            </div>
          </td>
          <td>
            ${milestoneProgressBar(mProgress || 0)}
          </td>
            <td><div id="${avatarContainerId}"></div></td>
          <td>
            ${milestoneStatusBadge(mStatusRaw, m.projectNodeTargetCompletion)}
          </td>
        </tr>
      `;
    }).join('');

    return {
        html: rowsHtml,
        avatarGroups: avatarGroups
    };
}

function renderMilestoneOwnerAvatarGroups(avatarGroups) {
    if (!Array.isArray(avatarGroups) || avatarGroups.length === 0) {
        return;
    }

    avatarGroups.forEach(function (group) {
        loadAndRenderAvatarGroup({
            dataSource: group.owners,
            container: group.container,
            maxVisible: 10,
            avatarSize: 40,
            avatarSpacing: 20,
            label: '...',
            showLabel: false,
            emptyText: 'No members found',
            backgroundColor: '#e0f2fe',
            fontColor: '#075985',
            labelBackgroundColor: '#607d8b',
            userInformationUrl: '/Settings/Profile/Index/{id}',
            userInformationTarget: '_self',
            labelFontColor: '#fff',
            sort: 'initials',
            onMoreClick: function (extraUsers, event) {
                alert('Show more users:\n' + extraUsers.map(u => u.name).join(', '));
            },
            onLabelClick: function (allUsers, event) {
                alert('All users:\n' + allUsers.map(u => u.name).join(', '));
            },
            transform: function (data) {
                return data.map(function (item) {
                    return {
                        id: item.userid,
                        name: item.firstname + ' ' + item.lastname,
                        avatarUrl: `/Settings/Profile/Photo/${encodeURIComponent(item.userid)}`
                    };
                });
            }
        });
    });
}

// ==========================
// FILTERING
// ==========================
function applyFilters() {
    const projStatus = document.getElementById('filterProjectStatus').value.toLowerCase();
    const msStatus = document.getElementById('filterMilestoneStatus').value.toLowerCase();
    const owner = document.getElementById('filterOwner').value.toLowerCase();
    const search = document.getElementById('filterSearch').value.toLowerCase();

    const rows = document.querySelectorAll('#projectsMilestonesTable tbody tr');

    rows.forEach(row => {
        const isProjectRow = row.classList.contains('project-row');
        const isMilestoneRow = row.classList.contains('milestone-row');
        let visible = true;

        if (isProjectRow) {
            const pStatus = (row.dataset.projectStatus || '').toLowerCase();
            const pOwner = (row.dataset.projectOwner || '').toLowerCase();
            const pName = (row.dataset.projectName || '').toLowerCase();

            if (projStatus && pStatus !== projStatus) visible = false;
            if (owner && !pOwner.includes(owner)) visible = false;
            if (search && !pName.includes(search)) visible = false;
        } else if (isMilestoneRow) {
            const mStatus = (row.dataset.milestoneStatus || '').toLowerCase();
            const mOwner = (row.dataset.milestoneOwner || '').toLowerCase();
            const mName = (row.dataset.milestoneName || '').toLowerCase();

            if (msStatus) {
                if (mStatus !== msStatus) visible = false;
            }
            if (owner && !mOwner.includes(owner)) visible = false;

            if (search && !mName.includes(search)) {
                const projRow =
                    row.previousElementSibling && row.previousElementSibling.classList.contains('project-row')
                        ? row.previousElementSibling
                        : null;
                const pName = projRow ? (projRow.dataset.projectName || '').toLowerCase() : '';
                if (!pName.includes(search)) visible = false;
            }
        }

        row.style.display = visible ? '' : 'none';
    });
}

// ==========================
// LOAD PROJECTS (your datatables ajax)
// ==========================
function loadProjects() {
    const tbody = $('#projectsMilestonesBody');
    tbody.html(`
      <tr>
        <td colspan="10" class="text-center py-3">
          <span class="text-muted" style="font-size:0.85rem;">
            <i class="fas fa-spinner fa-spin me-2"></i>Loading projects...
          </span>
        </td>
      </tr>
    `);

    const d = {
        draw: 1,
        start: 0,
        length: 1000,
        order: [],
        columns: [],
        showAllUsers: isDashboardAllContent()
    };

    d.status = 'ONGOING, COMPLETED, HOLD';
    d.nodeType = 'roadmap';

    $.ajax({
        url: API_URL,
        type: 'POST',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json',
        data: JSON.stringify(d),
        success: function (response) {
            const data = response.data || response.projects || [];
            dashboardAnalyticsState.projects = data;
            renderAnalyticsDashboard();

            if (!Array.isArray(data) || data.length === 0) {
                tbody.html(`
            <tr>
              <td colspan="10" class="text-center py-3">
                <span class="text-muted" style="font-size:0.85rem;">
                  No projects found.
                </span>
              </td>
            </tr>
          `);
                return;
            }

            let html = '';
            data.forEach(function (project, idx) {
                html += buildProjectRow(project, idx);
            });
            tbody.html(html);
            applyFilters();
        },
        error: function () {
            dashboardAnalyticsState.projects = [];
            renderAnalyticsDashboard();
            tbody.html(`
          <tr>
            <td colspan="10" class="text-center py-3">
              <span class="text-danger" style="font-size:0.85rem;">
                Error loading projects.
              </span>
            </td>
          </tr>
        `);
        }
    });
}

// ==========================
// LOAD MILESTONES PER PROJECT (lazy)
// ==========================
function loadMilestonesForProject(projectId, projectRow) {
    const projectKeyClass = 'project-' + projectId + '-milestones';
    const $projectRow = $(projectRow);
    const $tbody = $('#projectsMilestonesBody');

    // Insert a "loading" row under the project row, remove on success/error
    const loadingRowId = 'loading-ms-' + projectId;
    $projectRow.after(`
      <tr id="${loadingRowId}" class="milestone-row ${projectKeyClass}">
        <td></td>
        <td colspan="9" class="text-center py-2">
          <span class="text-muted" style="font-size:0.8rem;">
            <i class="fas fa-spinner fa-spin me-2"></i>Loading milestones...
          </span>
        </td>
      </tr>
    `);


    const d = {
        draw: 1,
        start: 0,
        length: 1000,
        order: [],
        columns: [],
        showAllUsers: isDashboardAllContent()
    };

    d.status = 'ONGOING, COMPLETED, HOLD';
    d.parentType = 'roadmap';
    d.search = { value: projectId };


    $.ajax({
        url: API_URL,
        type: 'POST',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json',
        data: JSON.stringify(d),
        success: function (data) {
            $('#' + loadingRowId).remove();
            const milestones = data.data;
            const milestoneRows = buildMilestoneRows(projectId, milestones);
            $projectRow.after(milestoneRows.html);
            renderMilestoneOwnerAvatarGroups(milestoneRows.avatarGroups);

            // mark as loaded
            $projectRow.attr('data-milestones-loaded', 'true');

            applyFilters();
        },
        error: function () {
            $('#' + loadingRowId).replaceWith(`
          <tr class="milestone-row ${projectKeyClass}">
            <td></td>
            <td colspan="9" class="text-danger" style="font-size:0.8rem;">
              Error loading milestones for this project.
            </td>
          </tr>
        `);
        }
    });
}



// ==========================
// EVENTS
// ==========================
// Chevron click -> load/toggle milestones
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.btn-toggle-row');
    if (!btn) return;

    // Use the button as the "icon"
    const icon = btn; // This is a <button> with fa-* classes

    const projectId = btn.getAttribute('data-project-id');
    const projectKeyClass = btn.getAttribute('data-ms-class');
    const projectRow = btn.closest('tr');
    const milestonesLoaded = projectRow.getAttribute('data-milestones-loaded') === 'true';
    const $milestoneRows = $('.' + projectKeyClass);

    if (!milestonesLoaded) {
        // First time: load milestones via AJAX
        //icon.classList.remove('fa-chevron-right');
        //icon.classList.add('fa-chevron-down');
        loadMilestonesForProject(projectId, projectRow);

        $(`#${projectId}-chevron`).removeClass('fa-chevron-right');
        $(`#${projectId}-chevron`).addClass('fa-chevron-down');
    } else {
        // Already loaded: toggle show/hide
        const currentlyVisible = $milestoneRows.is(':visible');

        if (currentlyVisible) {
            $milestoneRows.hide();
            //icon.classList.remove('fa-chevron-down');
            //icon.classList.add('fa-chevron-right');

            $(`#${projectId}-chevron`).removeClass('fa-chevron-down');
            $(`#${projectId}-chevron`).addClass('fa-chevron-right');

        } else {
            $milestoneRows.show();
            //icon.classList.remove('fa-chevron-right');
            //icon.classList.add('fa-chevron-down');

            $(`#${projectId}-chevron`).removeClass('fa-chevron-right');
            $(`#${projectId}-chevron`).addClass('fa-chevron-down');
        }
    }
});

// Filter events
document.getElementById('filterProjectStatus').addEventListener('change', applyFilters);
document.getElementById('filterMilestoneStatus').addEventListener('change', applyFilters);
document.getElementById('filterOwner').addEventListener('input', applyFilters);
document.getElementById('filterSearch').addEventListener('input', applyFilters);

    if (dashboardScopeToggle) {
        dashboardScopeToggle.addEventListener('change', async function () {
            try {
                await saveDashboardScopePreference(dashboardScopeToggle.checked);
                fetchRenderCounter();
                loadProjects();
            } catch (error) {
                dashboardScopeToggle.checked = !dashboardScopeToggle.checked;
                console.error(error);
            }
        });
    }

    loadProjects();
});
