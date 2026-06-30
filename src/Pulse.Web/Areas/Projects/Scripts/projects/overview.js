// =========================
// Constants & Globals
// =========================

const API_URL = getApiRootPath() + '/api/projects/datatables';
const limitCount = 7; // used in mapStatusToLabel



let project;
let projectNodes;
let autoCloseTask = false;
let _saveNode = null;
var firstEmptySelect = true;
var selectedUser = {};
// Centralized DOM cache
const DOM = {
    projectNo: document.getElementById('projectNo'),
    plantCode: document.getElementById('plantCode'),
    retrieveLatestRoadmapButton: document.getElementById('btnRetrieveLatestRoadmap'),
    headerTitle: document.getElementById('labelProjectName'),
    headerSubtitle: document.getElementById('labelProjectDescription'),
    headerIcon: document.getElementById('iProjectIcon'),
    projectTitle: document.getElementById('projectTitle'),
    projectDescription: document.getElementById('projectDescription'),
    productgroupSelect: document.getElementById('productgroupSelect'),
    productdivisionSelect: document.getElementById('productdivisionSelect'),
    projectForm: document.getElementById('projectForm'),
    projectmemberForm: document.getElementById('projectmemberForm'),
    transactionKey: document.getElementById('transactionKey'),
    projectName: document.getElementById('projectName'),
    projectIcon: document.getElementById('projectIcon'),
    projectColor: document.getElementById('projectColor'),
    ////milestoneContainer: document.querySelector('.project-milestones'),
    ////activitiesContainer: document.getElementById('activitiesContainer'),
    ////loadingMilestones: document.getElementById('loading-milestones'),
    ////loadingTasks: document.getElementById('loading-tasks'),
    ////filterStatus: document.getElementById('filterStatus'),
    ////filterOwner: document.getElementById('filterOwner'),
    ////filterSearch: document.getElementById('filterSearch'),
    ////selectedMilestoneTitle: document.getElementById('selectedMilestoneTitle'),
    ////selectedMilestoneProgress: document.getElementById('selectedMilestoneProgress'),
    ////hierarchyTree: document.getElementById('hierarchyTree'),
    ////chipsContainer: document.getElementById('chips'),
    ////subcontentContainer: document.getElementById('subcontent'),
    ////activeMilestoneCount: document.getElementById('active-milestone-count'),
    ////emptyState: document.getElementById('emptyState')
};




function onAddTaskClick() {
    alert("Floating Add Button clicked!");
}

// =========================
// Status & Badge Helpers
// =========================
// Transferred to common-functions.js


// =========================
// API helpers (async)
// =========================

// Transferred to common-functions.js

function escapeHtml(value) {
    return $('<div>').text(value ?? '').html();
}

function getRequestErrorMessage(xhr, fallbackMessage) {
    if (!xhr) {
        return fallbackMessage;
    }

    if (xhr.responseJSON) {
        return xhr.responseJSON.exceptionMessage || xhr.responseJSON.message || fallbackMessage;
    }

    if (xhr.responseText) {
        return xhr.responseText;
    }

    return fallbackMessage;
}

async function fetchRoadmapRefreshPreview(projectNo) {
    return await $.ajax({
        url: getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/roadmap-structure/preview`,
        type: 'GET'
    });
}

async function applyRoadmapRefreshSelection(projectNo, selectedChangeKeys) {
    return await $.ajax({
        url: getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/roadmap-structure/apply`,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({
            projectNo,
            selectedChangeKeys
        })
    });
}

function buildRoadmapRefreshPreviewMarkup(preview) {
    const items = Array.isArray(preview.items) ? preview.items : [];
    const rows = items.map(item => {
        const changeBadgeClass = item.changeType === 'add' ? 'bg-success-subtle text-success border border-success-subtle' : 'bg-warning-subtle text-warning border border-warning-subtle';
        const typeBadgeClass = item.itemType === 'milestone' ? 'bg-light text-dark border' : 'bg-info-subtle text-info border border-info-subtle';

        return `
            <label class="list-group-item list-group-item-action roadmap-refresh-item py-3">
                <div class="d-flex gap-3 align-items-start">
                    <input class="form-check-input mt-1 roadmap-refresh-item-checkbox"
                           type="checkbox"
                           value="${escapeHtml(item.changeKey)}"
                           ${item.selectedByDefault ? 'checked' : ''}>
                    <div class="flex-grow-1">
                        <div class="d-flex flex-wrap gap-2 align-items-center mb-2">
                            <span class="badge ${changeBadgeClass}">${escapeHtml(item.changeType)}</span>
                            <span class="badge ${typeBadgeClass}">${escapeHtml(item.itemType)}</span>
                            <span class="fw-semibold">${escapeHtml(item.title)}</span>
                        </div>
                        <div class="small text-muted mb-2">${escapeHtml(item.path)}</div>
                        <div class="small">${escapeHtml(item.summary)}</div>
                    </div>
                </div>
            </label>`;
    }).join('');

    return `
        <div class="container-fluid px-0 roadmap-refresh-dialog">
            <div class="alert alert-info mb-3">
                <div class="fw-semibold mb-1">Merge latest roadmap structure into this project</div>
                <div class="small mb-0">
                    This keeps current execution data intact. Missing parent items are auto-included when needed, and dependency links are created automatically for newly added activities.
                </div>
            </div>
            <div class="row g-3 mb-3">
                <div class="col-md-4">
                    <div class="border rounded p-3 h-100 bg-light">
                        <div class="text-muted small text-uppercase">Roadmap</div>
                        <div class="fw-semibold">${escapeHtml(preview.roadmapName || 'Current roadmap')}</div>
                    </div>
                </div>
                <div class="col-md-8">
                    <div class="border rounded p-3 h-100 bg-light">
                        <div class="d-flex flex-wrap gap-3 small">
                            <span><strong>${preview.addedCount || 0}</strong> additions</span>
                            <span><strong>${preview.updatedCount || 0}</strong> updates</span>
                            <span><strong>${preview.dependencyLinkCount || 0}</strong> dependency links</span>
                        </div>
                        <div class="roadmap-refresh-selection-summary text-muted small mt-2"></div>
                    </div>
                </div>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input roadmap-refresh-select-all" type="checkbox" id="roadmapRefreshSelectAll" checked>
                <label class="form-check-label" for="roadmapRefreshSelectAll">Select all previewed changes</label>
            </div>
            <div class="text-danger small d-none roadmap-refresh-validation mb-2">Select at least one change to apply.</div>
            <div class="list-group" style="max-height: 420px; overflow-y: auto;">
                ${rows}
            </div>
        </div>`;
}

function updateRoadmapRefreshDialogState(dialog) {
    const $dialog = $(dialog);
    const $items = $dialog.find('.roadmap-refresh-item-checkbox');
    const $checked = $items.filter(':checked');
    const $selectAll = $dialog.find('.roadmap-refresh-select-all');

    const total = $items.length;
    const selected = $checked.length;

    $dialog.find('.roadmap-refresh-selection-summary').text(`${selected} of ${total} changes selected.`);
    $dialog.find('.roadmap-refresh-validation').toggleClass('d-none', selected > 0);

    if (!$selectAll.length) {
        return;
    }

    const selectAllElement = $selectAll.get(0);
    if (!selectAllElement) {
        return;
    }

    selectAllElement.checked = total > 0 && selected === total;
    selectAllElement.indeterminate = selected > 0 && selected < total;
}

async function openRoadmapRefreshDialog() {
    if (!DOM.retrieveLatestRoadmapButton || !DOM.projectNo) {
        return;
    }

    const projectNo = DOM.projectNo.value;
    const originalHtml = DOM.retrieveLatestRoadmapButton.innerHTML;

    DOM.retrieveLatestRoadmapButton.disabled = true;
    DOM.retrieveLatestRoadmapButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Loading preview...';

    try {
        const preview = await fetchRoadmapRefreshPreview(projectNo);
        const items = Array.isArray(preview.items) ? preview.items : [];

        if (!items.length) {
            bootbox.alert({
                title: 'Retrieve latest roadmap structure',
                message: 'No roadmap structure changes are available for this project right now.'
            });
            return;
        }

        const dialog = bootbox.dialog({
            title: 'Retrieve latest roadmap structure',
            message: buildRoadmapRefreshPreviewMarkup(preview),
            size: 'large',
            buttons: {
                cancel: {
                    label: 'Cancel',
                    className: 'btn-outline-secondary'
                },
                confirm: {
                    label: 'Apply selected changes',
                    className: 'btn-primary',
                    callback: function () {
                        const $dialog = $(dialog);
                        const selectedChangeKeys = $dialog.find('.roadmap-refresh-item-checkbox:checked')
                            .map(function () { return this.value; })
                            .get();

                        if (!selectedChangeKeys.length) {
                            updateRoadmapRefreshDialogState(dialog);
                            return false;
                        }

                        const $confirmButton = $dialog.find('.btn-primary');
                        $confirmButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Applying...');

                        applyRoadmapRefreshSelection(projectNo, selectedChangeKeys)
                            .then(result => {
                                dialog.modal('hide');

                                const summary = [
                                    result.addedMilestones ? `${result.addedMilestones} milestone${result.addedMilestones === 1 ? '' : 's'} added` : '',
                                    result.updatedMilestones ? `${result.updatedMilestones} milestone${result.updatedMilestones === 1 ? '' : 's'} updated` : '',
                                    result.addedActivities ? `${result.addedActivities} activit${result.addedActivities === 1 ? 'y' : 'ies'} added` : '',
                                    result.updatedActivities ? `${result.updatedActivities} activit${result.updatedActivities === 1 ? 'y' : 'ies'} updated` : '',
                                    result.addedDependencyLinks ? `${result.addedDependencyLinks} dependency link${result.addedDependencyLinks === 1 ? '' : 's'} added` : ''
                                ].filter(Boolean).join(', ');

                                toastr.success(summary || 'Roadmap structure merged successfully.');
                                window.location.reload();
                            })
                            .catch(xhr => {
                                $confirmButton.prop('disabled', false).text('Apply selected changes');
                                toastr.error(getRequestErrorMessage(xhr, 'Unable to merge the roadmap structure.'));
                            });

                        return false;
                    }
                }
            }
        });

        dialog.on('shown.bs.modal', function () {
            updateRoadmapRefreshDialogState(dialog);
        });

        dialog.on('change', '.roadmap-refresh-select-all', function () {
            const checked = this.checked;
            $(dialog).find('.roadmap-refresh-item-checkbox').prop('checked', checked);
            updateRoadmapRefreshDialogState(dialog);
        });

        dialog.on('change', '.roadmap-refresh-item-checkbox', function () {
            updateRoadmapRefreshDialogState(dialog);
        });
    } catch (xhr) {
        toastr.error(getRequestErrorMessage(xhr, 'Unable to load roadmap changes preview.'));
    } finally {
        DOM.retrieveLatestRoadmapButton.disabled = false;
        DOM.retrieveLatestRoadmapButton.innerHTML = originalHtml;
    }
}


// =========================
// Milestone node renderer
// =========================

function milestoneNode(obj, count) {
    const projectNo = obj.projectNo;
    const nodeId = obj.nodeId;
    const nodeType = obj.nodeType;
    const nodeName = obj.nodeName;
    const projectNodeSysId = obj.projectNodeSysId;
    const transactionKey = obj.transactionKey;

    const percentCompleted = calcPercentCompleted({
        total: obj.projectNodeCount,
        completed: obj.projectNodeCompleteCount,
        cancelled: obj.projectNodeCancelCount
    });

    const startDate = obj.projectNodeTargetStartDate ?? obj.projectNodeTargetStart ?? obj.targetStart;
    const endDate = obj.projectNodeTargetCompletionDate ?? obj.projectNodeTargetCompletion ?? obj.targetCompletion;

    const endDateFormatted = formatDateISO(endDate);


    const startDateDataFormatted = formatDateISO(startDate);
    const endDateDataFormatted = formatDateISO(endDate);


    const uiStatus = mapStatusToLabel(
        (nodeId === "__ROOTACTIVITY__" && percentCompleted === 100) ? "COMPLETED" : obj.projectNodeStatus,
        endDate,
        limitCount
    );

    const total = obj.projectNodeCount;
    const completed = obj.projectNodeCompleteCount;
    const cancelled = obj.projectNodeCancelCount;
    const closed = completed + cancelled;
    const ongoing = uiStatus === "Not Started" ? 0 : (total - (cancelled + closed));

    const closedContainer = closed > 0 ? `<span><i class="far fa-circle-check text-success"></i> ${closed} closed</span>` : '';
    const ongoingContainer = ongoing > 0 ? `<span><i class="far fa-clock text-warning"></i> ${ongoing} ongoing</span>` : '';
    const waitingContainer = uiStatus === "Not Started" ? `<span><i class="fas fa-hourglass-half text-default"></i> ${total} waiting</span>` : '';


    let rawOwners = obj.jsonNodeOwners ?? obj.jsonMembers ?? [];
    const owners = Array.isArray(rawOwners)
        ? rawOwners
        : (safeJsonParse(rawOwners, []) || []);


    const ownerlist = owners
        .map(o => {
            const first = o.firstname || o.firstName || '';
            const last = o.lastname || o.lastName || '';
            return `${first} ${last}`.trim();
        })
        .filter(x => x.length > 0)
        .join(', ');

    const ownerIdArray = owners
        .map(o => o.userid || o.userId || o.EmployeeId || '')
        .filter(Boolean);

    const isOwned = ownerIdArray.includes(user ?.EmployeeId); // FIX: true if user is ANY owner



    const nodeLocked = obj.projectNodeStatus === "NOT STARTED";

    const unlockButton = nodeLocked ? `
        <button id="btnUnlockSubmit" name="btnUnlockSubmit" class="btn p-0 border-0 bg-transparent icon-btn text-sm btn-unlock disabled">
             <i class="fa fa-lock"></i>
        </button>
<span id="unlockProcessing" class="ms-2 text-muted d-none">
    <i class="fas fa-spinner fa-spin me-1"></i>Processing...
</span>
`: '';





    return `
        <div class="milestone-item"
             data-project-node-sys-id="${projectNodeSysId}"
             data-transaction-key="${transactionKey}"
             data-milestone-id="${nodeId}"
             data-milestone-type="${nodeType}"
             data-milestone-name="${nodeName}"
             data-milestone-project-no="${projectNo}"
             data-milestone-progress="${percentCompleted}"
             data-milestone-start="${startDateDataFormatted}"
             data-milestone-end="${endDateDataFormatted}"
             data-milestone-owners="${ownerlist}"
             data-milestone-owner-ids="${ownerIdArray}"
             data-milestone-is-locked="${nodeLocked}"
>
            <div class="d-flex justify-content-between align-items-center mb-1">
                <div>
                    <div class="milestone-small-label">Milestone ${count}</div>
                    <div class="milestone-title">${unlockButton} ${nodeName}</div> 
                </div>
                <div class="text-end">
                    <span class="badge bg-light text-dark border">
                        <i class="far fa-calendar-alt me-1"></i>Due ${endDateFormatted}
                    </span>
                </div>
            </div>
            <div class="d-flex align-items-center mb-1">
                <div class="flex-grow-1 me-2">
                    <div class="progress" style="height: 6px;">
                        <div class="progress-bar bg-primary" style="width: ${percentCompleted}%;"></div>
                    </div>
                </div>
                <div class="milestone-dates" style="padding-right:5px">${percentCompleted}%</div>  
            </div>
            <div class="milestone-summary">
                ${closedContainer}
                ${ongoingContainer}
                ${waitingContainer}
            </div>
            <div class="mt-1">
                ${projectnodeStatusBadge(uiStatus)}
            </div>
        </div>`;
}


// =========================
// Task node renderer (async)
// =========================

async function taskNode(obj) {
    const projectNo = obj.projectNo;
    const nodeId = obj.nodeId;
    const nodeType = obj.nodeType;
    const nodeName = obj.nodeName;
    const projectNodeSysId = obj.projectNodeSysId;
    const parentNodeId = obj.parentSysId;
    const parentNodeType = obj.parentType;
    const transactionKey = obj.transactionKey;
    const isresched = obj.projectNodeIsResched === 1;
    const startDate = obj.projectNodeTargetStartDate ?? obj.projectNodeTargetStart ?? obj.targetStart;
    const endDate = obj.projectNodeTargetCompletionDate ?? obj.projectNodeTargetCompletion ?? obj.targetCompletion;

    const startDateFormatted = formatDate(startDate);
    const startDateDataFormatted = formatDateISO(startDate);
    const endDateFormatted = formatDate(endDate);
    const endDateDataFormatted = formatDateISO(endDate);




    //islocked = item.dataset.islocked === "true";

    const parentIsLocked = document.querySelector(`.milestone-item.active`).dataset.milestoneIsLocked === "true";

    const nodeStatus = mapStatusToLabel(obj.projectNodeStatus, endDate, limitCount);


    let closureDateElement = "";
    if (obj.projectNodeActualCompletionDate) {
        const formattedCompletionDate = formatDate(obj.projectNodeActualCompletionDate, "MMM DD, YYYY HH:mm");
        closureDateElement = `<span class="mx-1">•</span> <i class="fa fa-circle-check me-1" title="Date Closed" ></i> ${formattedCompletionDate}`;
    }

    const percentCompleted = calcPercentCompleted({
        total: obj.projectNodeCount,
        completed: obj.projectNodeCompleteCount,
        cancelled: obj.projectNodeCancelCount
    });

    // ---- robust owner parsing + ownership check ----
    let rawOwners = obj.jsonNodeOwners ?? obj.jsonMembers ?? [];
    const owners = Array.isArray(rawOwners)
        ? rawOwners
        : (safeJsonParse(rawOwners, []) || []);

    const ownerlist = owners
        .map(o => {
            const first = o.firstname || o.firstName || '';
            const last = o.lastname || o.lastName || '';
            return `${first} ${last}`.trim();
        })
        .filter(x => x.length > 0)
        .join(', ');

    const ownerIdArray = owners
        .map(o => o.userid || o.userId || o.EmployeeId || '')
        .filter(Boolean);

    const owneridlist = ownerIdArray.join(',');

    const isOwned = ownerIdArray.includes(user ?.EmployeeId); // FIX: true if user is ANY owner

    // prerequisites
    const prerequisitesObj = safeJsonParse(obj.prerequisitesJson, { prerequisites: [] });
    const prerequisites = prerequisitesObj ?.prerequisites || [];
    let chipPrerequisites = "";
    let pendingPrerequisites = 0;

    prerequisites.forEach(p => {
        const parts = p.path.split("/");
        const cleaned = parts.map(p => p.trim()).filter(Boolean);
        //const result = cleaned.slice(1).join(" / ");
        const result = cleaned[0];

        chipPrerequisites += `<span class="chip ${p.status === "COMPLETED" ? "text-bg-success" : ""}">${result}</span>`;
        pendingPrerequisites += p.status === "COMPLETED" ? 1 : 0;
    });

    const prerequisiteElement = prerequisites.length > 0
        ? `<div class="mb-2"><small>Prerequisites:</small> ${chipPrerequisites}</div>`
        : "";

    const isRequired = `<span class="badge bg-warning text-dark">Required</span>`;


    const allowUpdate = parentIsLocked ? false : !isOwned ? false : true;

    const isRescheduled = isresched ? `<span data-entity-id="${nodeId}" data-entity-type="${nodeType}" class="badge bg-info text-dark">Rescheduled</span>` : '';



    const nodeNameDeleted = `<del>${nodeName}</del>`;

    // ---- forms linked to this node ----
    const validForms = await fetchProjectDataAsync(projectNo, nodeType, nodeId);

    let formObj;
    let fields = [];
    validForms.forEach(f => {
        try {
            formObj = JSON.parse(f.formJson);
        } catch (e) {
            bootbox.alert("Invalid form JSON!");
            return;
        }
        formObj.formSysId = f.formSysId;
        formObj.formEntityLinkSysId = f.formEntityLinkSysId;

        if (Array.isArray(formObj.fields)) {
            fields.push(...formObj.fields);
        }
    });

    let normalizedFields = fields.map(fld => ({
        ...fld,
        isrequired: fld.isrequired === "true",
        urlIsParameter: fld.urlIsParameter === "true"
    }));

    // Fetch existing submission values for each field
    const submissionFetches = [];

    normalizedFields.forEach(f => {
        const jsonparsed = safeJsonParse(f.values, []);
        if (!Array.isArray(jsonparsed)) return;

        const submissionValue = jsonparsed.find(a =>
            a.entitysysid === nodeId &&
            a.entitytype === nodeType &&
            a.formfieldsysid === f.id
        );

        if (submissionValue && submissionValue.id) {
            submissionFetches.push(
                fetchSubmissionValueAsync(submissionValue.id).then(data => {
                    f.defaultValue = data ?.fieldValue ?? null;
                    f.defaultClobValue = data ?.fieldValueClob ?? null;
                    f.submissionSysId = data ?.submissionSysId ?? "";
                    f.submissionValueSysId = data ?.submissionValueSysId ?? "";
                    f.submissionTransactionKey = data ?.submissionTransactionKey ?? "";
                    f.submissionValueTransactionKey = data ?.transactionKey ?? "";
                })
            );
        }
    });

    await Promise.all(submissionFetches);

    // Build read-only dynamic form for activity card
    const $containerForms = $('#container-render');
    $containerForms.html('');

    $containerForms.dynamicField({
        fields: normalizedFields,
        userCode: "*",
        blankrowsDisplay: "",
        mode: "READONLY",
        displayEmptyMessage: false,
        emptyMessage: 'No additional details required.',
        buildFieldDataAttributes: function (field) {
            return {
                "field-form-field-sys-id": field.id,
                "field-form-sys-id": field.formSysId,
                "field-form-entity-link-sys-id": field.formEntityLinkSysId,
                "field-name": field.name,
                "field-element-sys-id": nodeId,
                "field-element-type": nodeType,
                "field-submission-sys-id": field.submissionSysId,
                "field-submission-transaction-key": field.submissionTransactionKey,
                "field-submission-value-sys-id": field.submissionValueSysId,
                "field-submission-value-transaction-key": field.submissionValueTransactionKey,
                "field-type": field.type,
            };
        }
    });

    const withField = normalizedFields.length > 0;

    const pendingRequiredFields = normalizedFields.filter(a =>
        a.isrequired === true &&
        (
            !a.defaultValue && a.defaultValue !== 0 &&
            !a.defaultClobValue && a.defaultClobValue !== 0
        )
    ).length > 0;

    const completeTaskCheckbox = `
        <input type="checkbox" 
            class="form-check-input chk-complete-task disabled"
            disabled tabindex="-1" aria-disabled="true"
            ${nodeStatus === "Closed" ? "checked" : ""}
        >`;



    return `
        <div class="activity-card"
            id="activity-${nodeId}"
            data-project-no="${projectNo}"
            data-node-id="${nodeId}"
            data-node-type="${nodeType}"
            data-project-node-sys-id="${projectNodeSysId}"
            data-transaction-key="${transactionKey}"
            data-parent-id="${parentNodeId}"
            data-parent-type="${parentNodeType}"
            data-activity-name="${nodeName}"
            data-activity-owner="${owneridlist}"
            data-activity-status="${nodeStatus}"
            data-activity-progress="${percentCompleted}"
            data-activity-start="${startDateDataFormatted}"
            data-activity-end="${endDateDataFormatted}"
            data-pending-prerequisites="${pendingPrerequisites}"
            data-activity-with-field="${withField ? "true" : "false"}" 
            data-activity-with-pending="${pendingRequiredFields ? "true" : "false"}"
        >
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <div class="activity-title">
                        <div class="d-flex align-items-center flex-nowrap">
                            <div class="me-2">
                                ${completeTaskCheckbox}
                            </div>
                            <div class="me-2 node-name">
                                ${nodeStatus === "Closed" ? nodeNameDeleted : nodeName} ${isRequired} ${isRescheduled}
                            </div> 
                        </div>
                    </div>
                    <div class="activity-meta">
                        <i class="far fa-user me-1"></i> ${ownerlist}
                        <span class="mx-1">•</span>
                        <i class="far fa-calendar me-1"></i>${startDateFormatted} → ${endDateFormatted}
                        ${closureDateElement}
                        ${prerequisiteElement}
                    </div>
                </div>
                <div class="text-end">
                    ${taskStatusBadge(nodeStatus)}
                </div>
            </div>
            <div class="d-flex flex-wrap align-items-center mt-2 gap-2">
                <div class="flex-grow-1">
                    <div class="progress" style="height: 6px;">
                        <div class="progress-bar bg-secondary" style="width: ${percentCompleted}%;"></div>
                    </div>
                </div>
                <div class="activity-progress-text">${percentCompleted}% done</div> 
            </div>
            ${withField ? `<div class="div-dashed">${$containerForms.html()}</div>` : ""}
        </div>`;
}


// =========================
// Builder / Page initialization
// =========================

function builderReviewer(id, obj) {
    project = obj[0];
    projectNodes = obj;

    const plantCode = project.plantCode;

    $('#buttonIsActive').text(project.status !== "HOLD" ? 'Deactivate' : 'Activate');
    $('#buttonIsActive').attr('data-action', project.status !== "HOLD" ? 'deactivate' : 'activate');
    $('#buttonIsActive').removeClass('btn-outline-success');
    $('#buttonIsActive').removeClass('btn-outline-secondary');
    $('#buttonIsActive').addClass(`btn-outline-${project.status !== "HOLD" ? 'secondary' : 'success'}`);


    if (DOM.headerTitle) DOM.headerTitle.textContent = project.projectName;
    if (DOM.headerSubtitle) DOM.headerSubtitle.textContent = project.projectDescription;

    if (DOM.headerIcon) {
        DOM.headerIcon.className = project.projectIcon;
        DOM.headerIcon.style.color = project.projectIconColor;
    }

    if (DOM.projectTitle) DOM.projectTitle.value = project.projectName;
    if (DOM.projectDescription) DOM.projectDescription.value = project.projectDescription;
    if (DOM.plantCode) DOM.plantCode.value = project.plantCode;
    if (DOM.transactionKey) DOM.transactionKey.value = project.transactionKey;


    DOM.projectName.value = project.projectName;
    DOM.projectIcon.value = project.projectIcon;
    DOM.projectColor.value = project.projectIconColor;

    // ===== Icon Picker (custom modal with Bootstrap Icons) =====
    const iconModalBackdrop = document.getElementById('iconModalBackdrop');
    const iconGrid = document.getElementById('iconGrid');
    const iconSearchInput = document.getElementById('iconSearchInput');
    const iconCountLabel = document.getElementById('iconCountLabel');
    const iconModalClose = document.getElementById('iconModalClose');
    const iconModalCancel = document.getElementById('iconModalCancel');
    const iconModalApply = document.getElementById('iconModalApply');
    const btnOpenIconPicker = document.getElementById('btnOpenIconPicker');

    let currentIconClass = project.projectIcon;
    let hoveredIconClass = project.projectIcon;

    function openIconModal() {
        iconModalBackdrop.classList.add('show');
        iconSearchInput.value = '';
        renderIconGrid(BOOTSTRAP_ICON_CLASSES);
        markSelectedIcon(currentIconClass);
        updateIconCountLabel(BOOTSTRAP_ICON_CLASSES.length);
        iconSearchInput.focus();
    }

    function closeIconModal() {
        iconModalBackdrop.classList.remove('show');
    }

    function renderIconGrid(list) {
        iconGrid.innerHTML = '';
        list.forEach(cls => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'icon-item';
            btn.dataset.iconClass = cls;
            btn.title = cls.replace('bi ', '');
            btn.innerHTML = '<i class="' + cls + '"></i>';
            btn.addEventListener('click', () => {
                hoveredIconClass = cls;
                markSelectedIcon(cls);
            });
            iconGrid.appendChild(btn);
        });
    }

    function markSelectedIcon(cls) {
        Array.from(iconGrid.querySelectorAll('.icon-item')).forEach(el => {
            el.classList.toggle('selected', el.dataset.iconClass === cls);
        });
    }

    function updateIconCountLabel(count) {
        iconCountLabel.textContent = count + ' icons';
    }

    function filterIcons(term) {
        term = term.trim().toLowerCase();
        if (!term) return BOOTSTRAP_ICON_CLASSES;
        return BOOTSTRAP_ICON_CLASSES.filter(c => c.toLowerCase().includes(term));
    }

    function setIconClass(iconClass) {
        currentIconClass = iconClass;

        document.getElementById('projectIconPreview').className = iconClass;
        document.getElementById('projectIconLabel').textContent = iconClass;
        $('#icon').val(iconClass);
        const headerIcon = document.getElementById('headerIcon');
        headerIcon.className = iconClass + ' text-success';

        //const previewIcon = document.getElementById('previewIcon');
        //previewIcon.className = iconClass;






    }

    btnOpenIconPicker.addEventListener('click', openIconModal);
    iconModalClose.addEventListener('click', closeIconModal);
    iconModalCancel.addEventListener('click', closeIconModal);

    iconModalApply.addEventListener('click', () => {
        if (hoveredIconClass) {
            setIconClass(hoveredIconClass);
            scheduleDraftSave();
        }
        closeIconModal();
    });

    iconModalBackdrop.addEventListener('click', (e) => {
        if (e.target === iconModalBackdrop) {
            closeIconModal();
        }
    });

    iconSearchInput.addEventListener('input', () => {
        const filtered = filterIcons(iconSearchInput.value);
        renderIconGrid(filtered);
        updateIconCountLabel(filtered.length);
        if (filtered.includes(currentIconClass)) {
            markSelectedIcon(currentIconClass);
        }
    });

    // Initialize icon preview
    setIconClass(currentIconClass);

    // ===== Color Picker (Pickr) =====
    const btnColorPicker = document.getElementById('btnColorPicker');
    let currentColorHex = project.projectIconColor;

    function setPrimaryColor(hex) {
        currentColorHex = hex;

        const headerIcon = document.getElementById('headerIcon');
        headerIcon.style.color = hex;
        const previewBadge = document.getElementById('previewBadge');

        $('#iconcolor').val(hex);

    }

    const pickr = Pickr.create({
        el: '#btnColorPicker',
        theme: 'nano',
        default: currentColorHex,
        comparison: false,
        position: 'bottom-middle',
        swatches: [
            '#3b82f6',
            '#22c55e',
            '#f97316',
            '#ec4899',
            '#8b5cf6',
            '#eab308',
            '#f43f5e',
            '#0ea5e9'
        ],
        components: {
            preview: true,
            opacity: false,
            hue: true,
            interaction: {
                hex: true,
                input: true,
                save: true,
                clear: false
            }
        }
    });

    pickr.on('save', (color, instance) => {
        const hex = color.toHEXA().toString();
        setPrimaryColor(hex);
        instance.hide();
        scheduleDraftSave();
    });

    pickr.on('init', instance => {
        const initial = instance.getColor().toHEXA().toString();
        setPrimaryColor(initial);
    });

    $('#projectownerSelect').select2({
        ajax: {
            url: getApiRootPath() + "/api/ActiveDirectory/Search",
            type: "GET",
            data: function (params) {
                var query = {
                    key: params.term
                };
                return query;
            },
            delay: 250,
            processResults: function (data, params) {

                var __formattedData = $.map(data.data, function (obj) {
                    obj.text = obj.firstName + " " + obj.lastName;
                    obj.id = obj.userName;
                    return obj;
                })

                return {
                    results: __formattedData
                };
            }
        },
        cache: true,
        placeholder: 'Search user to add (First Name, Last Name, Username, Email)',
        escapeMarkup: function (m) { return m; },
        allowClear: true,
        minimumInputLength: 3,
        templateResult: formatSelect2Username,
        templateSelection: function (data, container) {
            if (!data.id) return data.text || '';
            if (data.firstName == undefined) return data.text;
            return data.firstName + " " + data.lastName;
        },
        matcher: matchCustom
    });



    $('#productgroupSelect').select2({
        ajax: {
            url: getApiRootPath() + '/api/productgroups',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                data.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.productGroupName.localeCompare(b.productGroupName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: data.map(item => ({
                        id: item.productGroupCode,
                        text: item.productGroupName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select product group --',
        width: '100%'
    });

    $('#productdivisionSelect').select2({
        ajax: {
            url: getApiRootPath() + '/api/productdivisions',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                data.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.productDivisionName.localeCompare(b.productDivisionName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: data.map(item => ({
                        id: item.productDivisionCode,
                        text: item.productDivisionName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select product group --',
        width: '100%'
    });


    var $option = $('<option selected>')
        .val(project.projectOwnerId)
        .text(project.projectOwnerFirstName + " " + project.projectOwnerLastName)
        .attr('data-email', project.projectOwnerEmail)
        .attr('data-firstName', project.projectOwnerFirstName)
        .attr('data-lastName', project.projectOwnerLastName)
        .attr('data-userId', project.projectOwnerId)
        .attr('data-userName', project.projectOwnerUserName)
        .prop('selected', true);

    $('#projectownerSelect').append($option).trigger('change');

    $('#productgroupSelect').append(new Option(project.productGroup.productGroupName, project.productGroup.productGroupCode, true, true)).trigger('change');

    $('#productdivisionSelect').append(new Option(project.productDivision.productDivisionName, project.productDivision.productDivisionCode, true, true)).trigger('change');


    // --------------------------------------------------------------------
    // Project details
    // --------------------------------------------------------------------

    function buildCurrentProjectData() {


        const dataproductgroup = $('#productgroupSelect').select2('data');
        const productGroupCode = dataproductgroup[0].id;

        const dataproductdivision = $('#productdivisionSelect').select2('data');
        const productDivisionCode = dataproductdivision[0].id;


        const title = document.getElementById('projectTitle').value.trim();
        const description = document.getElementById('projectDescription').value.trim();
        const icon = $('#icon').val();
        const iconColor = $('#iconcolor').val();
        const projectNo = $('#projectNo').val();
        const transactionKey = $('#transactionKey').val();


        return {
            projectNo,
            transactionKey,
            title,
            description,
            icon,
            iconColor,
            productGroupCode,
            productDivisionCode
        };

    }


    $('#projectForm').validate({
        ignore: [], // include hidden fields if needed (e.g. Select2 hidden inputs)
        rules: {
            projectTitle: {
                required: true,
                maxlength: 200
            },
            projectDescription: {
                required: true,
                maxlength: 2000
            },
            productgroupSelect: { required: true },
            productdivisionSelect: { required: true }
        },
        messages: {
            projectTitle: {
                required: "",
                maxlength: "Title must not exceed 200 characters."
            },
            projectDescription: {
                required: "",
                maxlength: "Description must not exceed 2000 characters."
            },
            productgroupSelect: { required: "" },
            productdivisionSelect: { required: "" }
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            if (element.hasClass('select2-hidden-accessible')) {
                // place error label after the visible Select2 element
                error.insertAfter(element.next('.select2'));
            } else {
                // suppress label for standard Bootstrap inputs (we use .invalid-feedback)
                return;
            }
        },

        highlight: function (element) {
            const $el = $(element);

            $el.addClass('is-invalid').removeClass('is-valid');

            const $feedback = $el.closest('.form-floating').find('.invalid-feedback');
            if ($feedback.length) {
                $feedback.show();
            }

            if ($el.hasClass('select2-hidden-accessible')) {
                const $container = $el.next('.select2');
                $container.find('.select2-selection').addClass('is-invalid');
            }
        },

        unhighlight: function (element) {
            const $el = $(element);

            $el.removeClass('is-invalid').addClass('is-valid');

            const $feedback = $el.closest('.form-floating').find('.invalid-feedback');
            if ($feedback.length) {
                $feedback.hide();
            }

            if ($el.hasClass('select2-hidden-accessible')) {
                const $container = $el.next('.select2');
                $container.find('.select2-selection').removeClass('is-invalid');
            }
        }
    });

    document.getElementById('projectForm').addEventListener('submit', function (e) {
        e.preventDefault();

        var payload = buildCurrentProjectData()

        //showLoading('Updating project, please wait...');



        $.ajax({
            url: apiPath + '/api/Projects',
            type: 'PUT',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(payload),
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function (result) {
                //hideLoading();

                toastr.success('Project details are successfully updated!')
                //refreshProjects();

                // optional redirect
                // window.location.href = appPath + '/Projects/Details/' + result.projectId;
            },
            error: function (xhr, status, error) {

                hideLoading();


                if (xhr.status == 401) {
                    window.location.href = '/auth/relogin';
                    return false;
                }

                console.error('Error saving project', xhr);
                let msg = 'Error saving project.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg += '\n' + xhr.responseJSON.message;
                }
                alert(msg);
            }
        });


        closeDrawer();
    });



    // --------------------------------------------------------------------
    // MEMBERS LIST
    // --------------------------------------------------------------------
    var userSearch = $('#projectmemberSelect').select2({
        ajax: {
            url: apiPath + "/api/ActiveDirectory/Search",
            type: "GET",
            data: function (params) {
                var query = {
                    key: params.term
                };
                return query;
            },
            delay: 250,
            processResults: function (data, params) {

                var __formattedData = $.map(data.data, function (obj) {
                    obj.text = obj.firstName + " " + obj.lastName;
                    obj.id = obj.userId;
                    return obj;
                })

                return {
                    results: __formattedData
                };
            }
        },
        cache: true,
        placeholder: 'Search user to add (First Name, Last Name, Username, Email)',
        escapeMarkup: function (m) { return m; },
        allowClear: true,
        minimumInputLength: 3,
        templateResult: formatSelect2Username,
        templateSelection: function (data, container) {
            if (!data.id) return data.text || '';


            $(data.element).attr('data-custom-attribute', data.customValue);

            if (data.id) {
                selectedUser.UserName = data.userName;
                selectedUser.UserId = data.id;
                selectedUser.FirstName = data.firstName;
                selectedUser.LastName = data.lastName;
                selectedUser.Email = data.email;
            }
            else {
                return "";
            }
            if (data.firstName == undefined) return data.text;

            return data.firstName + " " + data.lastName;
        },
        matcher: matchCustom
    });

    $('#projectmemberForm').validate({
        // Add your validation rules if needed
        rules: {
            projectmemberSelect: { required: true }
        },
        messages: {
            projectmemberSelect: { required: "" }
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $('[id*=-error]').hide()
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function (form) {

            // Collect data
            var _data = {
                userId: $('#projectmemberSelect').val(),
                projectNo: id,
                user: selectedUser
            };

            // AJAX call
            $.ajax({
                url: apiPath + `/api/projects/${id}/addmember`,
                type: "POST",
                data: JSON.stringify(_data),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {
                    toastr.success('User is successfully registered!')
                    $('#projectmemberSelect').val(null).trigger('change');
                    tableMembers.ajax.reload(null, false);
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Link does not exist!');
                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                            toastr.error('User already linked!');
                        } else {
                            alert('Error: ' + xhr.responseText);
                        }

                    }
                },

            });





            // Prevent default form submission
            return false;
        }
    });

    var tableMembers = $('#tableMembers').DataTable({
        "processing": true, "responsive": true,
        "serverSide": false,
        "ajax": {
            "url": apiPath + `/api/projects/${id}/members`,
            "type": "GET",
            "contentType": "application/json"
        },
        "initComplete": function () {
            document.querySelectorAll(".status-switch .form-check-input").forEach(function (input) {
                input.addEventListener("change", function () {
                    const pill = input.closest("td").querySelector(".status-pill-text");
                    const isOn = input.checked;

                    if (isOn) {
                        pill.textContent = "Allowed";
                        pill.classList.remove("status-pill-off");
                        pill.classList.add("status-pill-on");
                    } else {
                        pill.textContent = "Restricted";
                        pill.classList.remove("status-pill-on");
                        pill.classList.add("status-pill-off");
                    }
                });
            });


        },
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        columnDefs: [

            { targets: [1, 2], className: 'text-center', orderable: false },
        ],
        "language": {
            "search": "",
            "searchPlaceholder": "Search...",
            "emptyTable": "No data found.",
            'processing': '<div>Loading data please wait...  </div>',
            paginate: {
                previous: "«",
                next: "»"
            }
        },
        "columns": [
            {
                "data": "userId",
                "render": function (value, type, data) {
                    return `${data.user.firstName} ${data.user.lastName}`;

                }
            },
            {
                "data": "user.email"
            },
            {
                "data": "userId",
                "render": function (value, type, data) {
                    return `<button data-key="${value}" data-user-id="${data.userId}" data-display-name='${data.user.firstName} ${data.user.lastName}' class="settasks btn btn-sm btn-outline-dark mb-0" type="button">Tasks</button>`;

                }
            },
            {
                "data": "projectMemberSysId",
                "render": function (value, type, data) {
                    return `<button data-key="${value}" data-user-id="${data.userId}" data-display-name='${data.user.firstName} ${data.user.lastName}'  data-transaction-key="${data.transactionKey}"  class="removemember btn btn-sm btn-outline-danger mb-0" type="button">Remove</button>`;

                }
            }
        ]
    });

    $('#tableMembers tbody').on('click', '.removemember', function () {

        const $d = $(this).data();
        // Collect data
        var _data = {
            userId: $d.userId,
            projectMemberSysId: $d.key,
            transactionKey: $d.transactionKey,
            projectNo: id
        };

        // AJAX call
        $.ajax({
            url: apiPath + `/api/projects/${id}/removemember`,
            type: "DELETE",
            data: JSON.stringify(_data),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {
                toastr.success('User is successfully removed!')
                $('#projectmemberSelect').val(null).trigger('change');
                tableMembers.ajax.reload(null, false);
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    toastr.error('Link does not exist!');
                } else {
                    if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                        toastr.error('User already linked!');
                    } else {
                        alert('Error: ' + xhr.responseText);
                    }

                }
            },

        });

    });

    $('#tableMembers tbody').on('click', '.settasks', function () {

        const $d = $(this).data();
        // Collect data
        var _data = {
            userId: $d.userId,
            projectMemberSysId: $d.key,
            displayName: $d.displayName,
            projectNo: id
        };

        openMemberTasksModal(id, $d.userId, $d.displayName);
    });



    // --------------------------------------------------------------------
    // PRODUCT LIST
    // --------------------------------------------------------------------
    let transactAdd = false;
    var productcodesList = $('#productcodesList').DataTable({
        paging: false,
        "initComplete": function () {
            $('.dt-search').appendTo('.card-tools-productcodesList-filter');
        },
        columns: [
            {   // Product Code
                data: 'productCode',
                title: 'Product Code'
            },
            {   // Plant Type (you can also show description via render)
                data: 'plantType',
                title: 'Plant Type',
                render: function (data, type, row) {
                    // Example: show "FE - Front End"
                    return row.plantType + (row.plantTypeDesc ? ' - ' + row.plantTypeDesc : '');
                }
            },
            {   // Product Family
                data: 'productFamily',
                title: 'Product Family',
                render: function (data, type, row) {
                    return row.productFamily + (row.productFamilyDesc ? ' - ' + row.productFamilyDesc : '');
                }
            },
            {   // Macro Package
                data: 'macroPackage',
                title: 'Macro Package',
                render: function (data, type, row) {
                    return row.macroPackage + (row.macroPackageDesc ? ' - ' + row.macroPackageDesc : '');
                }
            },
            {   // Package
                data: 'pack',
                title: 'Package',
                render: function (data, type, row) {
                    return row.pack + (row.packDesc ? ' - ' + row.packDesc : '');
                }
            },
            {   // PLine
                data: 'pLine',
                title: 'Product Line',
                render: function (data, type, row) {
                    return row.pLine + (row.pLineDesc ? ' - ' + row.pLineDesc : '');
                }
            },
            {   // Maturity (simple)
                data: 'maturity',
                title: 'Maturity'
            },
            {   // Action column
                data: null,
                defaultContent:
                    '<button type="button" class="btn btn-sm btn-outline-danger btnDelete">Delete</button>',
                orderable: false,
                searchable: false
            }
        ],
        language: {
            emptyTable: "No product code selected"
        },
        // DOM: we will move the filter into #productcodeslistTools
        dom: 'lrtip',
        paging: false,
        searching: true,
        info: false
    });

    var currentProductCodes = project.productCodes;

    async function getDetails(productcode, plantcode) {
        const encodedProductcode = encodeURIComponent(productcode).replace(/\*/g, "%2A");

        const data = await getDataAsync(getApiRootPath() + `/api/products?productcode=${encodedProductcode}&plantcode=${plantcode}`); // getDataAsync returns a Promise


        var newRow = {
            productCode: data.productCode,
            plantType: data.plantType,
            plantTypeDesc: data.plantTypeDescription,
            productFamily: data.productFamilyCode,
            productFamilyDesc: data.productFamilyDescription,
            macroPackage: data.macroPackageCode,
            macroPackageDesc: data.macroPackageDescription,
            pack: data.packageCode,
            packDesc: data.packageDescription,
            pLine: data.productCode,
            pLineDesc: data.productLine,
            maturity: data.maturityCode
        };

        if ((!data.projectNo) || (project.projectNo === data.projectNo)) {

            if (!transactAdd) {
                productcodesList.row.add(newRow).draw(false);
                return;
            }

            const payload = {
                projectNo: data.projectNo,
                productCode: data.productCode
            };

            const url = getApiRootPath() + `/api/projects/${projectNo}/products/link`;
            return fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
                },
                body: JSON.stringify(payload)
            }).then(res => {
                if (!res.ok) throw new Error("Failed to save changes");

                productcodesList.row.add(newRow).draw(false);
            });

        }
        else {
            alert(`Product Code ${_productCode} already linked to project no: ${data.projectNo}.`);
            return;
        }
    }


    const productCodes = currentProductCodes.split(/[;,|\t ]+/);

    for (const productcode of productCodes) {
        getDetails(productcode, plantCode);
    }




    $('#productcodesList tbody').on('click', '.btnDelete', function () {
        var row = productcodesList.row($(this).closest('tr'));  // DataTables row
        var data = row.data();

        const payload = {
            projectNo: DOM.projectNo.value,
            productCode: data.productCode
        };

        const url = getApiRootPath() + `/api/projects/${DOM.projectNo.value}/products/unlink`;
        return fetch(url, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
                "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
            },
            body: JSON.stringify(payload)
        }).then(res => {
            if (!res.ok) throw new Error("Failed to save changes");

            productcodesList.row($(this).closest('tr')).remove().draw(false);
        });

    });

    $('#btnAddProductCode').on('click', function () {
        var _productCode = $('#textProductCode').val().trim();
        var _plantType = '';
        var _plantTypeDesc = '';
        var _productFamily = '';
        var _productFamilyDesc = '';
        var _macroPackage = '';
        var _macroPackageDesc = '';
        var _pack = '';
        var _packDesc = '';
        var _pLine = '';
        var _pLineDesc = '';
        var _maturity = '';

        // Required + uniqueness check
        if (!_productCode) {
            alert('Product Code is required');
            return;
        }





        const productCodes = _productCode.split(/[;,|\t ]+/);
        var productlinked;
        for (const productcode of productCodes) {
            var skip = false;
            if (productCodeExists(productcode)) {

                productlinked += productcode + ","
                skip = true;
            }

            if (!skip)
                getDetails(productcode, DOM.plantCode.value)
        }

        if (productlinked)
            alert('Product Code "' + productlinked + '" already exists.');

        $('#productcodeInput').val('');

    });

    function productCodeExists(code) {
        var exists = false;
        var normalized = code.toLowerCase();

        productcodesList.column(0).data().each(function (value, index) {
            if (String(value).toLowerCase() === normalized) {
                exists = true;
                return false; // break
            }
        });

        return exists;
    }

    // ================================================================
    // DRAFT: DataTables (productcodesList) helpers
    // ================================================================ 
    function getProductCodesDraftData() {
        const rows = [];
        productcodesList.rows().every(function () {
            const data = this.data();
            rows.push(data);
        });
        return rows;
    }




    ////////const startDate = formatDateISO(project.targetStart);
    ////////const endDate = formatDateISO(project.targetCompletion);
    ////////const formattedModifiedDate = formatDate(project.modifiedDate, "MMM DD, YYYY HH:mm");

    ////////const productCodes = project.productCodes;
    ////////const productDivision = project.productDivision.productDivisionName;
    ////////const productGroup = project.productGroup.productGroupName;

    ////////const nodeStatus = mapStatusToLabel(project.status, endDate, limitCount);

    ////////const percentCompleted = calcPercentCompleted({
    ////////    total: project.projectCount,
    ////////    completed: project.projectCompleteCount,
    ////////    cancelled: project.projectCancelCount
    ////////});

    ////////if (DOM.filterOwner && project.jsonMembers) {
    ////////    const members = safeJsonParse(project.jsonMembers, []) || [];
    ////////    const optionsHtml = members
    ////////        .map(m => {
    ////////            const id = m.userid || m.userId || m.EmployeeId || '';
    ////////            const first = m.firstname || m.firstName || '';
    ////////            const last = m.lastname || m.lastName || '';
    ////////            if (!id) return '';
    ////////            const label = `${first} ${last}`.trim() || id;
    ////////            return `<option value="${id}">${label}</option>`;
    ////////        })
    ////////        .filter(Boolean)
    ////////        .join("");

    ////////    DOM.filterOwner.innerHTML = `<option value="">All owners</option>${optionsHtml}`;
    ////////}

    ////////if (DOM.chipsContainer) {
    ////////    DOM.chipsContainer.innerHTML = `
    ////////        <div class="d-flex flex-wrap gap-2 mb-2">
    ////////            <span class="project-chip">
    ////////                <i class="far fa-user"></i> ${project.projectOwnerFirstName} ${project.projectOwnerLastName}
    ////////            </span>
    ////////            <span class="project-chip">
    ////////                <i class="far fa-calendar-alt"></i> ${startDate} → ${endDate}
    ////////            </span>
    ////////            <span class="project-chip">
    ////////                <i class="fas fa-check-circle text-emerald-400"></i> ${nodeStatus}
    ////////            </span>
    ////////            <span class="project-chip">
    ////////                Product Division: ${productDivision}
    ////////            </span>
    ////////            <span class="project-chip">
    ////////                 Product Group: ${productGroup}
    ////////            </span>
    ////////            <span class="project-chip">
    ////////                 Product Codes: ${productCodes}
    ////////            </span>
    ////////        </div>`;
    ////////}

    ////////if (DOM.subcontentContainer) {
    ////////    DOM.subcontentContainer.innerHTML = `
    ////////        <div class="project-meta-label">Overall Progress</div>
    ////////        <div class="d-flex align-items-center mt-1" style="min-width: 220px;">
    ////////            <div class="flex-grow-1 me-2">
    ////////                <div class="progress" style="height: 8px; background-color: rgba(15,23,42,0.6);">
    ////////                    <div class="progress-bar bg-success" style="width: ${percentCompleted}%;"></div>
    ////////                </div>
    ////////            </div>
    ////////            <div class="project-meta-value fw-semibold">${percentCompleted}%</div>
    ////////        </div>
    ////////        <div class="project-meta-label mt-1">
    ////////            Updated: ${formattedModifiedDate}
    ////////        </div>`;
    ////////}

    ////////const activeMilestonesRaw = projectNodes.filter(m => m.parentType === "roadmap");

    ////////// Sort by orderIndex ascending (adjust if field name / type differs)
    ////////const activeMilestones = activeMilestonesRaw.sort((a, b) => {
    ////////    const oa = Number(a.orderIndex ?? a.orderindex ?? 0);
    ////////    const ob = Number(b.orderIndex ?? b.orderindex ?? 0);
    ////////    return oa - ob;  // ascending
    ////////});

    ////////if (DOM.milestoneContainer) {
    ////////    DOM.milestoneContainer.innerHTML = "";
    ////////    let activeCount = 0;
    ////////    let idx = 0;

    ////////    activeMilestones.forEach(m => {
    ////////        if (m.projectNodeStatus === "ONGOING") activeCount++;
    ////////        DOM.milestoneContainer.insertAdjacentHTML('beforeend', milestoneNode(m, idx++));
    ////////    });

    ////////    if (DOM.activeMilestoneCount) {
    ////////        DOM.activeMilestoneCount.textContent = activeCount;
    ////////    }
    ////////}

    ////////bindGlobalEvents();
    ////////buildHierarchyTree();




}




// =========================
// Task building / Refresh
// =========================

async function buildTaskItems(projectNo, nodeType, nodeId) {
    if (!DOM.milestoneContainer || !DOM.activitiesContainer) return;


    DOM.milestoneContainer.querySelectorAll('.milestone-item').forEach(mi => mi.classList.remove('active'));
    DOM.milestoneContainer.querySelectorAll('.milestone-item').forEach(item => {
        if (item.dataset.milestoneType === nodeType && item.dataset.milestoneId === nodeId) {
            item.classList.add('active');
        }
    });

    DOM.activitiesContainer.innerHTML = '';

    showLoadingTasks();

    try {
        const [node, activeChildrenRaw] = await Promise.all([
            fetchProjectNodeItemAsync(projectNo, nodeType, nodeId),
            fetchProjectNodeChildrenAsync(projectNo, nodeType, nodeId)
        ]);

        const percentCompleted = calcPercentCompleted({
            total: node.projectNodeCount,
            completed: node.projectNodeCompleteCount,
            cancelled: node.projectNodeCancelCount
        });

        if (DOM.selectedMilestoneTitle) {
            DOM.selectedMilestoneTitle.innerHTML =
                '<i class="fas fa-flag-checkered text-primary"></i> ' + node.nodeName;
        }
        if (DOM.selectedMilestoneProgress) {
            DOM.selectedMilestoneProgress.textContent = percentCompleted + '%';
        }

        const activeChildren = activeChildrenRaw.sort((a, b) => {
            const oa = Number(a.orderIndex ?? a.orderindex ?? 0);
            const ob = Number(b.orderIndex ?? b.orderindex ?? 0);
            return oa - ob;  // ascending
        });

        for (const c of activeChildren) {
            const html = await taskNode(c);
            DOM.activitiesContainer.insertAdjacentHTML('beforeend', html);
        }

        buildHierarchyTree();
        applyFilters();
    } catch (err) {
        console.error('Error building task items', err);
    } finally {
        hideLoadingTasks();
    }
}

async function refreshItemNode(projectNo, nodeType, nodeId) {
    if (!DOM.activitiesContainer) return;

    const oldCard = DOM.activitiesContainer.querySelector(
        `.activity-card[data-node-id="${nodeId}"][data-node-type="${nodeType}"]`
    );
    if (!oldCard) return;

    const loadingHTML = `
        <div class="activity-card" id="activity-${nodeId}" data-node-id="${nodeId}" data-node-type="${nodeType}">
            <span class="text-muted" style="font-size:0.8rem;">
                <i class="fas fa-spinner fa-spin me-2"></i>Refreshing activity...
            </span>
        </div>`;
    const loadingCard = htmlToElement(loadingHTML);

    DOM.activitiesContainer.replaceChild(loadingCard, oldCard);

    showLoadingTasks();

    try {
        const obj = await fetchProjectNodeItemAsync(projectNo, nodeType, nodeId);
        const newCardHTML = await taskNode(obj);
        const newCard = htmlToElement(newCardHTML);

        const cardWithLoading = DOM.activitiesContainer.querySelector(
            `.activity-card[data-node-id="${nodeId}"][data-node-type="${nodeType}"]`
        );
        if (cardWithLoading) {
            DOM.activitiesContainer.replaceChild(newCard, cardWithLoading);
        }

        applyFilters();
    } catch (err) {
        console.error('Error refreshing item node', err);
    } finally {
        hideLoadingTasks();
    }
}

async function finalizeAction(autoClose, saveNode) {
    const projectNo = $('#projectNo').val();
    const id = saveNode.dataset.nodeId || '';
    const type = saveNode.dataset.nodeType || '';
    const projectNodeSysId = saveNode.dataset.projectNodeSysId || '';
    const transactionKey = saveNode.dataset.transactionKey || '';

    if (autoClose) {
        await saveTaskStatusAsync(projectNo, projectNodeSysId, id, transactionKey, "COMPLETE");
    }

    await refreshItemNode(projectNo, type, id);
}






// =========================
// Event binding (delegation)
// =========================

function bindGlobalEvents() {
    // Milestone click
    if (DOM.milestoneContainer) {
        DOM.milestoneContainer.addEventListener('click', async function (e) {
            const mi = e.target.closest('.milestone-item');         // the milestone card itself
            if (!mi) return;

            const nodeId = mi.dataset.milestoneId;
            const nodeType = mi.dataset.milestoneType;
            const projectNo = mi.dataset.milestoneProjectNo;

            // Generic milestone click (no unlock button)
            await buildTaskItems(projectNo, nodeType, nodeId);
        });
    }



    if (DOM.filterStatus) DOM.filterStatus.addEventListener('change', applyFilters);
    if (DOM.filterOwner) DOM.filterOwner.addEventListener('change', applyFilters);
    if (DOM.filterSearch) DOM.filterSearch.addEventListener('input', applyFilters);


}



// =========================
// Document ready bootstrap
// =========================

$(document).ready(async function () {
    function bindSidebarStretchToggle() {
        const btnToggle = document.getElementById('btnToggleSidebars');
        const sidebarRow = document.getElementById('sidebarRow');

        if (!btnToggle || !sidebarRow) return;

        btnToggle.addEventListener('click', function () {
            const stretched = sidebarRow.classList.toggle('sidebar-stretched');

            // Optional: change button text/icon
            if (stretched) {
                this.innerHTML = '<i class="fas fa-up-down"></i> Collapse';
            } else {
                this.innerHTML = '<i class="fas fa-up-down"></i> Expand';
            }
        });
    }

    const projectno = $('#projectNo').val();

    if (DOM.retrieveLatestRoadmapButton) {
        DOM.retrieveLatestRoadmapButton.addEventListener('click', openRoadmapRefreshDialog);
    }

    try {

        showLoadingMilestone();

        const projects = await fetchProjectAsync(projectno);
        builderReviewer(projectno, projects);

        ////const attachments = await fetchAttachmentsAsync(projectno);
        ////builderAttachments(attachments);

        ////const comments = await fetchCommentsAsync(projectno);
        ////builderComments(comments);

        ////const notifications = await fetchNotificationsAsync(projectno);
        ////builderNotifications(notifications);

        ////bindSidebarStretchToggle();
    } catch (err) {
        console.error('Error initializing page', err);
    } finally {
        hideLoadingMilestone();
        hideLoadingTasks();
    }


    $('#formChangeStatusProject').submit(function () {


        bootbox.dialog({
            title: $('#buttonIsActive').data('action') == "deactivate" ? 'Put Project on InActive' : 'Put Project on Active',
            message: `
            <form id="holdProjectForm">
                <div class="mb-3">
                    <label for="holdReason" class="form-label">Reason</label>
                    <textarea
                        id="holdReason"
                        class="form-control"
                        rows="4"
                        placeholder="Enter the reason for putting the project on ${$('#buttonIsActive').data('action') == "deactivate" ? 'InActive' : 'Active'}..."
                        required
                    ></textarea>
                    <div id="holdReasonError" class="text-danger mt-2 d-none">
                        Reason is required.
                    </div>
                </div>
            </form>
        `,
            buttons: {
                cancel: {
                    label: 'Cancel',
                    className: 'btn-secondary'
                },
                confirm: {
                    label: 'Confirm Hold',
                    className: 'btn-warning',
                    callback: function () {
                        const reason = document.getElementById('holdReason').value.trim();
                        const errorEl = document.getElementById('holdReasonError');

                        if (!reason) {
                            errorEl.classList.remove('d-none');
                            return false; // prevent dialog from closing
                        }



                        var formData = new FormData();

                        var submitform = {
                            ProjectNo: $('#projectNo').val(),
                            TransactionKey: $('#transactionKey').val(),
                            Reason: reason,
                            IsActive: $('#buttonIsActive').data('action') == "deactivate" ? 0 : 1
                        };

                        formData.append("roadmap", JSON.stringify(submitform));

                        // AJAX call
                        $.ajax({
                            url: getApiRootPath() + '/api/projects/ChangeStatus/' + $('#projectNo').val(),
                            type: 'PUT',
                            data: formData,
                            processData: false,
                            contentType: false,
                            // xhrFields: { withCredentials: true }, //** REMOVED**
                            success: function (d, x) {
                                if ($('#buttonIsActive').text() == 'Deactivate')
                                    toastr.success('Project is successfully deactivated!');
                                else
                                    toastr.success('Project is successfully activated!');


                                $('#buttonIsActive').attr('data-action', $('#buttonIsActive').text() == 'Activate' ? 'deactivate' : 'activate');
                                $('#buttonIsActive').removeClass('btn-outline-success');
                                $('#buttonIsActive').removeClass('btn-outline-secondary');

                                var css = $('#buttonIsActive').text() == 'Activate' ? 'secondary' : 'success'

                                $('#buttonIsActive').addClass(`btn-outline-${css}`);
                                $('#buttonIsActive').text($('#buttonIsActive').text() == 'Activate' ? 'Deactivate' : 'Activate');


                                $('#transactionKey').val(d.transactionKey);
                            },
                            error: function (xhr) {
                                if (xhr.status === 404) {
                                    toastr.error('Project does not exist!');

                                } else {
                                    if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                                        toastr.error('Project already exist!');
                                    } else {
                                        alert('Error: ' + xhr.responseText);
                                    }

                                }
                            },

                        });



                        //bootbox.alert('Project has been placed on hold.');
                    }
                }
            }
        });






        // Prevent default form submission
        return false;
    });

    $('#formDeleteProject').submit(function () {

        bootbox.confirm({
            title: "Confirm Deletion",
            message: "Are you sure you want to delete this Project?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    // Prepare FormData
                    var formData = new FormData();

                    var submitform = {
                        ProjectNo: $('#projectNo').val(),
                        TransactionKey: $('#transactionKey').val()
                    };

                    formData.append("roadmap", JSON.stringify(submitform));

                    // AJAX call
                    $.ajax({
                        url: getApiRootPath() + '/api/projects/' + $('#projectNo').val(),
                        type: 'DELETE',
                        data: formData,
                        processData: false,
                        contentType: false,
                        // xhrFields: { withCredentials: true }, //** REMOVED**
                        success: function () {

                            bootbox.alert({
                                title: "Project Deleted",
                                message: "Project is successfully deleted!",
                                callback: function () {
                                    window.location.href = "/";
                                }
                            });



                        },
                        error: function (xhr) {
                            if (xhr.status === 404) {
                                toastr.error('Project does not exist!');

                            } else {
                                if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                                    toastr.error('Project already exist!');
                                } else {
                                    alert('Error: ' + xhr.responseText);
                                }

                            }
                        },

                    });
                }
            }
        });





        // Prevent default form submission
        return false;
    });
});