// =========================
// Constants & Globals
// =========================

const API_URL = getApiRootPath() + '/api/projects/datatables';
const limitCount = 7; // used in mapStatusToLabel
const REVIEW_MODES = Object.freeze({
    PROJECT: 'project',
    MEMBER: 'member'
});
const MEMBER_SWIMLANE_LANES = Object.freeze([
    { code: 'NOT STARTED', label: 'Not Started' },
    { code: 'ONGOING', label: 'Ongoing' },
    { code: 'COMPLETED', label: 'Completed' },
    { code: 'HOLD', label: 'On-Hold' },
    { code: 'CANCELLED', label: 'Cancelled' },
    { code: 'ARCHIVED', label: 'Archived' }
]);

let project;
let projectNodes;
let allDetailsAttachments = [];
let allDetailsComments = [];
let currentReviewMode = REVIEW_MODES.PROJECT;
let selectedMemberId = '';
let memberTaskSearch = '';
let memberSwimlaneRenderToken = 0;
const taskFormMarkupCache = new Map();
let autoCloseTask = false;
let _saveNode = null;

// Centralized DOM cache
const DOM = {
    reviewShell: document.querySelector('.project-workspace-shell'),
    headerTitle: document.getElementById('headerTitle'),
    headerSubtitle: document.querySelector('.page-header__subtitle'),
    headerIcon: document.getElementById('headerIcon'),
    reviewModeSummary: document.getElementById('reviewModeSummary'),
    reviewModeProjectButton: document.getElementById('btnReviewModeProject'),
    reviewModeMemberButton: document.getElementById('btnReviewModeMember'),
    milestoneContainer: document.querySelector('.project-milestones'),
    activitiesContainer: document.getElementById('activitiesContainer'),
    projectCentricPanel: document.getElementById('projectCentricPanel'),
    memberCentricPanel: document.getElementById('memberCentricPanel'),
    memberFocusList: document.getElementById('memberFocusList'),
    memberSwimlane: document.getElementById('memberSwimlane'),
    memberTaskSearch: document.getElementById('memberTaskSearch'),
    toggleMemberTasksCollapseButton: document.getElementById('btnToggleMemberTasksCollapse'),
    memberSelectionLabel: document.getElementById('memberSelectionLabel'),
    memberSelectionMeta: document.getElementById('memberSelectionMeta'),
    memberEmptyState: document.getElementById('memberEmptyState'),
    loadingMilestones: document.getElementById('loading-milestones'),
    loadingTasks: document.getElementById('loading-tasks'),
    filterStatus: document.getElementById('filterStatus'),
    filterOwner: document.getElementById('filterOwner'),
    filterSearch: document.getElementById('filterSearch'),
    selectedMilestoneTitle: document.getElementById('selectedMilestoneTitle'),
    selectedMilestoneProgress: document.getElementById('selectedMilestoneProgress'),
    hierarchyTree: document.getElementById('hierarchyTree'),
    chipsContainer: document.getElementById('chips'),
    subcontentContainer: document.getElementById('subcontent'),
    activeMilestoneCount: document.getElementById('active-milestone-count'),
    emptyState: document.getElementById('emptyState'),
    projectName: document.getElementById('projectName'),
    projectIcon: document.getElementById('projectIcon'),
    projectColor: document.getElementById('projectColor'),
    toggleActivitiesCollapseButton: document.getElementById('btnToggleActivitiesCollapse')
};

function normalizeDetailsContextType(value) {
    return String(value ?? '').trim().toUpperCase();
}

function normalizeDetailsContextId(value) {
    return String(value ?? '').trim();
}

function normalizeDetailsCollection(items) {
    if (Array.isArray(items)) {
        return items;
    }

    if (items && Array.isArray(items.data)) {
        return items.data;
    }

    if (items && typeof items.length === 'number') {
        return Array.from(items);
    }

    return [];
}

function filterDetailsItemsByContext(items, entityType, entitySysId) {
    const normalizedItems = normalizeDetailsCollection(items);
    const normalizedType = normalizeDetailsContextType(entityType);
    const normalizedId = normalizeDetailsContextId(entitySysId);

    if (!normalizedType || !normalizedId) {
        return normalizedItems.slice();
    }

    return normalizedItems.filter(item => {
        const itemType = normalizeDetailsContextType(item.entityType || item.EntityType);
        const itemId = normalizeDetailsContextId(item.entitySysId || item.EntitySysId);
        return itemType === normalizedType && itemId === normalizedId;
    });
}

function refreshDetailsSupportPanels(entityType, entitySysId) {
    const attachments = filterDetailsItemsByContext(allDetailsAttachments, entityType, entitySysId);
    const comments = filterDetailsItemsByContext(allDetailsComments, entityType, entitySysId);

    builderAttachments(attachments);
    builderComments(comments);
}

function setDetailsSupportContext(entityType, entitySysId) {
    const normalizedType = normalizeDetailsContextType(entityType);
    const normalizedId = normalizeDetailsContextId(entitySysId);
    const projectNo = $('#projectNo').val();

    selectedEntityType = normalizedType || '';
    selectedEntitySysId = normalizedId || '';

    populateFloatingContextSelectors(selectedEntityType || 'PROJECT', selectedEntitySysId || projectNo);

    const valueToApply = (selectedEntityType && selectedEntitySysId)
        ? buildContextValue(selectedEntityType, selectedEntitySysId)
        : '';

    if (valueToApply) {
        const attachmentsSelect = document.getElementById('attachmentsTargetContext');
        const commentsSelect = document.getElementById('commentTargetContext');

        if (attachmentsSelect && Array.from(attachmentsSelect.options).some(option => option.value === valueToApply)) {
            attachmentsSelect.value = valueToApply;
        }

        if (commentsSelect && Array.from(commentsSelect.options).some(option => option.value === valueToApply)) {
            commentsSelect.value = valueToApply;
        }
    }

    refreshDetailsSupportPanels(selectedEntityType, selectedEntitySysId);
}

function escapeReviewHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function decodeReadonlyRichTextHtml(input) {
    let html = String(input ?? '');
    const encodedTagPattern = /&lt;\/?[a-z][\s\S]*&gt;/i;

    if (!encodedTagPattern.test(html)) {
        return html;
    }

    const decoder = document.createElement('textarea');
    decoder.innerHTML = html;
    html = decoder.value;

    if (encodedTagPattern.test(html)) {
        decoder.innerHTML = html;
        html = decoder.value;
    }

    return html;
}

function sanitizeReadonlyRichTextHtml(input) {
    if (!input) {
        return '';
    }

    const template = document.createElement('template');
    template.innerHTML = input;

    const blockedTags = ['script', 'style', 'iframe', 'object', 'embed', 'form', 'input', 'button', 'textarea', 'select', 'option', 'link', 'meta'];
    blockedTags.forEach(function (tag) {
        template.content.querySelectorAll(tag).forEach(function (node) {
            node.remove();
        });
    });

    template.content.querySelectorAll('*').forEach(function (node) {
        Array.from(node.attributes).forEach(function (attr) {
            const attrName = String(attr.name || '').toLowerCase();
            const attrValue = attr.value || '';

            if (attrName.startsWith('on')) {
                node.removeAttribute(attr.name);
                return;
            }

            if ((attrName === 'href' || attrName === 'src') && /^\s*javascript:/i.test(attrValue)) {
                node.removeAttribute(attr.name);
            }
        });
    });

    return template.innerHTML;
}

function renderReadonlyRichTextAsContainedHtml($container) {
    if (!$container || !$container.length) {
        return;
    }

    $container.find('[data-field-type="richtext"]').each(function () {
        const $field = $(this);
        const $textarea = $field.find('textarea').first();
        const editorHtml = $field.find('.note-editable').first().html() || '';
        const rawHtml = ($textarea.length ? $textarea.val() : '') || editorHtml;
        const sanitizedHtml = sanitizeReadonlyRichTextHtml(decodeReadonlyRichTextHtml(rawHtml));

        $field.find('.note-editor').remove();
        $textarea.remove();

        const $display = $('<div class="project-richtext-readonly comment-text-rich"></div>');
        $display.html(sanitizedHtml);
        $field.append($display);
    });
}

function normalizeMemberId(value) {
    return String(value ?? '').trim();
}

function buildMemberInitials(name) {
    const parts = String(name ?? '').trim().split(/\s+/).filter(Boolean);
    if (!parts.length) {
        return 'NA';
    }

    return parts.slice(0, 2).map(part => part.charAt(0).toUpperCase()).join('');
}

function normalizeReviewMember(member) {
    if (!member) {
        return null;
    }

    const id = normalizeMemberId(member.userid || member.userId || member.EmployeeId || member.id);
    if (!id) {
        return null;
    }

    const first = member.firstname || member.firstName || '';
    const last = member.lastname || member.lastName || '';
    const name = `${first} ${last}`.trim() || id;

    return {
        id,
        name,
        initials: buildMemberInitials(name),
        avatarUrl: `/Settings/Profile/Photo/${encodeURIComponent(id)}`
    };
}

function buildMemberAvatarMarkup(member) {
    return `
        <span class="member-focus-avatar" aria-hidden="true">
            <span class="member-focus-avatar__initials">${escapeReviewHtml(member.initials)}</span>
            <img class="member-focus-avatar__image"
                 src="${escapeReviewHtml(member.avatarUrl || '')}"
                 alt=""
                 loading="lazy"
                 onerror="this.remove();">
        </span>`;
}

function parseReviewMembers(source) {
    if (Array.isArray(source)) {
        return source.map(normalizeReviewMember).filter(Boolean);
    }

    if (typeof source === 'string' && source.trim()) {
        return (safeJsonParse(source, []) || []).map(normalizeReviewMember).filter(Boolean);
    }

    return [];
}

function dedupeReviewMembers(members) {
    const mappedMembers = new Map();

    (members || []).forEach(member => {
        if (!member || !member.id) {
            return;
        }

        const key = normalizeMemberId(member.id).toLowerCase();
        if (!mappedMembers.has(key)) {
            mappedMembers.set(key, member);
        }
    });

    return Array.from(mappedMembers.values());
}

function getNodeOwners(obj) {
    return dedupeReviewMembers(parseReviewMembers(obj ?.jsonNodeOwners ?? obj ?.jsonMembers ?? []));
}

function getProjectReviewMembers() {
    const members = [];

    parseReviewMembers(project ?.jsonMembers ?? []).forEach(member => members.push(member));

    (projectNodes || []).forEach(node => {
        getNodeOwners(node).forEach(member => members.push(member));
    });

    return dedupeReviewMembers(members);
}

function findProjectReviewMember(memberId) {
    const normalizedId = normalizeMemberId(memberId).toLowerCase();
    return getProjectReviewMembers().find(member => normalizeMemberId(member.id).toLowerCase() === normalizedId) || null;
}

function getPreferredMemberId(members) {
    const currentUserId = normalizeMemberId(window.user ?.EmployeeId || window.user ?.userId || window.user ?.userid);
    if (currentUserId && members.some(member => normalizeMemberId(member.id).toLowerCase() === currentUserId.toLowerCase())) {
        return currentUserId;
    }

    return members[0] ?.id || '';
}

function getMilestoneLookup() {
    const lookup = new Map();

    (projectNodes || []).forEach(node => {
        if (node.nodeType === 'milestone' || node.nodeType === 'rootactivity') {
            lookup.set(String(node.nodeId), {
                name: node.nodeName || 'Project backlog',
                orderIndex: Number(node.orderIndex ?? node.orderindex ?? 0)
            });
        }
    });

    return lookup;
}

function getNodeTaskStatus(node) {
    const endDate = node.projectNodeTargetCompletionDate ?? node.projectNodeTargetCompletion ?? node.targetCompletion;
    return mapStatusToLabel(node.projectNodeStatus, endDate, limitCount);
}

function getNodeRawTaskStatus(node) {
    return String(node ?.projectNodeStatus ?? '').trim().toUpperCase();
}

function getMemberLaneStatus(rawStatus, statusLabel) {
    const normalizedStatus = normalizePulseStatus(rawStatus || statusLabel);

    if (normalizedStatus === 'NOT STARTED') return 'NOT STARTED';
    if (normalizedStatus === 'HOLD') return 'HOLD';
    if (normalizedStatus === 'CANCELLED') return 'CANCELLED';
    if (normalizedStatus === 'ARCHIVED') return 'ARCHIVED';
    if (normalizedStatus === 'COMPLETED') return 'COMPLETED';
    return 'ONGOING';
}

function getMemberLaneLabel(laneCode) {
    const normalized = normalizePulseStatus(laneCode);
    const lane = MEMBER_SWIMLANE_LANES.find(item => item.code === normalized);
    return lane ? lane.label : normalized;
}

function matchesMemberTaskSearch(task) {
    const query = memberTaskSearch.trim().toLowerCase();
    if (!query) {
        return true;
    }

    const haystack = [
        task.title,
        task.milestoneName,
        task.ownerNames,
        task.statusLabel,
        task.laneLabel
    ].join(' ').toLowerCase();

    return haystack.includes(query);
}

function buildTaskPrerequisiteSummary(source) {
    const prerequisitesObj = safeJsonParse(source, { prerequisites: [] });
    const prerequisites = prerequisitesObj ?.prerequisites || [];

    if (!prerequisites.length) {
        return {
            chipsHtml: '',
            displayHtml: '',
            incompleteCount: 0
        };
    }

    let incompleteCount = 0;
    const chipsHtml = prerequisites.map(prerequisite => {
        const parts = String(prerequisite ?.path ?? '').split('/');
        const cleaned = parts.map(part => part.trim()).filter(Boolean);
        const label = cleaned.length > 1
            ? cleaned.slice(1).join(' / ')
            : (cleaned[0] || 'Task');
        const isCompleted = String(prerequisite ?.status ?? '').toUpperCase() === 'COMPLETED';

        if (!isCompleted) {
            incompleteCount += 1;
        }

        return `<span class="chip ${isCompleted ? 'text-bg-success' : ''}">${escapeReviewHtml(label)}</span>`;
    }).join('');

    return {
        chipsHtml,
        displayHtml: `<div class="task-prerequisites"><small>Prerequisites:</small> ${chipsHtml}</div>`,
        incompleteCount
    };
}

function getTaskFormCacheKey(projectNo, nodeType, nodeId) {
    return [projectNo, nodeType, nodeId].map(value => String(value ?? '').trim()).join('::');
}

function normalizeFormComparisonId(value) {
    return String(value ?? '').trim();
}

function normalizeFormComparisonType(value) {
    const normalized = String(value ?? '').trim().toLowerCase();
    if (normalized === 'task') {
        return 'activity';
    }

    return normalized;
}

function findSubmissionValueByContext(values, nodeType, nodeIds, fieldId) {
    if (!Array.isArray(values)) {
        return null;
    }

    const normalizedNodeType = normalizeFormComparisonType(nodeType);
    const normalizedFieldId = normalizeFormComparisonId(fieldId);
    const normalizedNodeIds = (Array.isArray(nodeIds) ? nodeIds : [nodeIds])
        .map(normalizeFormComparisonId)
        .filter(Boolean);

    return values.find(value =>
        normalizedNodeIds.includes(normalizeFormComparisonId(value.entitysysid)) &&
        normalizeFormComparisonType(value.entitytype) === normalizedNodeType &&
        normalizeFormComparisonId(value.formfieldsysid) === normalizedFieldId
    );
}

async function getReadonlyTaskFormDetails(projectNo, nodeType, nodeId, alternateNodeIds = []) {
    const cacheKey = getTaskFormCacheKey(projectNo, nodeType, nodeId);
    if (taskFormMarkupCache.has(cacheKey)) {
        return taskFormMarkupCache.get(cacheKey);
    }

    const validForms = await fetchProjectDataAsync(projectNo, nodeType, nodeId, alternateNodeIds);

    let formObj;
    const fields = [];
    validForms.forEach(form => {
        try {
            formObj = JSON.parse(form.formJson);
        } catch (e) {
            bootbox.alert('Invalid form JSON!');
            return;
        }

        formObj.formSysId = form.formSysId;
        formObj.formEntityLinkSysId = form.formEntityLinkSysId;

        if (Array.isArray(formObj.fields)) {
            fields.push(...formObj.fields);
        }
    });

    const normalizedFields = fields.map(field => ({
        ...field,
        isrequired: field.isrequired === 'true',
        urlIsParameter: field.urlIsParameter === 'true'
    }));

    const submissionFetches = [];

    normalizedFields.forEach(field => {
        const parsedValues = safeJsonParse(field.values, []);
        if (!Array.isArray(parsedValues)) {
            return;
        }

        const submissionValue = findSubmissionValueByContext(parsedValues, nodeType, [nodeId, ...alternateNodeIds], field.id);

        if (submissionValue && submissionValue.id) {
            submissionFetches.push(
                fetchSubmissionValueAsync(submissionValue.id).then(data => {
                    field.defaultValue = data ?.fieldValue ?? null;
                    field.defaultClobValue = data ?.fieldValueClob ?? null;
                    field.submissionSysId = data ?.submissionSysId ?? '';
                    field.submissionValueSysId = data ?.submissionValueSysId ?? '';
                    field.submissionTransactionKey = data ?.submissionTransactionKey ?? '';
                    field.submissionValueTransactionKey = data ?.transactionKey ?? '';
                })
            );
        }
    });

    await Promise.all(submissionFetches);

    const $containerForms = $('<div></div>');
    $containerForms.dynamicField({
        fields: normalizedFields,
        userCode: '*',
        blankrowsDisplay: '',
        mode: 'READONLY',
        displayEmptyMessage: false,
        emptyMessage: 'No additional details required.',
        buildFieldDataAttributes: function (field) {
            return {
                'field-form-field-sys-id': field.id,
                'field-form-sys-id': field.formSysId,
                'field-form-entity-link-sys-id': field.formEntityLinkSysId,
                'field-name': field.name,
                'field-element-sys-id': nodeId,
                'field-element-type': nodeType,
                'field-submission-sys-id': field.submissionSysId,
                'field-submission-transaction-key': field.submissionTransactionKey,
                'field-submission-value-sys-id': field.submissionValueSysId,
                'field-submission-value-transaction-key': field.submissionValueTransactionKey,
                'field-type': field.type,
            };
        }
    });

    renderReadonlyRichTextAsContainedHtml($containerForms);

    const details = {
        markup: $containerForms.html(),
        withField: normalizedFields.length > 0,
        pendingRequiredFields: normalizedFields.some(field =>
            field.isrequired === true &&
            (!field.defaultValue && field.defaultValue !== 0 && !field.defaultClobValue && field.defaultClobValue !== 0)
        )
    };

    taskFormMarkupCache.set(cacheKey, details);
    return details;
}

function getMemberTasksForReview(memberId) {
    const normalizedMemberId = normalizeMemberId(memberId).toLowerCase();
    if (!normalizedMemberId) {
        return [];
    }

    const milestoneLookup = getMilestoneLookup();

    return (projectNodes || [])
        .filter(node => node.nodeType === 'activity')
        .map(node => {
            const owners = getNodeOwners(node);
            const ownerKeys = owners.map(owner => normalizeMemberId(owner.id).toLowerCase());
            const milestoneInfo = milestoneLookup.get(String(node.parentSysId || node.parentId || node.parentNodeId || '')) || {
                name: 'Project backlog',
                orderIndex: Number.MAX_SAFE_INTEGER
            };
            const startDate = node.projectNodeTargetStartDate ?? node.projectNodeTargetStart ?? node.targetStart;
            const endDate = node.projectNodeTargetCompletionDate ?? node.projectNodeTargetCompletion ?? node.targetCompletion;
            const statusLabel = getNodeTaskStatus(node);
            const rawStatus = getNodeRawTaskStatus(node);
            const prerequisiteSummary = buildTaskPrerequisiteSummary(node.prerequisitesJson);
            const percentCompleted = calcPercentCompleted({
                total: node.projectNodeCount,
                completed: node.projectNodeCompleteCount,
                cancelled: node.projectNodeCancelCount
            });

            return {
                projectNo: node.projectNo || project ?.projectNo || $('#projectNo').val(),
                nodeId: node.nodeId,
                nodeType: node.nodeType,
                title: node.nodeName || 'Untitled task',
                milestoneName: milestoneInfo.name,
                milestoneOrder: milestoneInfo.orderIndex,
                orderIndex: Number(node.orderIndex ?? node.orderindex ?? 0),
                startDate: formatDateISO(startDate),
                endDate: formatDateISO(endDate),
                statusLabel,
                laneStatus: getMemberLaneStatus(rawStatus, statusLabel),
                laneLabel: getMemberLaneLabel(getMemberLaneStatus(rawStatus, statusLabel)),
                prerequisiteMarkup: prerequisiteSummary.displayHtml,
                ownerNames: owners.map(owner => owner.name).join(', ') || 'Unassigned',
                percentCompleted,
                isRequired: String(node.isRequired) === '1' || node.isRequired === 1 || node.isRequired === true,
                ownerKeys
            };
        })
        .filter(task => task.ownerKeys.includes(normalizedMemberId))
        .sort((left, right) => {
            if (left.milestoneOrder !== right.milestoneOrder) {
                return left.milestoneOrder - right.milestoneOrder;
            }

            if (left.orderIndex !== right.orderIndex) {
                return left.orderIndex - right.orderIndex;
            }

            return left.title.localeCompare(right.title);
        });
}

function renderMemberFocusList() {
    if (!DOM.memberFocusList) {
        return [];
    }

    const members = getProjectReviewMembers();
    const taskCountByMember = new Map();

    members.forEach(member => {
        taskCountByMember.set(member.id, getMemberTasksForReview(member.id).length);
    });

    if (!members.length) {
        selectedMemberId = '';
        DOM.memberFocusList.innerHTML = '<div class="member-lane__empty">No team members are assigned to this project yet.</div>';
        return [];
    }

    const normalizedSelectedId = normalizeMemberId(selectedMemberId).toLowerCase();
    const hasSelectedMember = members.some(member => normalizeMemberId(member.id).toLowerCase() === normalizedSelectedId);
    selectedMemberId = hasSelectedMember ? selectedMemberId : getPreferredMemberId(members);

    DOM.memberFocusList.innerHTML = members.map(member => {
        const isActive = normalizeMemberId(member.id).toLowerCase() === normalizeMemberId(selectedMemberId).toLowerCase();
        const taskCount = taskCountByMember.get(member.id) || 0;
        const taskLabel = `${taskCount} task${taskCount === 1 ? '' : 's'}`;

        return `
            <button type="button"
                    class="member-focus-chip ${isActive ? 'is-active' : ''}"
                    data-member-id="${escapeReviewHtml(member.id)}"
                    data-member-name="${escapeReviewHtml(member.name)}"
                    aria-pressed="${isActive ? 'true' : 'false'}">
                ${buildMemberAvatarMarkup(member)}
                <span class="member-focus-copy">
                    <span class="member-focus-name">${escapeReviewHtml(member.name)}</span>
                    <span class="member-focus-count">${escapeReviewHtml(taskLabel)}</span>
                </span>
            </button>`;
    }).join('');

    return members;
}

async function renderMemberSwimlane() {
    if (!DOM.memberSwimlane || !DOM.memberSelectionLabel || !DOM.memberSelectionMeta || !DOM.memberEmptyState) {
        return;
    }

    const renderToken = ++memberSwimlaneRenderToken;

    const activeMember = findProjectReviewMember(selectedMemberId);
    const activeMemberName = activeMember ?.name || 'Team member';
    const tasks = getMemberTasksForReview(selectedMemberId);
    const filteredTasks = tasks.filter(matchesMemberTaskSearch);

    DOM.memberSelectionLabel.innerHTML = `
        <i class="fas fa-user-group text-primary"></i>
        ${escapeReviewHtml(activeMemberName)}`;
    DOM.memberSelectionMeta.textContent = tasks.length
        ? `${filteredTasks.length} of ${tasks.length} task${tasks.length === 1 ? '' : 's'} shown across five lanes`
        : 'No assigned tasks yet';

    DOM.memberEmptyState.classList.toggle('d-none', tasks.length > 0);

    if (!tasks.length) {
        DOM.memberSwimlane.innerHTML = '';
        syncMemberTaskCollapseButton();
        return;
    }

    DOM.memberSwimlane.innerHTML = '<div class="member-lane__empty">Loading tasks...</div>';
    syncMemberTaskCollapseButton();

    const laneSections = await Promise.all(MEMBER_SWIMLANE_LANES.map(async lane => {
        const laneTasks = filteredTasks.filter(task => task.laneStatus === lane.code);
        const laneCards = laneTasks.length
            ? (await Promise.all(laneTasks.map(async task => {
                const formDetails = await getReadonlyTaskFormDetails(task.projectNo, task.nodeType, task.nodeId);

                return `
                <article class="member-task-card">
                    <div class="member-task-card__header">
                        <div class="member-task-card__header-main">
                            <span class="member-task-card__pill"><i class="fas fa-flag-checkered"></i>${escapeReviewHtml(task.milestoneName)}</span>
                            <h3 class="member-task-card__title">${escapeReviewHtml(task.title)}</h3>
                        </div>
                        <button type="button" class="activity-collapse-toggle" aria-label="Toggle member task details" aria-expanded="true">
                            <i class="fas fa-chevron-down"></i>
                        </button>
                    </div>
                    <div class="member-task-card__body">
                        <div class="member-task-card__meta">
                            <span><i class="fas fa-list-check"></i>${escapeReviewHtml(`${task.percentCompleted}% complete`)}</span>
                            <span><i class="far fa-user"></i>${escapeReviewHtml(task.ownerNames)}</span>
                            ${task.isRequired ? '<span><i class="fas fa-circle-exclamation"></i>Required</span>' : ''}
                        </div>
                        <div class="member-task-card__dates">
                            <span><i class="far fa-calendar"></i>${escapeReviewHtml(task.startDate)} -> ${escapeReviewHtml(task.endDate)}</span>
                        </div>
                        ${task.prerequisiteMarkup || ''}
                        ${formDetails.withField ? `<div class="div-dashed">${formDetails.markup}</div>` : ''}
                        <div class="member-task-card__status">${taskStatusBadge(task.statusLabel)}</div>
                    </div>
                </article>`;
            }))).join('')
            : `<div class="member-lane__empty">${memberTaskSearch.trim() ? 'No matching tasks in this lane.' : 'No tasks in this status for the selected member.'}</div>`;

        return `
            <section class="member-lane">
                <header class="member-lane__header">
                    <div class="member-lane__title">${escapeReviewHtml(lane.label)}</div>
                    <span class="member-lane__count">${laneTasks.length}</span>
                </header>
                <div class="member-lane__body" data-lane-status="${escapeReviewHtml(lane.code)}">${laneCards}</div>
            </section>`;
    }));

    if (renderToken !== memberSwimlaneRenderToken) {
        return;
    }

    DOM.memberSwimlane.innerHTML = laneSections.join('');
    syncMemberTaskCollapseButton();
}

function syncReviewModeUI() {
    const isMemberMode = currentReviewMode === REVIEW_MODES.MEMBER;

    if (DOM.reviewShell) {
        DOM.reviewShell.classList.toggle('review-mode-member', isMemberMode);
    }

    if (DOM.projectCentricPanel) {
        DOM.projectCentricPanel.hidden = isMemberMode;
    }

    if (DOM.memberCentricPanel) {
        DOM.memberCentricPanel.hidden = !isMemberMode;
    }

    if (DOM.reviewModeProjectButton) {
        DOM.reviewModeProjectButton.classList.toggle('is-active', !isMemberMode);
        DOM.reviewModeProjectButton.setAttribute('aria-pressed', String(!isMemberMode));
    }

    if (DOM.reviewModeMemberButton) {
        DOM.reviewModeMemberButton.classList.toggle('is-active', isMemberMode);
        DOM.reviewModeMemberButton.setAttribute('aria-pressed', String(isMemberMode));
    }

    if (DOM.reviewModeSummary) {
        DOM.reviewModeSummary.textContent = isMemberMode
            ? 'Inspect one teammate at a time in a read-only task swimlane view.'
            : 'Milestones on the left, activity context on the right, and supporting collaboration tools underneath.';
    }

    if (isMemberMode) {
        renderMemberFocusList();
        renderMemberSwimlane();
    }
}

function setReviewMode(mode, memberId) {
    currentReviewMode = mode === REVIEW_MODES.MEMBER ? REVIEW_MODES.MEMBER : REVIEW_MODES.PROJECT;

    if (memberId) {
        selectedMemberId = memberId;
    }

    if (currentReviewMode === REVIEW_MODES.MEMBER && !selectedMemberId) {
        const members = getProjectReviewMembers();
        selectedMemberId = getPreferredMemberId(members);
    }

    syncReviewModeUI();
}

function bindReviewModeEvents() {
    const projectButton = DOM.reviewModeProjectButton;
    const memberButton = DOM.reviewModeMemberButton;

    if (projectButton && projectButton.dataset.bound !== 'true') {
        projectButton.dataset.bound = 'true';
        projectButton.addEventListener('click', function () {
            setReviewMode(REVIEW_MODES.PROJECT);
        });
    }

    if (memberButton && memberButton.dataset.bound !== 'true') {
        memberButton.dataset.bound = 'true';
        memberButton.addEventListener('click', function () {
            setReviewMode(REVIEW_MODES.MEMBER);
        });
    }

    if (DOM.memberFocusList && DOM.memberFocusList.dataset.bound !== 'true') {
        DOM.memberFocusList.dataset.bound = 'true';
        DOM.memberFocusList.addEventListener('click', function (event) {
            const memberButton = event.target.closest('[data-member-id]');
            if (!memberButton) {
                return;
            }

            selectedMemberId = memberButton.dataset.memberId || '';
            setReviewMode(REVIEW_MODES.MEMBER, selectedMemberId);
        });
    }

    if (DOM.memberTaskSearch && DOM.memberTaskSearch.dataset.bound !== 'true') {
        DOM.memberTaskSearch.dataset.bound = 'true';
        DOM.memberTaskSearch.addEventListener('input', function () {
            memberTaskSearch = this.value || '';
            void renderMemberSwimlane();
        });
    }
}




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
        total: obj.projectNodeCount - 1,
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

    const total = obj.projectNodeCount === 0 ? 0 : obj.projectNodeCount - 1;
    const completed = obj.projectNodeCompleteCount;
    const cancelled = obj.projectNodeCancelCount;
    const closed = completed + cancelled;
    const ongoing = uiStatus === "Not Started" ? 0 : (total - (cancelled + closed));

    const closedContainer = closed > 0 ? `<span><i class="far fa-circle-check text-success"></i> ${closed} closed</span>` : '';
    const ongoingContainer = ongoing > 0 ? `<span><i class="far fa-clock text-warning"></i> ${ongoing} ongoing</span>` : '';
    const waitingContainer = uiStatus === "Not Started" ? `<span><i class="fas fa-hourglass-half text-default"></i> ${total} waiting</span>` : '';


    const owners = getNodeOwners(obj);

    const ownerlist = owners
        .map(o => o.name)
        .filter(x => x.length > 0)
        .join(', ');

    const ownerIdArray = owners
        .map(o => o.id)
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
        total: (obj.projectNodeCount - 1),
        completed: obj.projectNodeCompleteCount,
        cancelled: obj.projectNodeCancelCount
    });

    // ---- robust owner parsing + ownership check ----
    const owners = getNodeOwners(obj);

    const ownerlist = owners
        .map(o => o.name)
        .filter(x => x.length > 0)
        .join(', ');

    const ownerIdArray = owners
        .map(o => o.id)
        .filter(Boolean);

    const owneridlist = ownerIdArray.join(',');

    const isOwned = ownerIdArray.includes(user ?.EmployeeId); // FIX: true if user is ANY owner

    // prerequisites
    const prerequisiteSummary = buildTaskPrerequisiteSummary(obj.prerequisitesJson);
    const pendingPrerequisites = prerequisiteSummary.incompleteCount;
    const prerequisiteElement = prerequisiteSummary.displayHtml;

    const isRequired = `<span class="badge bg-warning text-dark">Required</span>`;


    const allowUpdate = parentIsLocked ? false : !isOwned ? false : true;

    const isRescheduled = isresched ? `<span data-entity-id="${nodeId}" data-entity-type="${nodeType}" class="badge bg-info text-dark">Rescheduled</span>` : '';



    const nodeNameDeleted = `<del>${nodeName}</del>`;

    const formDetails = await getReadonlyTaskFormDetails(projectNo, nodeType, nodeId, [projectNodeSysId]);
    const withField = formDetails.withField;
    const pendingRequiredFields = formDetails.pendingRequiredFields;

    const completeTaskCheckbox = `
        <input type="checkbox" 
            class="form-check-input chk-complete-task disabled"
            disabled tabindex="-1" aria-disabled="true"
            ${nodeStatus === "Completed" ? "checked" : ""}
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
            <div class="activity-card-header">
                <div class="flex-grow-1">
                    <div class="activity-title">
                        <div class="d-flex align-items-center flex-nowrap">
                            <div class="me-2">
                                ${completeTaskCheckbox}
                            </div>
                            <div class="me-2 node-name">
                                ${nodeStatus === "Completed" ? nodeNameDeleted : nodeName} ${isRequired} ${isRescheduled}
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
                <div class="d-flex align-items-start gap-2">
                    <div class="text-end">
                        ${taskStatusBadge(nodeStatus)}
                    </div>
                    <button type="button" class="activity-collapse-toggle" aria-label="Toggle task details" aria-expanded="true">
                        <i class="fas fa-chevron-down"></i>
                    </button>
                </div>
            </div>
            <div class="activity-card-body">
                <div class="d-flex flex-wrap align-items-center mt-2 gap-2">
                    <div class="flex-grow-1">
                        <div class="progress" style="height: 6px;">
                            <div class="progress-bar bg-secondary" style="width: ${percentCompleted}%;"></div>
                        </div>
                    </div>
                    <div class="activity-progress-text">${percentCompleted}% done</div> 
                </div>
                ${withField ? `<div class="div-dashed">${formDetails.markup}</div>` : ""}
            </div>
        </div>`;
}

function toggleActivityCard(container) {
    if (!container) {
        return;
    }

    const isCollapsed = !container.classList.contains('is-collapsed');
    setActivityCardCollapsed(container, isCollapsed);
}

function setActivityCardCollapsed(container, isCollapsed) {
    if (!container) {
        return;
    }

    container.classList.toggle('is-collapsed', isCollapsed);
    const toggleButton = container.querySelector('.activity-collapse-toggle');
    if (toggleButton) {
        toggleButton.setAttribute('aria-expanded', String(!isCollapsed));
    }

    syncActivitiesCollapseButton();
}

function syncActivitiesCollapseButton() {
    const button = DOM.toggleActivitiesCollapseButton;
    if (!button) {
        return;
    }

    const cards = DOM.activitiesContainer
        ? Array.from(DOM.activitiesContainer.querySelectorAll('.activity-card[data-node-id]'))
        : [];

    if (!cards.length) {
        button.disabled = true;
        button.dataset.collapsed = 'false';
        button.innerHTML = '<i class="fas fa-angle-up me-1"></i>Collapse Tasks';
        return;
    }

    button.disabled = false;

    const allCollapsed = cards.every(card => card.classList.contains('is-collapsed'));
    button.dataset.collapsed = String(allCollapsed);
    button.setAttribute('aria-expanded', String(!allCollapsed));
    button.innerHTML = allCollapsed
        ? '<i class="fas fa-angle-down me-1"></i>Expand Tasks'
        : '<i class="fas fa-angle-up me-1"></i>Collapse Tasks';
}

function setAllActivityCardsCollapsed(isCollapsed) {
    if (!DOM.activitiesContainer) {
        return;
    }

    DOM.activitiesContainer.querySelectorAll('.activity-card[data-node-id]').forEach(card => {
        card.classList.toggle('is-collapsed', isCollapsed);
        const toggleButton = card.querySelector('.activity-collapse-toggle');
        if (toggleButton) {
            toggleButton.setAttribute('aria-expanded', String(!isCollapsed));
        }
    });

    syncActivitiesCollapseButton();
}

function bindActivitiesCollapseButton() {
    const button = DOM.toggleActivitiesCollapseButton;
    if (!button || button.dataset.bound === 'true') {
        return;
    }

    button.dataset.bound = 'true';
    button.addEventListener('click', function () {
        const allCollapsed = this.dataset.collapsed === 'true';
        setAllActivityCardsCollapsed(!allCollapsed);
    });

    syncActivitiesCollapseButton();
}

function syncMemberTaskCollapseButton() {
    const button = DOM.toggleMemberTasksCollapseButton;
    if (!button) {
        return;
    }

    const cards = DOM.memberSwimlane
        ? Array.from(DOM.memberSwimlane.querySelectorAll('.member-task-card'))
        : [];

    if (!cards.length) {
        button.disabled = true;
        button.dataset.collapsed = 'false';
        button.innerHTML = '<i class="fas fa-angle-up me-1"></i>Collapse All';
        return;
    }

    button.disabled = false;

    const allCollapsed = cards.every(card => card.classList.contains('is-collapsed'));
    button.dataset.collapsed = String(allCollapsed);
    button.setAttribute('aria-expanded', String(!allCollapsed));
    button.innerHTML = allCollapsed
        ? '<i class="fas fa-angle-down me-1"></i>Expand All'
        : '<i class="fas fa-angle-up me-1"></i>Collapse All';
}

function setAllMemberTaskCardsCollapsed(isCollapsed) {
    if (!DOM.memberSwimlane) {
        return;
    }

    DOM.memberSwimlane.querySelectorAll('.member-task-card').forEach(card => {
        card.classList.toggle('is-collapsed', isCollapsed);
        const toggleButton = card.querySelector('.activity-collapse-toggle');
        if (toggleButton) {
            toggleButton.setAttribute('aria-expanded', String(!isCollapsed));
        }
    });

    syncMemberTaskCollapseButton();
}

function bindMemberTasksCollapseButton() {
    const button = DOM.toggleMemberTasksCollapseButton;
    if (!button || button.dataset.bound === 'true') {
        return;
    }

    button.dataset.bound = 'true';
    button.addEventListener('click', function () {
        const allCollapsed = this.dataset.collapsed === 'true';
        setAllMemberTaskCardsCollapsed(!allCollapsed);
    });

    syncMemberTaskCollapseButton();
}

function bindActivityCardToggles() {
    if (!DOM.activitiesContainer || DOM.activitiesContainer.dataset.toggleBound === 'true') {
        return;
    }

    DOM.activitiesContainer.dataset.toggleBound = 'true';
    DOM.activitiesContainer.addEventListener('click', function (e) {
        const btnToggleCollapse = e.target.closest('.activity-collapse-toggle');
        if (!btnToggleCollapse) {
            return;
        }

        const container = btnToggleCollapse.closest('[data-node-id]');
        toggleActivityCard(container);
    });
}

function bindMemberTaskCardToggles() {
    if (!DOM.memberSwimlane || DOM.memberSwimlane.dataset.toggleBound === 'true') {
        return;
    }

    DOM.memberSwimlane.dataset.toggleBound = 'true';
    DOM.memberSwimlane.addEventListener('click', function (e) {
        const btnToggleCollapse = e.target.closest('.activity-collapse-toggle');
        if (!btnToggleCollapse) {
            return;
        }

        const container = btnToggleCollapse.closest('.member-task-card');
        if (!container) {
            return;
        }

        const isCollapsed = !container.classList.contains('is-collapsed');
        container.classList.toggle('is-collapsed', isCollapsed);
        btnToggleCollapse.setAttribute('aria-expanded', String(!isCollapsed));
        syncMemberTaskCollapseButton();
    });
}


// =========================
// Builder / Page initialization
// =========================

function builderReviewer(id, obj) {
    project = obj[0];
    projectNodes = obj;
    window.projectNodes = projectNodes;




    if (DOM.headerTitle) DOM.headerTitle.textContent = project.projectName;
    if (DOM.headerSubtitle) DOM.headerSubtitle.textContent = project.projectDescription;

    if (DOM.headerIcon) {
        DOM.headerIcon.className = project.projectIcon + " me-2";
        DOM.headerIcon.style.color = project.projectIconColor;
    }

    DOM.projectName.value = project.projectName;
    DOM.projectIcon.value = project.projectIcon;
    DOM.projectColor.value = project.projectIconColor;

    const startDate = formatDateISO(project.targetStart);
    const endDate = formatDateISO(project.targetCompletion);
    const formattedModifiedDate = formatDate(project.modifiedDate, "MMM DD, YYYY HH:mm");

    const productCodes = project.productCodes;
    const productDivision = project.productDivision.productDivisionName;
    const productGroup = project.productGroup.productGroupName;

    const nodeStatus = mapStatusToLabel(project.status, endDate, limitCount);

    const percentCompleted = calcPercentCompleted({
        total: project.projectCount,
        completed: project.projectCompleteCount,
        cancelled: project.projectCancelCount
    });

    if (DOM.filterOwner && project.jsonMembers) {
        const members = safeJsonParse(project.jsonMembers, []) || [];
        const optionsHtml = members
            .map(m => {
                const id = m.userid || m.userId || m.EmployeeId || '';
                const first = m.firstname || m.firstName || '';
                const last = m.lastname || m.lastName || '';
                if (!id) return '';
                const label = `${first} ${last}`.trim() || id;
                return `<option value="${id}">${label}</option>`;
            })
            .filter(Boolean)
            .join("");

        DOM.filterOwner.innerHTML = `<option value="">All owners</option>${optionsHtml}`;
    }

    if (DOM.chipsContainer) {
        DOM.chipsContainer.innerHTML = `
            <div class="d-flex flex-wrap gap-2 mb-2">
                <span class="project-chip">
                    <i class="far fa-user"></i> ${project.projectOwnerFirstName} ${project.projectOwnerLastName}
                </span>
                <span class="project-chip">
                    <i class="far fa-calendar-alt"></i> ${startDate} → ${endDate}
                </span>
                <span class="project-chip">
                    <i class="fas fa-check-circle text-emerald-400"></i> ${nodeStatus}
                </span>
                <span class="project-chip">
                    Product Division: ${productDivision}
                </span>
                <span class="project-chip">
                     Product Group: ${productGroup}
                </span>
                <span class="project-chip">
                     Product Codes: ${productCodes}
                </span>
            </div>`;
    }

    if (DOM.subcontentContainer) {
        DOM.subcontentContainer.innerHTML = `
            <div class="project-meta-label">Overall Progress</div>
            <div class="d-flex align-items-center mt-1" style="min-width: 220px;">
                <div class="flex-grow-1 me-2">
                    <div class="progress" style="height: 8px; background-color: rgba(15,23,42,0.6);">
                        <div class="progress-bar bg-success" style="width: ${percentCompleted}%;"></div>
                    </div>
                </div>
                <div class="project-meta-value fw-semibold">${percentCompleted}%</div>
            </div>
            <div class="project-meta-label mt-1">
                Updated: ${formattedModifiedDate}
            </div>`;
    }

    const activeMilestonesRaw = projectNodes.filter(m => {
        if (m.parentType !== "roadmap") {
            return false;
        }

        if (String(m.nodeType || '').toLowerCase() !== 'rootactivity') {
            return true;
        }

        return projectNodes.some(node =>
            String(node.nodeType || '').toLowerCase() === 'activity' &&
            String(node.parentSysId || '') === String(m.nodeId || '')
        );
    });

    // Sort by orderIndex ascending (adjust if field name / type differs)
    const activeMilestones = activeMilestonesRaw.sort((a, b) => {
        const oa = Number(a.orderIndex ?? a.orderindex ?? 0);
        const ob = Number(b.orderIndex ?? b.orderindex ?? 0);
        return oa - ob;  // ascending
    });

    if (DOM.milestoneContainer) {
        DOM.milestoneContainer.innerHTML = "";
        let activeCount = 0;
        let idx = 0;

        activeMilestones.forEach(m => {
            if (m.projectNodeStatus === "ONGOING") activeCount++;
            DOM.milestoneContainer.insertAdjacentHTML('beforeend', milestoneNode(m, idx++));
        });

        if (DOM.activeMilestoneCount) {
            DOM.activeMilestoneCount.textContent = activeCount;
        }
    }

    bindGlobalEvents();
    bindReviewModeEvents();
    buildHierarchyTree();
    syncReviewModeUI();




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
    syncActivitiesCollapseButton();

    showLoadingTasks();

    try {
        const [node, activeChildren] = await Promise.all([
            fetchProjectNodeItemAsync(projectNo, nodeType, nodeId),
            fetchProjectNodeDescendantsAsync(projectNo, nodeType, nodeId)
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

        for (const c of activeChildren) {
            const html = await taskNode(c);
            DOM.activitiesContainer.insertAdjacentHTML('beforeend', html);
        }

        buildHierarchyTree();
        applyFilters();
        syncActivitiesCollapseButton();
    } catch (err) {
        console.error('Error building task items', err);
    } finally {
        hideLoadingTasks();
    }
}

async function fetchProjectNodeDescendantsAsync(projectNo, nodeType, nodeId, visited) {
    const nextVisited = visited || new Set();
    const nodeKey = `${projectNo || ''}::${nodeType || ''}::${nodeId || ''}`.toLowerCase();

    if (nextVisited.has(nodeKey)) {
        return [];
    }

    nextVisited.add(nodeKey);

    const directChildren = await fetchProjectNodeChildrenAsync(projectNo, nodeType, nodeId);
    const orderedChildren = (directChildren || []).slice().sort((a, b) => {
        const orderA = Number(a.orderIndex ?? a.orderindex ?? 0);
        const orderB = Number(b.orderIndex ?? b.orderindex ?? 0);
        return orderA - orderB;
    });

    const descendants = [];

    for (const child of orderedChildren) {
        descendants.push(child);

        const totalDescendants = Number(child.nodeTotalDescendants ?? child.nodetotaldescendants ?? 0);
        if (totalDescendants > 0) {
            const nestedChildren = await fetchProjectNodeDescendantsAsync(projectNo, child.nodeType, child.nodeId, nextVisited);
            descendants.push(...nestedChildren);
        }
    }

    return descendants;
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
        syncActivitiesCollapseButton();
        if (currentReviewMode === REVIEW_MODES.MEMBER) {
            renderMemberFocusList();
            renderMemberSwimlane();
        }
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
        await saveTaskStatusAsync(projectNo, projectNodeSysId, id, transactionKey, "COMPLETED");
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
            setDetailsSupportContext(nodeType, nodeId);
            await buildTaskItems(projectNo, nodeType, nodeId);
        });
    }

    if (DOM.activitiesContainer && DOM.activitiesContainer.dataset.contextBound !== 'true') {
        DOM.activitiesContainer.dataset.contextBound = 'true';
        DOM.activitiesContainer.addEventListener('click', function (e) {
            const activityCard = e.target.closest('.activity-card[data-node-id]');
            if (!activityCard) {
                return;
            }

            const taskContextId = activityCard.dataset.projectNodeSysId || activityCard.dataset.nodeId;
            setDetailsSupportContext('TASK', taskContextId);
        });
    }

    const attachmentsContextSelect = document.getElementById('attachmentsTargetContext');
    if (attachmentsContextSelect && attachmentsContextSelect.dataset.bound !== 'true') {
        attachmentsContextSelect.dataset.bound = 'true';
        attachmentsContextSelect.addEventListener('change', function () {
            const selected = parseContextValue(this.value);
            setDetailsSupportContext(selected.entityType, selected.entitySysId);
        });
    }

    const commentsContextSelect = document.getElementById('commentTargetContext');
    if (commentsContextSelect && commentsContextSelect.dataset.bound !== 'true') {
        commentsContextSelect.dataset.bound = 'true';
        commentsContextSelect.addEventListener('change', function () {
            const selected = parseContextValue(this.value);
            setDetailsSupportContext(selected.entityType, selected.entitySysId);
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

    function initSidebarNotifications(projectno) {
        if (!window.PulseNotificationManager || !projectno) {
            return;
        }

        window.PulseNotificationManager.init({
            name: 'projectSidebarNotifications',
            entityType: 'PROJECT',
            entitySysId: projectno,
            contextLabel: 'Project',
            listSelector: '#notificationsList',
            emptySelector: '#notificationsEmpty',
            modalSelector: '#notificationEditorModal',
            formSelector: '#notificationEditorForm',
            canAdd: !!(window.projectNotificationPermissions && window.projectNotificationPermissions.canAdd),
            canEdit: !!(window.projectNotificationPermissions && window.projectNotificationPermissions.canEdit),
            canDelete: !!(window.projectNotificationPermissions && window.projectNotificationPermissions.canDelete),
            projectNo: projectno,
            loadingText: 'Loading notifications...',
            emptyText: 'No notifications found.',
            createTitle: 'Add project notification',
            editTitle: 'Edit project notification'
        });
    }

    const projectno = $('#projectNo').val();

    try {

        showLoadingMilestone();

        const projects = await fetchProjectAsync(projectno);
        builderReviewer(projectno, projects);
        populateFloatingContextSelectors('PROJECT', projectno);

        allDetailsAttachments = normalizeDetailsCollection(await fetchAttachmentsAsync(projectno));

        allDetailsComments = normalizeDetailsCollection(await fetchCommentsAsync(projectno));
        setDetailsSupportContext('', '');
        initSidebarNotifications(projectno);

        bindSidebarStretchToggle();
        bindActivityCardToggles();
        bindMemberTaskCardToggles();
        bindActivitiesCollapseButton();
        bindMemberTasksCollapseButton();
    } catch (err) {
        console.error('Error initializing page', err);
    } finally {
        hideLoadingMilestone();
        hideLoadingTasks();
    }
});