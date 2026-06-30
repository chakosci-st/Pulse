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
const reviewMemberLanePermissions = window.reviewMemberLanePermissions || {};
const hasAdvancedStatusModule = !!reviewMemberLanePermissions.hasAdvancedStatusModule;
const restrictedMemberLaneSet = hasAdvancedStatusModule
    ? new Set()
    : new Set(['CANCELLED', 'ARCHIVED']);

let project;
let projectNodes;
let allReviewAttachments = [];
let allReviewComments = [];
let currentReviewMode = REVIEW_MODES.PROJECT;
let selectedMemberId = '';
let memberTaskSearch = '';
let memberDragTaskId = '';
let isMemberLaneTransitionPending = false;
let memberSwimlaneRenderToken = 0;
let reviewSupportRenderToken = 0;
const taskFormMarkupCache = new Map();

let autoCloseTask = false;
let _saveNode = null;

const modalEl = document.getElementById('modalUpdateActivity');
const modal = new bootstrap.Modal(modalEl);

const modalElForm = document.getElementById('modalUpdateForm');
const modalForm = new bootstrap.Modal(modalElForm);

const modalElUnlock = document.getElementById('modalUnlock');
const modalUnlock = new bootstrap.Modal(modalElUnlock);

const modalElM = document.getElementById('modalUpdateMilestone');
const modalM = new bootstrap.Modal(modalElM);


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

function normalizeReviewContextType(value) {
    return String(value ?? '').trim().toUpperCase();
}

function normalizeReviewContextId(value) {
    return String(value ?? '').trim();
}

function fetchReviewAttachmentsForContextAsync(projectNo, entityType, entitySysId) {
    return $.ajax({
        url: getApiRootPath() + `/api/project/${projectNo}/attachments/${entityType}/${entitySysId}`,
        type: 'GET',
        contentType: 'application/json',
        dataType: 'json'
    }).then(resp => resp.data);
}

function fetchReviewCommentsForContextAsync(projectNo, entityType, entitySysId) {
    return $.ajax({
        url: getApiRootPath() + `/api/comments/${projectNo}/${entityType}/${entitySysId}`,
        type: 'GET',
        contentType: 'application/json',
        dataType: 'json'
    }).then(resp => resp.data);
}

async function refreshReviewSupportPanels(entityType, entitySysId) {
    const normalizedType = normalizeReviewContextType(entityType);
    const normalizedId = normalizeReviewContextId(entitySysId);
    const projectNo = $('#projectNo').val();
    const renderToken = ++reviewSupportRenderToken;

    if (!projectNo) {
        allReviewAttachments = [];
        allReviewComments = [];
        builderAttachments(allReviewAttachments);
        builderComments(allReviewComments);
        return;
    }

    const useProjectContext = !normalizedType || !normalizedId || normalizedType === 'PROJECT';

    try {
        const [attachments, comments] = useProjectContext
            ? await Promise.all([
                fetchAttachmentsAsync(projectNo),
                fetchCommentsAsync(projectNo)
            ])
            : await Promise.all([
                fetchReviewAttachmentsForContextAsync(projectNo, normalizedType, normalizedId),
                fetchReviewCommentsForContextAsync(projectNo, normalizedType, normalizedId)
            ]);

        if (renderToken !== reviewSupportRenderToken) {
            return;
        }

        allReviewAttachments = Array.isArray(attachments) ? attachments : [];
        allReviewComments = Array.isArray(comments) ? comments : [];
    } catch (error) {
        console.error('Error loading review support context', error);
        if (renderToken !== reviewSupportRenderToken) {
            return;
        }

        allReviewAttachments = [];
        allReviewComments = [];
    }

    builderAttachments(allReviewAttachments);
    builderComments(allReviewComments);
}

async function setReviewSupportContext(entityType, entitySysId) {
    const normalizedType = normalizeReviewContextType(entityType);
    const normalizedId = normalizeReviewContextId(entitySysId);
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

    await refreshReviewSupportPanels(selectedEntityType, selectedEntitySysId);
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

function getIncompletePrerequisiteCount(source) {
    const prerequisitesObj = safeJsonParse(source, { prerequisites: [] });
    const prerequisites = prerequisitesObj ?.prerequisites || [];

    return prerequisites.reduce((count, prerequisite) => {
        return count + (String(prerequisite ?.status ?? '').toUpperCase() === 'COMPLETED' ? 0 : 1);
    }, 0);
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

function getRefreshNodeTypeCandidates(nodeType) {
    const normalized = String(nodeType ?? '').trim().toLowerCase();
    const candidates = [normalized];

    if (normalized === 'activity') {
        candidates.push('task');
    } else if (normalized === 'task') {
        candidates.push('activity');
    }

    return candidates;
}

function invalidateTaskFormCacheForNode(projectNo, nodeType, nodeId, alternateNodeIds = []) {
    const nodeIds = [nodeId, ...alternateNodeIds]
        .map(value => String(value ?? '').trim())
        .filter(Boolean);

    if (!nodeIds.length) {
        return;
    }

    const nodeTypeCandidates = getRefreshNodeTypeCandidates(nodeType);
    nodeTypeCandidates.forEach(typeCandidate => {
        nodeIds.forEach(nodeIdCandidate => {
            taskFormMarkupCache.delete(getTaskFormCacheKey(projectNo, typeCandidate, nodeIdCandidate));
        });
    });
}

function getNodeCards(nodeType, nodeId) {
    const selector = `[data-node-id="${nodeId}"][data-node-type="${nodeType}"]`;
    const cards = [];

    if (DOM.activitiesContainer) {
        DOM.activitiesContainer.querySelectorAll(`.activity-card${selector}`).forEach(card => cards.push(card));
    }

    if (DOM.memberSwimlane) {
        DOM.memberSwimlane.querySelectorAll(`.member-task-card${selector}`).forEach(card => cards.push(card));
    }

    return cards;
}

function setNodeRefreshingState(nodeType, nodeId, isRefreshing) {
    const cards = getNodeCards(nodeType, nodeId);
    cards.forEach(card => {
        card.classList.toggle('node-refreshing', !!isRefreshing);

        const existingOverlay = card.querySelector('.node-refresh-overlay');
        if (isRefreshing) {
            if (!existingOverlay) {
                const overlay = document.createElement('div');
                overlay.className = 'node-refresh-overlay';
                overlay.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Updating...';
                card.appendChild(overlay);
            }
        } else if (existingOverlay) {
            existingOverlay.remove();
        }
    });
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

function isFieldActiveForEditing(field) {
    const raw = field ?.isActive;
    if (raw === undefined || raw === null) {
        return true;
    }

    if (raw === true || raw === 1) {
        return true;
    }

    if (typeof raw === 'string') {
        const normalized = raw.toLowerCase();
        return normalized === 'true' || normalized === '1';
    }

    return false;
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

function promptForTaskComment(taskTitle, incompletePrerequisiteCount) {
    return new Promise(resolve => {
        const promptTitle = incompletePrerequisiteCount > 0
            ? `<div class="mb-2">Add comment before completing ${escapeReviewHtml(taskTitle || 'this task')}</div><div class="text-muted small">${incompletePrerequisiteCount} prerequisite${incompletePrerequisiteCount === 1 ? '' : 's'} ${incompletePrerequisiteCount === 1 ? 'is' : 'are'} still not complete. A comment is required to continue.</div>`
            : `Add comment before completing ${escapeReviewHtml(taskTitle || 'this task')}`;

        bootbox.prompt({
            title: promptTitle,
            inputType: 'textarea',
            centerVertical: true,
            value: '',
            callback: function (result) {
                if (result === null) {
                    resolve(null);
                    return;
                }

                const remarks = String(result || '').trim();
                if (!remarks) {
                    toastr.warning('Comment is required before completing this task.');
                    return false;
                }

                resolve(remarks);
            }
        });
    });
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
                projectTaskSysId: node.projectNodeSysId || '',
                transactionKey: node.transactionKey || '',
                rawStatus,
                statusLabel,
                laneStatus: getMemberLaneStatus(rawStatus, statusLabel),
                laneLabel: getMemberLaneLabel(getMemberLaneStatus(rawStatus, statusLabel)),
                incompletePrerequisiteCount: prerequisiteSummary.incompleteCount,
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

function getMemberLaneStatuses(tasks) {
    return MEMBER_SWIMLANE_LANES.slice();
}

function isMemberLaneRestricted(status) {
    return restrictedMemberLaneSet.has(String(status || '').trim().toUpperCase());
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
    const laneStatuses = getMemberLaneStatuses(tasks);

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

    const laneSections = await Promise.all(laneStatuses.map(async lane => {
        const laneTasks = filteredTasks.filter(task => task.laneStatus === lane.code);
        const laneRestricted = isMemberLaneRestricted(lane.code);
        const laneCards = laneTasks.length
            ? (await Promise.all(laneTasks.map(async task => {
                const formDetails = await getReadonlyTaskFormDetails(task.projectNo, task.nodeType, task.nodeId, [task.projectTaskSysId]);
                const formActionButton = `
                    <button type="button" class="btn btn-outline-secondary btn-sm btn-update-forms ${!formDetails.withField ? 'disabled' : ''}"
                            ${!formDetails.withField ? 'tabindex="-1" aria-disabled="true"' : ''}>
                        <i class="fas fa-clipboard-list me-1"></i>Forms
                    </button>`;
                const targetDateButton = `
                    <button type="button" class="btn btn-outline-primary btn-sm btn-update-activity">
                        <i class="fas fa-calendar-plus me-1"></i>Target Date
                    </button>`;

                return `
                <article class="member-task-card"
                         draggable="true"
                         data-member-task-id="${escapeReviewHtml(task.nodeId)}"
                         data-member-lane="${escapeReviewHtml(task.laneStatus)}"
                         data-project-no="${escapeReviewHtml(task.projectNo)}"
                         data-node-id="${escapeReviewHtml(task.nodeId)}"
                         data-node-type="${escapeReviewHtml(task.nodeType)}"
                         data-project-node-sys-id="${escapeReviewHtml(task.projectTaskSysId)}"
                         data-transaction-key="${escapeReviewHtml(task.transactionKey)}"
                         data-activity-name="${escapeReviewHtml(task.title)}"
                         data-activity-owner="${escapeReviewHtml(task.ownerNames)}"
                         data-activity-status="${escapeReviewHtml(task.statusLabel)}"
                         data-activity-progress="${escapeReviewHtml(task.percentCompleted)}"
                         data-activity-start="${escapeReviewHtml(task.startDate)}"
                         data-activity-end="${escapeReviewHtml(task.endDate)}">
                    <div class="member-task-card__header">
                        <div class="member-task-card__header-main">
                            <span class="member-task-card__pill"><i class="fas fa-flag-checkered"></i>${escapeReviewHtml(task.milestoneName)}</span>
                            <h3 class="member-task-card__title">${escapeReviewHtml(task.title)}</h3>
                        </div>
                        <div class="member-task-card__header-actions">
                            <span class="member-task-card__drag"><i class="fas fa-grip-vertical"></i>Drag task</span>
                            <button type="button" class="activity-collapse-toggle" aria-label="Toggle member task details" aria-expanded="true">
                                <i class="fas fa-chevron-down"></i>
                            </button>
                        </div>
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
                        <div class="member-task-card__footer">
                            <div class="member-task-card__status">${taskStatusBadge(task.statusLabel)}</div>
                            <div class="member-task-card__actions">
                                ${formActionButton}
                                ${targetDateButton}
                            </div>
                        </div>
                    </div>
                </article>`;
            }))).join('')
            : `<div class="member-lane__empty">${memberTaskSearch.trim() ? 'No matching tasks in this lane.' : 'No tasks in this status for the selected member.'}</div>`;

        return `
            <section class="member-lane ${laneRestricted ? 'is-lane-restricted' : ''}" title="${laneRestricted ? 'You are not allowed to move tasks to this lane.' : ''}">
                <header class="member-lane__header">
                    <div class="member-lane__title-row">
                        <div class="member-lane__title">${escapeReviewHtml(lane.label)}</div>
                        ${laneRestricted ? '<span class="member-lane__restriction-badge">Restricted lane</span>' : ''}
                    </div>
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

function getTaskStatusRoute(status) {
    const routes = {
        COMPLETED: 'complete',
        REOPEN: 'reopen',
        UNLOCK: 'unlock',
        UNHOLD: 'unlock',
        INITIALIZE: 'initialize',
        START: 'start',
        HOLD: 'hold',
        CANCEL: 'cancel',
        ARCHIVED: 'archive'
    };

    const key = String(status ?? '').trim().toUpperCase();
    return routes[key] || String(status ?? '').trim().toLowerCase();
}

function clearMemberLaneDropTargets() {
    if (!DOM.memberSwimlane) {
        return;
    }

    DOM.memberSwimlane.querySelectorAll('.member-lane.is-drop-target').forEach(lane => {
        lane.classList.remove('is-drop-target');
    });
}

function setMemberTaskDragging(card, isDragging) {
    if (!card) {
        return;
    }

    card.classList.toggle('is-dragging', isDragging);
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

function findMemberTaskById(nodeId) {
    const normalizedId = String(nodeId ?? '').trim();
    return getMemberTasksForReview(selectedMemberId).find(task => String(task.nodeId ?? '').trim() === normalizedId) || null;
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
            ? 'Focus on one teammate at a time and review their workload in status-based swimlanes.'
            : 'Track milestone health, manage task actions, and jump into your assigned work without leaving the page.';
    }

    if (isMemberMode) {
        renderMemberFocusList();
        void renderMemberSwimlane();
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

    if (DOM.toggleMemberTasksCollapseButton && DOM.toggleMemberTasksCollapseButton.dataset.bound !== 'true') {
        DOM.toggleMemberTasksCollapseButton.dataset.bound = 'true';
        DOM.toggleMemberTasksCollapseButton.addEventListener('click', function () {
            const allCollapsed = this.dataset.collapsed === 'true';
            setAllMemberTaskCardsCollapsed(!allCollapsed);
        });

        syncMemberTaskCollapseButton();
    }
}

// =========================
// Status & Badge Helpers
// =========================
// Transferred to common-functions.js


// =========================
// Utility Helpers
// =========================
// Transferred to common-functions.js


function onAddTaskClick() {
    alert("Floating Add Button clicked!");
}




// =========================
// API helpers (async)
// =========================

function saveTaskStatusAsync(projectno, projecttasksysid, roadmapactivitysysId, transactionkey, status, remarks) {
    const formData = new FormData();
    const routeCode = projecttasksysid || roadmapactivitysysId;
    const submitform = {
        projectNo: projectno,
        projectTaskSysId: projecttasksysid,
        roadmapActivitySysId: roadmapactivitysysId,
        transactionKey: transactionkey,
        remarks: remarks || ''
    };

    formData.append("projecttask", JSON.stringify(submitform));

    return $.ajax({
        url: getApiRootPath() + `/api/projecttasks/${routeCode}/${getTaskStatusRoute(status)}`,
        type: 'PUT',
        data: formData,
        processData: false,
        contentType: false
    });
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

    const percentCompleted = obj.projectNodeCount === 0 ? 100 : calcPercentCompleted({
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

    const total = obj.projectNodeCount - (nodeId === "__ROOTACTIVITY__" ? 0 : 1);
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

    const updateTargetButton = (nodeId !== "__ROOTACTIVITY__") ? `
        <button class="btn btn-outline-primary btn-pill-sm btn-update-milestone ${!isOwned ? "disabled" : ""}"
                ${!isOwned ? 'tabindex="-1" aria-disabled="true"' : ""}>
            <i class="fas fa-pen-to-square me-1"></i> Update
        </button>
<span id="updateMilestoneProcessing" class="ms-2 text-muted d-none">
    <i class="fas fa-spinner fa-spin me-1"></i>Processing...
</span>`: '';

    const nodeLocked = obj.projectNodeStatus === "NOT STARTED";

    const unlockButton = nodeLocked ? `
        <button id="btnUnlockSubmit" name="btnUnlockSubmit" class="btn p-0 border-0 bg-transparent icon-btn text-sm btn-unlock ${!isOwned ? "disabled" : ""}">
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
                ${updateTargetButton}
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
    const owners = getNodeOwners(obj);

    const ownerlist = owners.map(owner => owner.name).join(', ');

    const ownerIdArray = owners
        .map(owner => owner.id)
        .filter(Boolean);

    const owneridlist = ownerIdArray.join(',');
    const ownerButtons = buildOwnerFocusButtons(owners);

    const isOwned = ownerIdArray.includes(user ?.EmployeeId); // FIX: true if user is ANY owner

    // prerequisites
    const prerequisiteSummary = buildTaskPrerequisiteSummary(obj.prerequisitesJson);
    const incompletePrerequisites = prerequisiteSummary.incompleteCount;
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
            class="form-check-input chk-complete-task ${!allowUpdate ? "disabled" : ""}"
            ${!allowUpdate ? 'disabled tabindex="-1" aria-disabled="true"' : ""}
            ${nodeStatus === "Completed" ? "checked" : ""}
        >`;

    const updateFormButton = `
        <button class="btn btn-outline-primary btn-pill-sm btn-update-forms ${(!allowUpdate || !withField) ? "disabled" : ""}"
                ${(!allowUpdate || !withField) ? 'tabindex="-1" aria-disabled="true"' : ""}>
            <i class="fas fa-pen-to-square me-1"></i> Update
        </button>`;

    const updateTargetButton = `
        <button class="btn p-0 border-0 bg-transparent icon-btn text-sm btn-update-activity ${!isOwned ? "disabled" : ""}"
                ${!isOwned ? 'tabindex="-1" aria-disabled="true"' : ""}>
            <i class="bi bi-calendar-check-fill""></i>
        </button>`;

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
            data-pending-prerequisites="${incompletePrerequisites}"
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
                            <div>
                                ${updateTargetButton}
                            </div>
                        </div>
                    </div>
                    <div class="activity-meta">
                        <span class="activity-owner-actions">
                            <i class="far fa-user me-1"></i>
                            ${ownerButtons}
                        </span>
                        <span class="mx-1">•</span>
                        <i class="far fa-calendar me-1"></i>${startDateFormatted} → ${endDateFormatted}
                        ${closureDateElement}
                    </div>
                    ${prerequisiteElement}
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
${updateFormButton}
                    
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

function buildOwnerFocusButtons(owners) {
    if (!owners.length) {
        return '<span class="owner-focus-empty">Unassigned</span>';
    }

    return owners.map(owner => `
        <button type="button"
                class="owner-focus-button btn-member-focus"
                data-member-id="${escapeReviewHtml(owner.id)}"
                data-member-name="${escapeReviewHtml(owner.name)}"
                title="Show ${escapeReviewHtml(owner.name)} in member-centric view">
            <span class="owner-focus-avatar">${escapeReviewHtml(owner.initials)}</span>
            <span>${escapeReviewHtml(owner.name)}</span>
        </button>`).join('');
}


// =========================
// Builder / Page initialization
// =========================

function builderReviewer(id, obj) {
    project = obj[0];
    projectNodes = obj;
    window.projectNodes = projectNodes;

    const manageMyTasksButton = document.getElementById('btnReviewManageMyTasks');
    if (manageMyTasksButton) {
        manageMyTasksButton.disabled = false;
        manageMyTasksButton.onclick = function () {
            openCurrentUserProjectTasksModal(id, project.jsonMembers);
        };
    }

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
    buildHierarchyTree();




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
        syncActivitiesCollapseButton();
    } catch (err) {
        console.error('Error building task items', err);
    } finally {
        hideLoadingTasks();
    }
}

function upsertProjectNode(node) {
    if (!node) {
        return;
    }

    const items = Array.isArray(projectNodes) ? projectNodes : [];
    const index = items.findIndex(item => item ?.nodeId === node.nodeId && item ?.nodeType === node.nodeType);

    if (index >= 0) {
        items[index] = node;
        return;
    }

    items.push(node);
    projectNodes = items;
}

async function fetchAndMergeProjectNode(projectNo, nodeType, nodeId) {
    const node = await fetchProjectNodeItemAsync(projectNo, nodeType, nodeId);
    upsertProjectNode(node);
    return node;
}

async function refreshItemNode(projectNo, nodeType, nodeId) {
    if (!DOM.activitiesContainer) return;

    setNodeRefreshingState(nodeType, nodeId, true);

    try {
        const obj = await fetchAndMergeProjectNode(projectNo, nodeType, nodeId);

        const oldCard = DOM.activitiesContainer.querySelector(
            `.activity-card[data-node-id="${nodeId}"][data-node-type="${nodeType}"]`
        );

        if (oldCard) {
            const newCardHTML = await taskNode(obj);
            const newCard = htmlToElement(newCardHTML);
            DOM.activitiesContainer.replaceChild(newCard, oldCard);

            applyFilters();
            syncActivitiesCollapseButton();
        }

        if (currentReviewMode === REVIEW_MODES.MEMBER) {
            renderMemberFocusList();
            renderMemberSwimlane();
        }
    } catch (err) {
        console.error('Error refreshing item node', err);
    } finally {
        setNodeRefreshingState(nodeType, nodeId, false);
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

function openUpdateActivityModal(container) {
    if (!container) {
        return;
    }

    _saveNode = container;
    const activityProjectNodeSysId = container.dataset.projectNodeSysId || '';
    const id = container.dataset.nodeId || '';
    const type = container.dataset.nodeType || '';
    const name = container.dataset.activityName || '';
    const owner = container.dataset.activityOwner || '';
    const status = container.dataset.activityStatus || 'Ongoing';
    const progress = container.dataset.activityProgress || '0';
    const start = container.dataset.activityStart || '';
    const end = container.dataset.activityEnd || '';

    document.getElementById('activityProjectNodeSysId').value = activityProjectNodeSysId;
    document.getElementById('activityId').value = id;
    document.getElementById('activityType').value = type;
    document.getElementById('activityName').value = name;
    document.getElementById('activityOwner').value = owner;
    document.getElementById('activityStatus').value = status;
    document.getElementById('activityProgress').value = progress;
    document.getElementById('activityStart').value = start;
    document.getElementById('activityEnd').value = end;
    document.getElementById('activityRemarks').value = '';
    document.getElementById('chkResched').checked = (status === 'Resched');

    modal.show();
}

async function openUpdateFormsModal(container) {
    if (!container) {
        return;
    }

    _saveNode = container;

    const id = container.dataset.nodeId || '';
    const type = container.dataset.nodeType || '';
    const name = container.dataset.activityName || '';
    const projectNodeSysId = container.dataset.projectNodeSysId || '';

    document.getElementById('form-nodeId').value = id;
    document.getElementById('form-activityName').textContent = name;

    await submitForm(id, type, false, [projectNodeSysId]);
}

async function setTaskCompletionState(container, completeNode) {
    if (!container) {
        return;
    }

    _saveNode = container;

    const id = container.dataset.nodeId || '';
    const type = container.dataset.nodeType || '';
    const projectNodeSysId = container.dataset.projectNodeSysId || '';
    const transactionKey = container.dataset.transactionKey || '';
    const activityWithField = container.dataset.activityWithField === 'true';
    const activityWithPending = container.dataset.activityWithPending === 'true';

    if (completeNode) {
        if (activityWithField && activityWithPending) {
            await submitForm(id, type, true, [projectNodeSysId]);
        } else {
            await finalizeAction(true, container);
        }
    } else {
        const projectNo = $('#projectNo').val();
        await saveTaskStatusAsync(projectNo, projectNodeSysId, id, transactionKey, 'REOPEN');
        await refreshItemNode(projectNo, type, id);

        const nodeName = container.querySelector('.node-name');
        const del = nodeName ?.querySelector('del');
        if (del && nodeName) {
            nodeName.textContent = del.textContent;
        }
    }
}

async function transitionMemberTaskToLane(task, targetLane) {
    if (!task || !targetLane || task.laneStatus === targetLane || isMemberLaneTransitionPending) {
        return;
    }

    if (isMemberLaneRestricted(targetLane)) {
        if (window.toastr) {
            window.toastr.warning('You are not allowed to move tasks to this lane.');
        } else {
            bootbox.alert('You are not allowed to move tasks to this lane.');
        }
        return;
    }

    isMemberLaneTransitionPending = true;

    try {
        const latestNode = await fetchAndMergeProjectNode(task.projectNo, task.nodeType, task.nodeId);
        const latestTaskSysId = latestNode ?.projectNodeSysId || task.projectTaskSysId || '';
        const latestTransactionKey = latestNode ?.transactionKey || task.transactionKey || '';
        const incompletePrerequisiteCount = getIncompletePrerequisiteCount(latestNode ?.prerequisitesJson);
        let remarks = '';

        if (targetLane === 'COMPLETED') {
            if (incompletePrerequisiteCount > 0) {
                remarks = await promptForTaskComment(task.title, incompletePrerequisiteCount);
                if (remarks === null) {
                    return;
                }
            }

            await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'COMPLETED', remarks);
        } else if (targetLane === 'CANCELLED') {
            await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'CANCEL');
        } else if (targetLane === 'HOLD') {
            await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'HOLD');
        } else if (targetLane === 'ARCHIVED') {
            await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'ARCHIVED');
        } else if (targetLane === 'NOT STARTED') {
            await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'INITIALIZE');
        } else if (targetLane === 'ONGOING') {
            if (task.laneStatus === 'HOLD') {
                await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'UNHOLD');
            } else if (task.laneStatus === 'COMPLETED' || task.laneStatus === 'CANCELLED' || task.laneStatus === 'ARCHIVED') {
                await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'INITIALIZE');
                await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'START');
            } else {
                await saveTaskStatusAsync(task.projectNo, latestTaskSysId, task.nodeId, latestTransactionKey, 'START');
            }
        }

        await refreshItemNode(task.projectNo, task.nodeType, task.nodeId);
    } catch (error) {
        console.error('Unable to move member task between lanes', error);
        bootbox.alert('Unable to update the task status right now.');
        renderMemberSwimlane();
    } finally {
        isMemberLaneTransitionPending = false;
        memberDragTaskId = '';
        clearMemberLaneDropTargets();
    }
}





// =========================
// Event binding (delegation)
// =========================

function bindGlobalEvents() {
    // Milestone click
    if (DOM.milestoneContainer) {
        DOM.milestoneContainer.addEventListener('click', async function (e) {
            const btnUnlock = e.target.closest('.btn-unlock');      // button inside milestone
            const btnUpdate = e.target.closest('.btn-update-milestone');      // button inside milestone
            const mi = e.target.closest('.milestone-item');         // the milestone card itself
            if (!mi) return;

            const nodeId = mi.dataset.milestoneId;
            const nodeType = mi.dataset.milestoneType;
            const projectNo = mi.dataset.milestoneProjectNo;
            const owners = mi.dataset.milestoneOwners;
            const ownerIds = mi.dataset.milestoneOwnerIds;



            // If unlock button clicked
            if (btnUnlock) {
                const form = document.getElementById('formUnlock');
                const projectNodeSysId = mi.dataset.projectNodeSysId;
                form.querySelector('input[name="nodeId"]').value = nodeId || '';
                form.querySelector('input[name="nodeType"]').value = nodeType || '';
                form.querySelector('input[name="projectNodeSysId"]').value = projectNodeSysId || '';
                form.querySelector('input[name="nodeName"]').value = mi.dataset.milestoneName || '';
                form.querySelector('input[name="nodeOwners"]').value = owners || '';
                form.querySelector('input[name="transactionKey"]').value = mi.dataset.transactionKey || '';

                ////// Adjust this when you have a real owners data attribute:
                ////// e.g. data-milestone-owners="John Doe,Jane Smith"
                ////form.querySelector('input[name="nodeOwners"]').value = mi.dataset.milestoneOwners || '';

                //const unlockModal = new bootstrap.Modal(document.getElementById('modalUnlock'));
                //unlockModal.show();

                modalUnlock.show();


                // Stop further handling
                e.stopPropagation();
                e.preventDefault();
                return;
            }

            // If update button clicked
            if (btnUpdate) {
                const formUpdateMilestone = document.getElementById('formUpdateMilestone');
                const projectNodeSysId = mi.dataset.projectNodeSysId;
                formUpdateMilestone.querySelector('input[name="nodeId"]').value = nodeId || '';
                formUpdateMilestone.querySelector('input[name="nodeType"]').value = nodeType || '';
                formUpdateMilestone.querySelector('input[name="projectNodeSysId"]').value = projectNodeSysId || '';
                formUpdateMilestone.querySelector('input[name="nodeName"]').value = mi.dataset.milestoneName || '';
                formUpdateMilestone.querySelector('input[name="nodeOwners"]').value = owners || '';

                formUpdateMilestone.querySelector('input[name="nodeStart"]').value = mi.dataset.milestoneStart || '';
                formUpdateMilestone.querySelector('input[name="nodeEnd"]').value = mi.dataset.milestoneEnd || '';

                ////// Adjust this when you have a real owners data attribute:
                ////// e.g. data-milestone-owners="John Doe,Jane Smith"
                ////form.querySelector('input[name="nodeOwners"]').value = mi.dataset.milestoneOwners || '';

                ////const updateModal = new bootstrap.Modal(document.getElementById('modalUpdateMilestone'));
                ////updateModal.show();
                modalM.show();


                // Stop further handling
                e.stopPropagation();
                e.preventDefault();
                return;
            }

            // Generic milestone click (no unlock button)
            setReviewSupportContext(nodeType, nodeId);
            await buildTaskItems(projectNo, nodeType, nodeId);
        });
    }

    // Activities container: delegation for buttons & checkboxes
    if (DOM.activitiesContainer) {
        DOM.activitiesContainer.addEventListener('click', async function (e) {
            const btnMemberFocus = e.target.closest('.btn-member-focus');
            if (btnMemberFocus) {
                e.preventDefault();
                setReviewMode(REVIEW_MODES.MEMBER, btnMemberFocus.dataset.memberId || '');
                return;
            }

            const btnToggleCollapse = e.target.closest('.activity-collapse-toggle');
            if (btnToggleCollapse) {
                const container = btnToggleCollapse.closest('[data-node-id]');
                toggleActivityCard(container);
                return;
            }

            const btnUpdateActivity = e.target.closest('.btn-update-activity');
            if (btnUpdateActivity) {
                const container = btnUpdateActivity.closest('[data-node-id]');
                if (!container) return;

                openUpdateActivityModal(container);
                return;
            }

            const btnUpdateForms = e.target.closest('.btn-update-forms');
            if (btnUpdateForms) {
                const container = btnUpdateForms.closest('[data-node-id]');
                if (!container) return;

                await openUpdateFormsModal(container);
                return;
            }

            const chkCompleteTask = e.target.closest('.chk-complete-task');
            if (chkCompleteTask) {
                const container = chkCompleteTask.closest('[data-node-id]');
                if (!container) return;
                const completeNode = chkCompleteTask.checked;

                await setTaskCompletionState(container, completeNode);
                return;
            }

            const activityCard = e.target.closest('.activity-card[data-node-id]');
            if (activityCard) {
                const taskContextId = activityCard.dataset.projectNodeSysId || activityCard.dataset.nodeId;
                setReviewSupportContext('TASK', taskContextId);
            }
        });
    }

    const attachmentsContextSelect = document.getElementById('attachmentsTargetContext');
    if (attachmentsContextSelect && attachmentsContextSelect.dataset.bound !== 'true') {
        attachmentsContextSelect.dataset.bound = 'true';
        attachmentsContextSelect.addEventListener('change', function () {
            const selected = parseContextValue(this.value);
            setReviewSupportContext(selected.entityType, selected.entitySysId);
        });
    }

    const commentsContextSelect = document.getElementById('commentTargetContext');
    if (commentsContextSelect && commentsContextSelect.dataset.bound !== 'true') {
        commentsContextSelect.dataset.bound = 'true';
        commentsContextSelect.addEventListener('change', function () {
            const selected = parseContextValue(this.value);
            setReviewSupportContext(selected.entityType, selected.entitySysId);
        });
    }

    if (DOM.filterStatus) DOM.filterStatus.addEventListener('change', applyFilters);
    if (DOM.filterOwner) DOM.filterOwner.addEventListener('change', applyFilters);
    if (DOM.filterSearch) DOM.filterSearch.addEventListener('input', applyFilters);

    if (DOM.memberSwimlane && DOM.memberSwimlane.dataset.bound !== 'true') {
        DOM.memberSwimlane.dataset.bound = 'true';

        DOM.memberSwimlane.addEventListener('click', async function (event) {
            const container = event.target.closest('.member-task-card[data-node-id]');
            if (!container) {
                return;
            }

            const btnToggleCollapse = event.target.closest('.activity-collapse-toggle');
            if (btnToggleCollapse) {
                event.preventDefault();
                const isCollapsed = !container.classList.contains('is-collapsed');
                container.classList.toggle('is-collapsed', isCollapsed);
                btnToggleCollapse.setAttribute('aria-expanded', String(!isCollapsed));
                syncMemberTaskCollapseButton();
                return;
            }

            const btnUpdateActivity = event.target.closest('.btn-update-activity');
            if (btnUpdateActivity) {
                event.preventDefault();
                openUpdateActivityModal(container);
                return;
            }

            const btnUpdateForms = event.target.closest('.btn-update-forms');
            if (btnUpdateForms) {
                event.preventDefault();
                await openUpdateFormsModal(container);
            }
        });

        DOM.memberSwimlane.addEventListener('dragstart', function (event) {
            const card = event.target.closest('.member-task-card[data-member-task-id]');
            if (!card || isMemberLaneTransitionPending) {
                return;
            }

            memberDragTaskId = card.dataset.memberTaskId || '';
            setMemberTaskDragging(card, true);

            if (event.dataTransfer) {
                event.dataTransfer.effectAllowed = 'move';
                event.dataTransfer.setData('text/plain', memberDragTaskId);
            }
        });

        DOM.memberSwimlane.addEventListener('dragend', function (event) {
            const card = event.target.closest('.member-task-card[data-member-task-id]');
            setMemberTaskDragging(card, false);
            memberDragTaskId = '';
            clearMemberLaneDropTargets();
        });

        DOM.memberSwimlane.addEventListener('dragover', function (event) {
            const laneBody = event.target.closest('.member-lane__body[data-lane-status]');
            if (!laneBody || isMemberLaneTransitionPending) {
                return;
            }

            const targetLane = laneBody.dataset.laneStatus || '';
            if (isMemberLaneRestricted(targetLane)) {
                return;
            }

            event.preventDefault();
            clearMemberLaneDropTargets();
            laneBody.closest('.member-lane') ?.classList.add('is-drop-target');

            if (event.dataTransfer) {
                event.dataTransfer.dropEffect = 'move';
            }
        });

        DOM.memberSwimlane.addEventListener('dragleave', function (event) {
            const lane = event.target.closest('.member-lane');
            if (!lane) {
                return;
            }

            const relatedTarget = event.relatedTarget instanceof Element ? event.relatedTarget.closest('.member-lane') : null;
            if (relatedTarget === lane) {
                return;
            }

            lane.classList.remove('is-drop-target');
        });

        DOM.memberSwimlane.addEventListener('drop', async function (event) {
            const laneBody = event.target.closest('.member-lane__body[data-lane-status]');
            if (!laneBody) {
                return;
            }

            event.preventDefault();

            const targetLane = laneBody.dataset.laneStatus || '';
            if (isMemberLaneRestricted(targetLane)) {
                clearMemberLaneDropTargets();
                if (window.toastr) {
                    window.toastr.warning('You are not allowed to move tasks to this lane.');
                }
                return;
            }

            const taskId = memberDragTaskId || event.dataTransfer ?.getData('text/plain') || '';
            const task = findMemberTaskById(taskId);

            clearMemberLaneDropTargets();

            if (!task) {
                return;
            }

            await transitionMemberTaskToLane(task, targetLane);
        });
    }

    document.getElementById('formUpdateActivity').addEventListener('submit', function (e) {
        e.preventDefault();

        const projectNodeSysId = document.getElementById('activityProjectNodeSysId').value;
        const id = document.getElementById('activityId').value;
        const type = document.getElementById('activityType').value;
        const start = document.getElementById('activityStart').value;
        const end = document.getElementById('activityEnd').value;
        const remarks = document.getElementById('activityRemarks').value;

        const container = document.querySelector(`[data-node-id="${id}"]`);
        if (container) {


            const formData = new FormData();
            const projectNo = $('#projectNo').val();

            const targetdate = {
                projectNo,
                projectNodeSysId,
                nodeId: id,
                nodeType: type,
                targetStartDate: start,
                targetCompletionDate: end,
                remarks: remarks
            };

            formData.append("targetdate", JSON.stringify(targetdate));

            const ajaxOptions = {
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: async function () {
                    await refreshItemNode(projectNo, type, id);
                },
                error: function (xhr) {
                }
            };

            $.ajax({
                ...ajaxOptions,
                url: getApiRootPath() + `/api/ProjectTasks/${projectNo}/targetchange`,
                type: 'PUT',
            });





            ////container.dataset.activityStatus = status;
            ////container.dataset.activityProgress = progress;
            ////container.dataset.activityStart = start;
            ////container.dataset.activityEnd = end;

            ////const owner = container.dataset.activityOwner || '';
            ////const metaEl = container.querySelector('.activity-meta');
            ////if (metaEl) {
            ////    metaEl.innerHTML =
            ////        '<i class="far fa-user me-1"></i>' + owner +
            ////        '<span class="mx-1">•</span>' +
            ////        '<i class="far fa-calendar me-1"></i>' + start + ' → ' + end;
            ////}

            ////const progressBar = container.querySelector('.progress-bar');
            ////if (progressBar) {
            ////    progressBar.style.width = progress + '%';
            ////}

            ////const progressText = container.querySelector('.activity-progress-text');
            ////if (progressText) {
            ////    progressText.textContent = progress + '% done';
            ////}

            ////const chip = container.querySelector('.status-chip');
            ////if (chip) {
            ////    chip.textContent = status;
            ////    chip.classList.remove('status-ontrack', 'status-resched', 'status-closed');
            ////    if (status === 'On Track') chip.classList.add('status-ontrack');
            ////    else if (status === 'Resched') chip.classList.add('status-resched');
            ////    else chip.classList.add('status-closed');
            ////}
        }

        modal.hide();
    });

    document.getElementById('modalUpdateForm').addEventListener('submit', function (e) {
        e.preventDefault();

        const form = document.getElementById('formUpdateForm');
        const submitBtn = form ? form.querySelector('button[type="submit"]') : null;
        const originalSubmitHtml = submitBtn ? submitBtn.innerHTML : '';

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Saving...';
        }

        const values = $('#formUpdateForm .modal-body').dynamicFieldGetValues();
        const jsonValues = JSON.parse(JSON.stringify(values));

        const fieldsToSubmit = Object.entries(jsonValues)
            .map(([key, field]) => ({
                fieldKey: key,
                formFieldSysId: field.dataAttributes.fieldFormFieldSysId,
                entityType: field.dataAttributes.fieldElementType,
                entitySysId: field.dataAttributes.fieldElementSysId,
                formSysId: field.dataAttributes.fieldFormSysId,
                formEntityLinkSysId: field.dataAttributes.fieldFormEntityLinkSysId,
                title: field.name,
                value: field.value,
                type: field.dataAttributes.fieldType,
                isActive: field.dataAttributes.fieldIsActive,
                submissionSysId: field.dataAttributes.fieldSubmissionSysId,
                submissionValueSysId: field.dataAttributes.fieldSubmissionValueSysId,
                transactionKey: field.dataAttributes.fieldSubmissionValueTransactionKey,
                submissionTransactionKey: field.dataAttributes.fieldSubmissionTransactionKey
            }))
            .filter(field => isFieldActiveForEditing(field));

        if (!fieldsToSubmit.length) {
            toastr.info('No active form fields available for update.');
            finalizeAction(autoCloseTask, _saveNode);
            modalForm.hide();
            return;
        }

        const submission = fieldsToSubmit[0];
        const submissionSysId = submission.submissionSysId;
        const trasactionKey = submission.submissionTransactionKey;
        const formData = new FormData();
        const projectNo = $('#projectNo').val();

        const submitform = {
            projectNo,
            submissionSysId,
            transactionKey: trasactionKey,
            fields: fieldsToSubmit
        };

        formData.append("formfields", JSON.stringify(submitform));

        const ajaxOptions = {
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: async function () {
                const saveNode = _saveNode;
                const saveNodeId = saveNode ?.dataset ?.nodeId || '';
                const saveNodeType = saveNode ?.dataset ?.nodeType || '';
                const saveNodeProjectNodeSysId = saveNode ?.dataset ?.projectNodeSysId || '';

                if (projectNo && saveNodeType && saveNodeId) {
                    invalidateTaskFormCacheForNode(projectNo, saveNodeType, saveNodeId, [saveNodeProjectNodeSysId]);
                }

                await finalizeAction(autoCloseTask, _saveNode);
                modalForm.hide();
            },
            complete: function () {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalSubmitHtml;
                }
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    toastr.error('Plant code does not exist!');
                } else {
                    toastr.error('Error: ' + xhr.responseText);
                }
            }
        };

        if (!submissionSysId) {
            $.ajax({
                ...ajaxOptions,
                url: getApiRootPath() + `/api/ProjectForms`,
                type: 'POST',
            });
        } else {
            $.ajax({
                ...ajaxOptions,
                url: getApiRootPath() + `/api/ProjectForms/${submissionSysId}`,
                type: 'PUT',
            });
        }

    });

    document.getElementById('formUnlock').addEventListener('submit', function (e) {
        e.preventDefault();

        const form = document.getElementById('formUnlock');
        const projectNodeSysId = form.querySelector('input[name="projectNodeSysId"]').value;
        const transactionKey = document.getElementById('transactionKey').value;
        const remarks = document.getElementById('reason').value;

        const formData = new FormData();
        const projectNo = $('#projectNo').val();

        const unlockNode = {
            projectNo: projectNo,
            id: projectNodeSysId,
            reason: remarks,
            transactionKey: transactionKey
        };

        // Button + processing indicator
        const submitBtn = document.getElementById('btnUnlockSubmit');
        const processingEl = document.getElementById('unlockProcessing');

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.dataset.originalText = submitBtn.dataset.originalText || submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Unlocking...';
        }
        if (processingEl) {
            processingEl.classList.remove('d-none');
        }


        modalUnlock.hide();




        $.ajax({
            url: getApiRootPath() + `/api/milestones/unlock`,
            method: 'PUT',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(unlockNode),
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: async function (result) {


                try {
                    const projectNo = $('#projectNo').val();

                    const activeMilestone = document.querySelector('.milestone-item.active');
                    const activeMilestoneId = activeMilestone ? activeMilestone.dataset.milestoneId : null;
                    const activeMilestoneType = activeMilestone ? activeMilestone.dataset.milestoneType : null;

                    const projects = await fetchProjectAsync(projectNo);
                    builderReviewer(projectNo, projects);

                    if (activeMilestoneId && activeMilestoneType) {
                        await buildTaskItems(projectNo, activeMilestoneType, activeMilestoneId);
                    }


                } catch (err) {
                    console.error('Error refreshing milestones after unlock', err);
                }
            },
            error: function (xhr) {
                console.error('Error adding comment', xhr);
                alert('Error adding comment.');
            },
            complete: function () {
                if (submitBtn) submitBtn.disabled = false;
            }
        });


    });

    document.getElementById('formUpdateMilestone').addEventListener('submit', function (e) {
        e.preventDefault();




        const projectNo = $('#projectNo').val();
        const form = document.getElementById('formUpdateMilestone');
        const projectNodeSysId = form.querySelector('input[name="projectNodeSysId"]').value;
        const id = form.querySelector('input[name="nodeId"]').value;
        const type = form.querySelector('input[name="nodeType"]').value;
        const start = form.querySelector('input[name="nodeStart"]').value;
        const end = form.querySelector('input[name="nodeEnd"]').value;
        const remarks = form.querySelector('textarea[name="remarks"]').value;




        // Button + processing indicator
        const submitBtn = form.querySelector('input[name="btn-update-milestone"]');
        const processingEl = form.querySelector('input[name="updateMilestoneProcessing"]');

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.dataset.originalText = submitBtn.dataset.originalText || submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Updating...';
        }
        if (processingEl) {
            processingEl.classList.remove('d-none');
        }



        const container = document.querySelector(`[data-milestone-id="${id}"]`);
        if (container) {


            const formData = new FormData();

            const targetdate = {
                projectNo,
                projectNodeSysId,
                nodeId: id,
                nodeType: type,
                targetStartDate: start,
                targetCompletionDate: end,
                remarks: remarks
            };

            formData.append("targetdate", JSON.stringify(targetdate));

            const ajaxOptions = {
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: async function (result) {


                    try {

                        const activeMilestone = document.querySelector('.milestone-item.active');
                        const activeMilestoneId = activeMilestone ? activeMilestone.dataset.milestoneId : null;
                        const activeMilestoneType = activeMilestone ? activeMilestone.dataset.milestoneType : null;

                        const projects = await fetchProjectAsync(projectNo);
                        builderReviewer(projectNo, projects);

                        if (activeMilestoneId && activeMilestoneType) {
                            await buildTaskItems(projectNo, activeMilestoneType, activeMilestoneId);
                        }


                    } catch (err) {
                        console.error('Error refreshing milestones after unlock', err);
                    }
                },
                error: function (xhr) {
                }
            };

            //////const updateModal = new bootstrap.Modal(document.getElementById('modalUpdateMilestone'));
            //////updateModal.hide();

            modalM.hide();

            $.ajax({
                ...ajaxOptions,
                url: getApiRootPath() + `/api/milestones/${projectNo}/targetchange`,
                type: 'PUT',
            });




        }

    });
}


// =========================
// submitForm (async modal builder)
// =========================

async function submitForm(id, type, autoClose, alternateNodeIds = []) {
    autoCloseTask = autoClose;

    const projectNo = $('#projectNo').val();
    const validForms = await fetchProjectDataAsync(projectNo, type, id, alternateNodeIds);
    const $containerForms = $('#formUpdateForm .modal-body');
    $containerForms.html('');

    let formObj;
    let fields = [];
    validForms.forEach(f => {
        try {
            formObj = JSON.parse(f.formJson);
        } catch (e) {
            bootbox.alert("Invalid form JSON!");
            return;
        }

        const formSysId = f.formSysId;
        const formEntityLinkSysId = f.formEntityLinkSysId;

        if (Array.isArray(formObj.fields)) {
            formObj.fields.forEach(field => {
                field.formSysId = field.formSysId || formSysId;
                field.formEntityLinkSysId = field.formEntityLinkSysId || formEntityLinkSysId;
            });
            fields.push(...formObj.fields);
        }
    });

    const normalizedFields = fields.map(fld => ({
        ...fld,
        isrequired: fld.isrequired === "true",
        urlIsParameter: fld.urlIsParameter === "true"
    }));

    const editableFields = normalizedFields.filter(isFieldActiveForEditing);

    const fetches = [];
    editableFields.forEach(f => {
        const jsonparsed = safeJsonParse(f.values, []);
        if (!Array.isArray(jsonparsed)) return;

        const submissionValue = findSubmissionValueByContext(jsonparsed, type, [id, ...alternateNodeIds], f.id);

        if (submissionValue && submissionValue.id) {
            fetches.push(
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

    await Promise.all(fetches);

    function fixArrayStringsRecursively(value) {
        if (Array.isArray(value)) {
            return value.map(fixArrayStringsRecursively);
        }
        if (value && typeof value === "object") {
            for (const key of Object.keys(value)) {
                value[key] = fixArrayStringsRecursively(value[key]);
            }
            return value;
        }
        if (typeof value === "string") {
            const trimmed = value.trim();
            if (trimmed.startsWith("[") && trimmed.endsWith("]")) {
                try {
                    const parsed = JSON.parse(trimmed);
                    if (Array.isArray(parsed)) return parsed;
                } catch (e) {
                    // ignore
                }
            }
        }
        return value;
    }

    fixArrayStringsRecursively(editableFields);

    $containerForms.dynamicField({
        fields: editableFields,
        userCode: "*",
        emptyMessage: 'No Link Form found for this node.',
        buildFieldDataAttributes: function (field) {
            return {
                "field-form-field-sys-id": field.id ?? field.fieldFormFieldSysId,
                "field-type": field.type ?? field.fieldType,
                "field-is-active": isFieldActiveForEditing(field) ? "true" : "false",
                "field-form-sys-id": field.formSysId ?? field.fieldFormSysId,
                "field-form-entity-link-sys-id": field.formEntityLinkSysId ?? field.fieldFormEntityLinkSysId,
                "field-name": field.name ?? field.fieldName,
                "field-element-sys-id": id,
                "field-element-type": type,
                "field-submission-sys-id": field.submissionSysId ?? field.fieldSubmissionSysId ?? "",
                "field-submission-transaction-key": field.submissionTransactionKey ?? field.fieldSubmissionTransactionKey ?? "",
                "field-submission-value-sys-id": field.submissionValueSysId ?? field.fieldSubmissionValueSysId ?? "",
                "field-submission-value-transaction-key": field.submissionValueTransactionKey ?? field.fieldSubmissionValueTransactionKey ?? ""
            };
        },
        includeDataAttributesInValues: true,
        includeBuiltDataAttributesInValues: true
    });

    modalForm.show();
}



// =========================
// submitUnlock (async modal builder)
// =========================

async function submitUnlock(id, type) {

    const projectNo = $('#projectNo').val();

    modalUnlock.show();
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

        allReviewAttachments = await fetchAttachmentsAsync(projectno);

        allReviewComments = await fetchCommentsAsync(projectno);
        await setReviewSupportContext('', '');
        initSidebarNotifications(projectno);

        bindSidebarStretchToggle();
        bindActivitiesCollapseButton();
        bindReviewModeEvents();
    } catch (err) {
        console.error('Error initializing page', err);
    } finally {
        hideLoadingMilestone();
        hideLoadingTasks();
    }
});