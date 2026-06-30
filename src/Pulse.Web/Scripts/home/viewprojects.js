var projectViewAppPath = getAppRootPath();
var projectViewApiPath = getApiRootPath();

const projectViewState = {
    projects: [],
    filteredProjects: [],
    mode: 'overview'
};

const projectViewModeLabels = {
    overview: 'Overview',
    schedule: 'Schedule',
    risk: 'Risk',
    ownership: 'Ownership'
};

function projectViewToNumber(value) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
}

function projectViewEscapeHtml(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function projectViewFormatDate(value) {
    if (!value) {
        return 'Not set';
    }

    const parsed = moment(value);
    return parsed.isValid() ? parsed.format('MMM D, YYYY') : 'Not set';
}

function projectViewParseMembers(rawMembers) {
    if (!rawMembers) {
        return [];
    }

    if (Array.isArray(rawMembers)) {
        return rawMembers;
    }

    try {
        const parsed = JSON.parse(rawMembers);
        return Array.isArray(parsed) ? parsed : [];
    } catch (error) {
        return [];
    }
}

function projectViewCanManageProject(project) {
    const loggedUserId = window.user?.EmployeeId || '';
    if (!loggedUserId) {
        return false;
    }

    if ((project.projectOwnerId || '') === loggedUserId) {
        return true;
    }

    return projectViewParseMembers(project.jsonMembers).some(function (member) {
        const memberId = member.userid || member.userId || member.EmployeeId || '';
        return memberId === loggedUserId;
    });
}

function projectViewNormalizeTaskFilter(project, taskFilter) {
    if (!taskFilter) {
        return true;
    }

    if (taskFilter === 'pending') {
        return projectViewToNumber(project.projectTaskPendingCount) > 0;
    }

    if (taskFilter === 'risk') {
        return projectViewToNumber(project.projectTaskAtRiskCount) > 0;
    }

    if (taskFilter === 'closed-delayed') {
        return projectViewToNumber(project.projectTaskClosedDelayedCount) > 0;
    }

    return true;
}

function projectViewComputeCompletion(project) {
    const total = projectViewToNumber(project.projectCount);
    const closed = projectViewToNumber(project.projectCompleteCount) + projectViewToNumber(project.projectCancelCount);

    if (!total) {
        return 0;
    }

    return Math.max(0, Math.min(100, Math.round((closed / total) * 100)));
}

function projectViewDaysToCompletion(project) {
    if (!project.targetCompletion) {
        return null;
    }

    const endDate = moment(project.targetCompletion).startOf('day');
    if (!endDate.isValid()) {
        return null;
    }

    return endDate.diff(moment().startOf('day'), 'days');
}

function projectViewScheduleTone(daysToCompletion) {
    if (daysToCompletion === null) {
        return 'No target date';
    }

    if (daysToCompletion < 0) {
        return `${Math.abs(daysToCompletion)} days overdue`;
    }

    if (daysToCompletion === 0) {
        return 'Due today';
    }

    return `${daysToCompletion} days remaining`;
}

function projectViewGetModeBody(project) {
    const mode = projectViewState.mode;
    const progress = projectViewComputeCompletion(project);
    const daysToCompletion = projectViewDaysToCompletion(project);
    const pendingCount = projectViewToNumber(project.projectTaskPendingCount);
    const atRiskCount = projectViewToNumber(project.projectTaskAtRiskCount);
    const closedCount = projectViewToNumber(project.projectTaskClosedCount);
    const delayedClosedCount = projectViewToNumber(project.projectTaskClosedDelayedCount);
    const ongoingCount = projectViewToNumber(project.projectOngoingCount);
    const totalNodes = projectViewToNumber(project.projectCount);
    const ownerName = `${project.projectOwnerFirstName || ''} ${project.projectOwnerLastName || ''}`.trim() || 'Unassigned';

    if (mode === 'schedule') {
        return `
            <div class="project-card-section">
                <div class="project-card-section-title">Delivery Pace</div>
                <div class="project-card-progress-row">
                    <div class="project-card-progress-track">
                        <div class="project-card-progress-bar" style="width:${progress}%;"></div>
                    </div>
                    <div class="project-card-progress-value">${progress}%</div>
                </div>
            </div>
            <div class="project-card-section">
                <div class="project-card-section-title">Timeline</div>
                <div class="project-card-timeline">
                    <div class="project-card-timeline-row">
                        <span class="project-card-timeline-label">Start</span>
                        <span>${projectViewEscapeHtml(projectViewFormatDate(project.targetStart))}</span>
                    </div>
                    <div class="project-card-timeline-row">
                        <span class="project-card-timeline-label">Finish</span>
                        <span>${projectViewEscapeHtml(projectViewFormatDate(project.targetCompletion))}</span>
                    </div>
                    <div class="project-card-timeline-row">
                        <span class="project-card-timeline-label">Window</span>
                        <span>${projectViewEscapeHtml(projectViewScheduleTone(daysToCompletion))}</span>
                    </div>
                    <div class="project-card-timeline-row">
                        <span class="project-card-timeline-label">Ongoing Nodes</span>
                        <span>${ongoingCount}</span>
                    </div>
                </div>
            </div>`;
    }

    if (mode === 'risk') {
        return `
            <div class="project-card-section">
                <div class="project-card-section-title">Task Health</div>
                <div class="project-card-risk-grid">
                    <div class="project-card-risk-item is-warning">
                        <div class="project-card-risk-value">${pendingCount}</div>
                        <div class="project-card-risk-label">Pending</div>
                    </div>
                    <div class="project-card-risk-item is-alert">
                        <div class="project-card-risk-value">${atRiskCount}</div>
                        <div class="project-card-risk-label">At Risk</div>
                    </div>
                    <div class="project-card-risk-item is-success">
                        <div class="project-card-risk-value">${delayedClosedCount}</div>
                        <div class="project-card-risk-label">Closed Delayed</div>
                    </div>
                </div>
            </div>
            <div class="project-card-section">
                <div class="project-card-section-title">Signals</div>
                <div class="project-card-insights">
                    <div class="project-card-insight">
                        <div class="project-card-insight-value">${closedCount}</div>
                        <div class="project-card-insight-label">Closed Tasks</div>
                    </div>
                    <div class="project-card-insight">
                        <div class="project-card-insight-value">${projectViewScheduleTone(daysToCompletion)}</div>
                        <div class="project-card-insight-label">Target Pressure</div>
                    </div>
                </div>
            </div>`;
    }

    if (mode === 'ownership') {
        return `
            <div class="project-card-section">
                <div class="project-card-section-title">Ownership</div>
                <div class="project-card-ownership">
                    <div class="project-card-owner-line">
                        <span>Primary Owner</span>
                        <strong>${projectViewEscapeHtml(ownerName)}</strong>
                    </div>
                    <div class="project-card-owner-line">
                        <span>Products</span>
                        <strong>${projectViewEscapeHtml(project.productCodes || 'None')}</strong>
                    </div>
                    <div class="project-card-owner-line">
                        <span>Plant / Category</span>
                        <strong>${projectViewEscapeHtml((project.plantCode || '-') + ' / ' + (project.categoryCode || '-'))}</strong>
                    </div>
                    <div class="project-card-owner-line">
                        <span>Tracked Nodes</span>
                        <strong>${totalNodes}</strong>
                    </div>
                </div>
            </div>
            <div id="project-members-${projectViewEscapeHtml(project.projectNo || '')}" class="project-card-members compact"></div>`;
    }

    return `
        <div class="project-card-dates">
            <div>
                <div class="project-card-date-label">Target Start</div>
                <div class="project-card-date-value">${projectViewEscapeHtml(projectViewFormatDate(project.targetStart))}</div>
            </div>
            <div>
                <div class="project-card-date-label">Target Completion</div>
                <div class="project-card-date-value">${projectViewEscapeHtml(projectViewFormatDate(project.targetCompletion))}</div>
            </div>
        </div>
        <div class="project-card-section">
            <div class="project-card-section-title">Execution Snapshot</div>
            <div class="project-card-insights">
                <div class="project-card-insight">
                    <div class="project-card-insight-value">${progress}%</div>
                    <div class="project-card-insight-label">Completion</div>
                </div>
                <div class="project-card-insight">
                    <div class="project-card-insight-value">${ongoingCount}</div>
                    <div class="project-card-insight-label">Ongoing Nodes</div>
                </div>
            </div>
        </div>
        <div id="project-members-${projectViewEscapeHtml(project.projectNo || '')}" class="project-card-members"></div>`;
}

function projectViewGetFooter(project) {
    const pendingCount = projectViewToNumber(project.projectTaskPendingCount);
    const atRiskCount = projectViewToNumber(project.projectTaskAtRiskCount);
    const closedCount = projectViewToNumber(project.projectTaskClosedCount);
    const delayedClosedCount = projectViewToNumber(project.projectTaskClosedDelayedCount);

    if (projectViewState.mode === 'schedule') {
        return `
            <div class="project-card-stats">
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${projectViewComputeCompletion(project)}%</div>
                    <div class="project-card-stat-label">Progress</div>
                </div>
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${projectViewToNumber(project.projectOngoingCount)}</div>
                    <div class="project-card-stat-label">Ongoing</div>
                </div>
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${pendingCount}</div>
                    <div class="project-card-stat-label">Pending</div>
                </div>
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${atRiskCount}</div>
                    <div class="project-card-stat-label">At Risk</div>
                </div>
            </div>`;
    }

    if (projectViewState.mode === 'ownership') {
        return `
            <div class="project-card-stats">
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${projectViewToNumber(project.projectCount)}</div>
                    <div class="project-card-stat-label">Tracked</div>
                </div>
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${pendingCount}</div>
                    <div class="project-card-stat-label">Pending</div>
                </div>
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${closedCount}</div>
                    <div class="project-card-stat-label">Closed</div>
                </div>
                <div class="project-card-stat">
                    <div class="project-card-stat-value">${delayedClosedCount}</div>
                    <div class="project-card-stat-label">Delayed Close</div>
                </div>
            </div>`;
    }

    return `
        <div class="project-card-stats">
            <div class="project-card-stat">
                <div class="project-card-stat-value">${pendingCount}</div>
                <div class="project-card-stat-label">Pending</div>
            </div>
            <div class="project-card-stat">
                <div class="project-card-stat-value">${atRiskCount}</div>
                <div class="project-card-stat-label">At Risk</div>
            </div>
            <div class="project-card-stat">
                <div class="project-card-stat-value">${closedCount}</div>
                <div class="project-card-stat-label">Closed</div>
            </div>
            <div class="project-card-stat">
                <div class="project-card-stat-value">${delayedClosedCount}</div>
                <div class="project-card-stat-label">Closed Delayed</div>
            </div>
        </div>`;
}

function projectViewSortProjects(projects) {
    const sortedProjects = projects.slice();

    if (projectViewState.mode === 'risk') {
        sortedProjects.sort(function (left, right) {
            const leftScore = (projectViewToNumber(left.projectTaskAtRiskCount) * 100) + (projectViewToNumber(left.projectTaskClosedDelayedCount) * 10) + projectViewToNumber(left.projectTaskPendingCount);
            const rightScore = (projectViewToNumber(right.projectTaskAtRiskCount) * 100) + (projectViewToNumber(right.projectTaskClosedDelayedCount) * 10) + projectViewToNumber(right.projectTaskPendingCount);
            return rightScore - leftScore;
        });
        return sortedProjects;
    }

    if (projectViewState.mode === 'schedule') {
        sortedProjects.sort(function (left, right) {
            const leftDays = projectViewDaysToCompletion(left);
            const rightDays = projectViewDaysToCompletion(right);
            if (leftDays === null && rightDays === null) return 0;
            if (leftDays === null) return 1;
            if (rightDays === null) return -1;
            return leftDays - rightDays;
        });
        return sortedProjects;
    }

    if (projectViewState.mode === 'ownership') {
        sortedProjects.sort(function (left, right) {
            const leftOwner = `${left.projectOwnerFirstName || ''} ${left.projectOwnerLastName || ''}`.trim();
            const rightOwner = `${right.projectOwnerFirstName || ''} ${right.projectOwnerLastName || ''}`.trim();
            return leftOwner.localeCompare(rightOwner);
        });
        return sortedProjects;
    }

    return sortedProjects;
}

function projectViewBuildCard(project) {
    const projectNo = project.projectNo || '';
    const projectName = project.projectName || 'Untitled Project';
    const projectIconName = project.projectIcon || 'bi bi-rocket-takeoff-fill';
    const projectIconColor = project.projectIconColor || '#ffffff';
    const ownerName = `${project.projectOwnerFirstName || ''} ${project.projectOwnerLastName || ''}`.trim() || 'Unassigned';
    const productCodes = project.productCodes || 'No products linked';
    const categoryCode = project.categoryCode || 'No category';
    const plantCode = project.plantCode || 'No plant';
    const statusBadge = getPulseStatusBadge(project.status, {
        targetDate: project.targetCompletion
    });
    const canManageProject = projectViewCanManageProject(project);

    return `
        <article class="project-card" data-project-card data-project-no="${projectViewEscapeHtml(projectNo)}">
            <div class="project-card-header" style="background:radial-gradient(circle at top right, rgba(255,255,255,0.28), transparent 32%), linear-gradient(135deg, ${project.projectIconColor || '#0f766e'} 0%, #0f172a 100%);">
                <div class="project-card-actions">
                    <a href="${projectViewAppPath}/projects/${projectNo}/details" class="project-card-action" title="Details">
                        <i class="bi bi-box-arrow-up-right"></i>
                    </a>
                    ${canManageProject ? `<a href="${projectViewAppPath}/projects/${projectNo}/configure" class="project-card-action" title="Configuration">
                        <i class="bi bi-gear"></i>
                    </a>
                    <a href="${projectViewAppPath}/projects/${projectNo}/review" class="project-card-action" title="Review">
                        <i class="bi bi-ui-checks"></i>
                    </a>` : ''}
                </div>
                <div class="project-card-kicker">Project View</div>
                <div class="project-card-title">
                    <span class="project-card-icon"><i class="${projectViewEscapeHtml(projectIconName)}" style="color:${projectViewEscapeHtml(projectIconColor)}"></i></span>
                    <div>
                        <h3 class="project-card-heading">${projectViewEscapeHtml(projectName)}</h3>
                        <div class="project-card-projectno">${projectViewEscapeHtml(projectNo)}</div>
                    </div>
                </div>
                <div class="project-card-meta">
                    <span class="project-card-chip"><i class="bi bi-box-seam"></i>${projectViewEscapeHtml(productCodes)}</span>
                    <span class="project-card-chip"><i class="bi bi-geo-alt"></i>${projectViewEscapeHtml(plantCode)}</span>
                    <span class="project-card-chip"><i class="bi bi-tag"></i>${projectViewEscapeHtml(categoryCode)}</span>
                </div>
            </div>
            <div class="project-card-body">
                <div class="project-card-status">
                    <div class="project-card-owner"><i class="bi bi-person-badge me-1"></i>${projectViewEscapeHtml(ownerName)}</div>
                    <div>${statusBadge}</div>
                </div>
                ${projectViewGetModeBody(project)}
            </div>
            ${projectViewGetFooter(project)}
        </article>`;
}

function projectViewRenderMembers(project) {
    const projectNo = project.projectNo || '';
    const members = projectViewParseMembers(project.jsonMembers)
        .map(function (member) {
            return {
                id: member.userid || member.userId,
                name: `${member.firstname || member.firstName || ''} ${member.lastname || member.lastName || ''}`.trim(),
                avatarUrl: `/Settings/Profile/Photo/${encodeURIComponent(member.userid || member.userId)}`
            };
        })
        .filter(function (member) {
            return member.id && member.name;
        });

    loadAndRenderAvatarGroup({
        dataSource: members,
        container: `#project-members-${projectNo}`,
        maxVisible: 8,
        avatarSize: 36,
        avatarSpacing: 18,
        label: 'Members',
        emptyText: 'No members assigned',
        backgroundColor: '#e0f2fe',
        fontColor: '#075985',
        labelBackgroundColor: '#0f172a',
        labelFontColor: '#ffffff',
        userInformationUrl: '/Settings/Profile/Index/{id}',
        userInformationTarget: '_self',
        sort: 'initials'
    });
}

function projectViewRenderSummary(projects) {
    const cardCount = projects.length;
    const totalPending = projects.reduce(function (sum, project) {
        return sum + projectViewToNumber(project.projectTaskPendingCount);
    }, 0);
    const totalAtRisk = projects.reduce(function (sum, project) {
        return sum + projectViewToNumber(project.projectTaskAtRiskCount);
    }, 0);
    const totalClosedDelayed = projects.reduce(function (sum, project) {
        return sum + projectViewToNumber(project.projectTaskClosedDelayedCount);
    }, 0);

    $('#projectViewSummary').text(
        `${cardCount} projects in ${projectViewModeLabels[projectViewState.mode] || 'Overview'} mode, ${totalPending} pending tasks, ${totalAtRisk} at risk tasks, ${totalClosedDelayed} closed delayed tasks.`
    );
    $('#projectViewCount').html(`<i class="bi bi-grid-3x3-gap-fill"></i><span>${cardCount} visible</span>`);
}

function projectViewRenderCards() {
    const searchValue = ($('#projectViewSearch').val() || '').trim().toLowerCase();
    const statusValue = ($('#projectViewStatus').val() || '').trim().toUpperCase();
    const taskFilter = ($('#projectViewTaskFilter').val() || '').trim();

    const filteredProjects = projectViewState.projects.filter(function (project) {
        const statusCode = getPulseStatusMeta(project.status, {
            targetDate: project.targetCompletion
        }).code;
        const searchableText = [
            project.projectNo,
            project.projectName,
            project.productCodes,
            project.plantCode,
            project.categoryCode,
            project.projectOwnerFirstName,
            project.projectOwnerLastName
        ].join(' ').toLowerCase();

        if (searchValue && searchableText.indexOf(searchValue) === -1) {
            return false;
        }

        if (statusValue && statusCode !== statusValue) {
            return false;
        }

        return projectViewNormalizeTaskFilter(project, taskFilter);
    });

    const orderedProjects = projectViewSortProjects(filteredProjects);

    projectViewState.filteredProjects = orderedProjects;
    projectViewRenderSummary(orderedProjects);

    if (!orderedProjects.length) {
        $('#projectCardList').html('<div class="project-card-empty">No projects matched the current filters.</div>');
        return;
    }

    $('#projectCardList').html(orderedProjects.map(projectViewBuildCard).join(''));
    orderedProjects.forEach(projectViewRenderMembers);
}

function projectViewSetMode(mode) {
    projectViewState.mode = mode;
    $('[data-view-mode]').removeClass('active');
    $(`[data-view-mode="${mode}"]`).addClass('active');
    projectViewRenderCards();
}

function projectViewFetchProjects() {
    $('#projectCardList').html('<div class="project-card-empty">Loading project cards...</div>');

    return $.ajax({
        url: projectViewApiPath + '/api/projects/datatables',
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
        projectViewState.projects = Array.isArray(response.data) ? response.data : [];
        projectViewRenderCards();
    }).fail(function () {
        projectViewState.projects = [];
        $('#projectViewSummary').text('Unable to load project cards right now.');
        $('#projectViewCount').html('<i class="bi bi-grid-3x3-gap-fill"></i><span>0 visible</span>');
        $('#projectCardList').html('<div class="project-card-empty">Unable to load the project view.</div>');
    });
}

$(document).ready(function () {
    $('#projectViewSearch').on('input', projectViewRenderCards);
    $('#projectViewStatus').on('change', projectViewRenderCards);
    $('#projectViewTaskFilter').on('change', projectViewRenderCards);
    $('[data-view-mode]').on('click', function () {
        const selectedMode = $(this).data('view-mode');
        if (!selectedMode || selectedMode === projectViewState.mode) {
            return;
        }

        projectViewSetMode(selectedMode);
    });

    projectViewFetchProjects();
});