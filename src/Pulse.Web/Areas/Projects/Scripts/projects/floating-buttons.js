const fabMenu = document.getElementById("fabMenu");
const mainFab = document.getElementById("mainFab");
const mainFabIcon = document.getElementById("mainFabIcon");
let fabOpen = false;
const PROJECT_ATTACHMENT_API_BASE = getApiRootPath() + "/api/files";

var selectedEntityType = "";
var selectedEntitySysId = "";

const FLOATING_CONTEXT_SELECTORS = [
    'attachmentsTargetContext',
    'commentTargetContext',
    'notificationTargetContext'
];

function escapeAttribute(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

function stripHtml(input) {
    const container = document.createElement('div');
    container.innerHTML = input || '';
    return (container.textContent || container.innerText || '').trim();
}

function sanitizeCommentHtml(input) {
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

function normalizeEntityType(value) {
    return String(value || '').trim().toUpperCase();
}

function normalizeFloatingContext(entityType, entitySysId, projectNodeSysId) {
    const normalizedType = normalizeEntityType(entityType);
    const normalizedProjectNodeSysId = String(projectNodeSysId || '').trim();

    if (normalizedProjectNodeSysId) {
        return {
            entityType: 'TASK',
            entitySysId: normalizedProjectNodeSysId
        };
    }

    return {
        entityType: normalizedType,
        entitySysId: String(entitySysId || '').trim()
    };
}

function buildContextValue(entityType, entitySysId) {
    return `${normalizeEntityType(entityType)}::${String(entitySysId || '')}`;
}

function parseContextValue(value) {
    const raw = String(value || '');
    const separatorIndex = raw.indexOf('::');
    if (separatorIndex < 0) {
        return {
            entityType: '',
            entitySysId: ''
        };
    }

    return {
        entityType: normalizeEntityType(raw.slice(0, separatorIndex)),
        entitySysId: raw.slice(separatorIndex + 2)
    };
}

function getProjectContext() {
    const projectNo = $('#projectNo').val() || '';
    const projectName = $('#projectName').val() || projectNo || 'Current project';

    return {
        entityType: 'PROJECT',
        entitySysId: projectNo,
        label: `Project: ${projectName}`,
        group: 'Project',
        sortOrder: 0
    };
}

function collectFloatingContexts() {
    const projectContext = getProjectContext();
    const contexts = [projectContext];
    const seen = new Set([buildContextValue(projectContext.entityType, projectContext.entitySysId)]);

    function pushContext(entityType, entitySysId, label, group, sortOrder, projectNodeSysId) {
        const normalizedContext = normalizeFloatingContext(entityType, entitySysId, projectNodeSysId);
        const normalizedType = normalizedContext.entityType;
        const normalizedId = normalizedContext.entitySysId;
        const key = buildContextValue(normalizedType, normalizedId);

        if (!normalizedType || !normalizedId || seen.has(key)) {
            return;
        }

        seen.add(key);
        contexts.push({
            entityType: normalizedType,
            entitySysId: normalizedId,
            label,
            group,
            sortOrder
        });
    }

    if (Array.isArray(window.projectNodes)) {
        window.projectNodes.forEach((node) => {
            const nodeType = normalizeEntityType(node.nodeType);
            if (nodeType === 'MILESTONE') {
                pushContext(nodeType, node.nodeId, `Milestone: ${node.nodeName || 'Milestone'}`, 'Milestones', 1);
            }

            if (nodeType === 'TASK' || nodeType === 'ACTIVITY') {
                pushContext(nodeType, node.nodeId, `Task: ${node.nodeName || 'Task'}`, 'Tasks', 2, node.projectNodeSysId);
            }
        });
    }

    document.querySelectorAll('.milestone-item[data-milestone-id][data-milestone-type]').forEach((item) => {
        const entityType = normalizeEntityType(item.dataset.milestoneType);
        const entitySysId = item.dataset.milestoneId || '';
        const label = item.dataset.milestoneName || item.querySelector('.milestone-title')?.textContent?.trim() || 'Milestone';
        pushContext(entityType, entitySysId, `Milestone: ${label}`, 'Milestones', 1);
    });

    document.querySelectorAll('.activity-card[data-node-id][data-node-type]').forEach((item) => {
        const context = normalizeFloatingContext(item.dataset.nodeType, item.dataset.nodeId, item.dataset.projectNodeSysId);
        const label = item.dataset.activityName || item.querySelector('.node-name')?.textContent?.trim() || 'Task';
        pushContext(context.entityType, context.entitySysId, `Task: ${label}`, 'Tasks', 2);
    });

    return contexts.sort((left, right) => {
        if (left.sortOrder !== right.sortOrder) {
            return left.sortOrder - right.sortOrder;
        }

        return left.label.localeCompare(right.label, undefined, { sensitivity: 'base' });
    });
}

function resolveFloatingContextLabel(entityType, entitySysId) {
    const normalizedType = normalizeEntityType(entityType);
    if (normalizedType === 'PROJECT' || (!normalizedType && !entitySysId)) {
        return getProjectContext().label;
    }

    const match = collectFloatingContexts().find((context) =>
        context.entityType === normalizedType && String(context.entitySysId || '') === String(entitySysId || ''));

    if (match) {
        return match.label;
    }

    if (normalizedType === 'MILESTONE') {
        return 'Milestone';
    }

    if (normalizedType === 'TASK') {
        return 'Task';
    }

    return 'Project';
}

function populateFloatingContextSelectors(preferredEntityType, preferredEntitySysId) {
    const contexts = collectFloatingContexts();
    const projectContext = getProjectContext();
    const preferredValue = buildContextValue(
        preferredEntityType || projectContext.entityType,
        preferredEntitySysId || projectContext.entitySysId
    );

    FLOATING_CONTEXT_SELECTORS.forEach((selectId) => {
        const select = document.getElementById(selectId);
        if (!select) {
            return;
        }

        const groups = new Map();
        contexts.forEach((context) => {
            if (!groups.has(context.group)) {
                groups.set(context.group, []);
            }

            groups.get(context.group).push(context);
        });

        select.innerHTML = '';
        groups.forEach((items, groupLabel) => {
            const optGroup = document.createElement('optgroup');
            optGroup.label = groupLabel;

            items.forEach((context) => {
                const option = document.createElement('option');
                option.value = buildContextValue(context.entityType, context.entitySysId);
                option.textContent = context.label;
                optGroup.appendChild(option);
            });

            select.appendChild(optGroup);
        });

        const hasPreferred = contexts.some((context) => buildContextValue(context.entityType, context.entitySysId) === preferredValue);
        select.value = hasPreferred
            ? preferredValue
            : buildContextValue(projectContext.entityType, projectContext.entitySysId);
    });
}

function getSelectedFloatingContext(selectId) {
    const projectContext = getProjectContext();
    const select = document.getElementById(selectId);
    const selectedValue = select?.value || buildContextValue(projectContext.entityType, projectContext.entitySysId);
    const parsed = parseContextValue(selectedValue);

    if (!parsed.entityType) {
        return {
            entityType: projectContext.entityType,
            entitySysId: projectContext.entitySysId,
            label: projectContext.label
        };
    }

    return {
        entityType: parsed.entityType,
        entitySysId: parsed.entitySysId,
        label: resolveFloatingContextLabel(parsed.entityType, parsed.entitySysId)
    };
}



mainFab.addEventListener("click", () => {
    fabOpen = !fabOpen;
    fabMenu.classList.toggle("show", fabOpen);
    mainFabIcon.classList.toggle("bi-three-dots-vertical", !fabOpen);
    mainFabIcon.classList.toggle("bi-x-lg", fabOpen);
});


function openModal(modalId) {
    // close the FAB menu when opening a modal
    fabOpen = false;
    fabMenu.classList.remove("show");
    mainFabIcon.classList.remove("bi-x-lg");
    mainFabIcon.classList.add("bi-three-dots-vertical");

    if (modalId == 'chatModal') {
        const roomMeta = {
            room: $('#projectNo').val(),
            roomName: $('#projectName').val(),
            roomIcon: $('#projectIcon').val(),
            roomColor: $('#projectColor').val()
        };


        NavbarChat.openPopup($('#projectNo').val(), roomMeta);

        return;
    }
     


    const modalEl = document.getElementById(modalId);
        populateFloatingContextSelectors(selectedEntityType || 'PROJECT', selectedEntitySysId || '');
    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}
 

// =========================
// API helpers (async)
// =========================

function fetchAttachmentsAsync(projectno) {
    return $.ajax({
        url: getApiRootPath() + `/api/project/${projectno}/attachments`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}

function fetchCommentsAsync(projectno) {
    return $.ajax({
        url: getApiRootPath() + `/api/comments/${projectno}`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}

function fetchNotificationsAsync(projectno) {
    return $.ajax({
        url: getApiRootPath() + `/api/notifications/entity/PROJECT/${projectno}`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}

function fetchChatsAsync(projectno) {
    return $.ajax({
        url: getApiRootPath() + `/api/chats/${projectno}`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}



// =========================
// Attachments / Refresh
// =========================
// Dropzone setup
Dropzone.autoDiscover = false;
document.addEventListener("DOMContentLoaded", function () {
    Dropzone.autoDiscover = false;
    const dzElement = document.getElementById("attachmentsDropzone");
    if (dzElement) {
        const dz = new Dropzone("#attachmentsDropzone", {
            url: getApiRootPath() + "/api/files/upload",
            maxFilesize: 10,
            headers: {
                ...(pulseJwtToken
                    ? { Authorization: `Bearer ${pulseJwtToken}` }
                    : {})
            },
            addRemoveLinks: true,
            paramName: "file",
            parallelUploads: 3,
            acceptedFiles: ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.zip",
            init: function () {

                this.on('sending', function (file, xhr, formData) {
                    const ctxId = document.getElementById('attachmentsContextId').value;
                    const selectedContext = getSelectedFloatingContext('attachmentsTargetContext');
                    formData.append('contextId', ctxId);
                    formData.append("projectno", $('#projectNo').val());
                    formData.append("entitytype", selectedContext.entityType);
                    formData.append("entitysysid", selectedContext.entitySysId);
                });

                this.on('success', function (file, response) {

                    response.filesMeta.forEach(f => {
                        //const ctxId = document.getElementById('attachmentsContextId').value;
                        const meta = {
                            'fileName': file.name,
                            'attachmentSysId': f.attachmentSysId,
                            'safeName': f.attachmentSysId + f.altFileName,
                            'fileType': file.type,
                            'fileSize': file.size,
                            'fileExtension': f.fileExtension,
                            'entityType': f.entityType,
                            'entitySysId': f.entitySysId,
                            'createdBy': `${f.createdByMeta.firstName || ''} ${f.createdByMeta.lastName || ''}`.trim(),
                            'createdDate': f.createdDate,
                            'canManageAttachment': Boolean(f.canManageAttachment)
                        }
                        addAttachmentItem(file.name, resolveFloatingContextLabel(f.entityType, f.entitySysId), meta);
                        //console.log("Upload success:", response);
                    });



                });



                this.on("successmultiple", function (files, response) {
                    console.log("Upload success:", response);
                });

                this.on("errormultiple", function (files, errorMessage) {
                    console.error("Upload error:", errorMessage);
                });
            }
        });

        const doneBtn = document.getElementById('attachmentsDoneBtn');
        if (doneBtn) {
            doneBtn.addEventListener('click', function () {
                dz.removeAllFiles(true); // true = also cancel any in-progress uploads
            });
        }
    }
});

function builderAttachments(data) {
    const list = document.getElementById('attachmentsList');
    if (list) {
        list.innerHTML = '';
    }

    data.forEach(f => {
        const metaJson = f.metaJson ? JSON.parse(f.metaJson) : {};
        const meta = metaJson.meta || metaJson;
        const contextLabel = resolveFloatingContextLabel(f.entityType || meta.entityType, f.entitySysId || meta.entitySysId);
        addAttachmentItem(f.fileName, contextLabel, {
            ...meta,
            attachmentSysId: f.attachmentSysId || f.AttachmentSysId,
            projectNo: f.projectNo || f.ProjectNo,
            altFileName: f.altFileName || f.AltFileName,
            canManageAttachment: Boolean(f.canManageAttachment ?? f.CanManageAttachment),
            entityType: f.entityType || meta.entityType,
            entitySysId: f.entitySysId || meta.entitySysId
        });
    });

    if (list && !list.children.length) {
        list.innerHTML = '<div class="side-empty-state">No attachments yet.</div>';
    }

}

function addAttachmentItem(filename, contextId, meta) {
    const list = document.getElementById('attachmentsList');
    if (!list) {
        return;
    }

    const item = document.createElement('div');
    item.className = 'attachment-item';
    const projectNo = $('#projectNo').val();
    if (meta) {

        const createdDate = formatDate(meta.createdDate, "MMM DD, YYYY hh:mm");
        const downloadUrl = `/files/${projectNo}/${meta.safeName}`;
        const actions = [`<a class="btn btn-outline-secondary btn-sm action-link" target="_blank" href="${downloadUrl}">Download</a>`];

        if (meta.canManageAttachment) {
            actions.push('<button type="button" class="btn btn-outline-secondary btn-sm" data-attachment-action="replace">Reupload</button>');
            actions.push('<button type="button" class="btn btn-outline-danger btn-sm" data-attachment-action="delete">Delete</button>');
        }

        item.innerHTML = `
        <div class="attachment-main">
            <div class="attachment-icon"><i class="bi bi-paperclip"></i></div>
            <div class="attachment-copy">
                <a class="no-underline attachment-name" target="_blank" href="${downloadUrl}">${escapeHtml(filename)}</a>
                <div class="attachment-meta">Context: ${escapeHtml(contextId || 'N/A')} • ${meta.createdBy || 'Unknown'} • ${createdDate}</div>
            </div>
        </div>
        <div class="attachment-actions">${actions.join('')}</div>
      `;

        if (meta.canManageAttachment) {
            const replaceBtn = item.querySelector('[data-attachment-action="replace"]');
            const deleteBtn = item.querySelector('[data-attachment-action="delete"]');

            if (replaceBtn) {
                replaceBtn.addEventListener('click', async function () {
                    const file = await promptProjectAttachmentFileSelectionAsync();
                    if (!file) {
                        return;
                    }

                    try {
                        await replaceProjectAttachmentAsync(meta.attachmentSysId, file);
                        const attachments = await fetchAttachmentsAsync($('#projectNo').val());
                        builderAttachments(attachments);
                    } catch (error) {
                        console.error(error);
                        alert('Failed to replace attachment.');
                    }
                });
            }

            if (deleteBtn) {
                deleteBtn.addEventListener('click', async function () {
                    if (!window.confirm(`Delete ${filename || 'this attachment'}?`)) {
                        return;
                    }

                    try {
                        await deleteProjectAttachmentAsync(meta.attachmentSysId);
                        const attachments = await fetchAttachmentsAsync($('#projectNo').val());
                        builderAttachments(attachments);
                    } catch (error) {
                        console.error(error);
                        alert('Failed to delete attachment.');
                    }
                });
            }
        }
    }
    else {
        item.innerHTML = `
        <div class="attachment-main">
            <div class="attachment-icon"><i class="bi bi-paperclip"></i></div>
            <div class="attachment-copy">
                <div class="attachment-name">${escapeHtml(filename)}</div>
                <div class="attachment-meta">Context: ${escapeHtml(contextId || 'N/A')}</div>
            </div>
        </div>
      `;
    }

    const emptyState = list.querySelector('.side-empty-state');
    if (emptyState) {
        emptyState.remove();
    }

    list.prepend(item);
}

function promptProjectAttachmentFileSelectionAsync() {
    return new Promise(function (resolve) {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.zip';
        input.addEventListener('change', function () {
            resolve(input.files && input.files.length ? input.files[0] : null);
        }, { once: true });
        input.click();
    });
}

async function deleteProjectAttachmentAsync(attachmentId) {
    const response = await fetch(`${PROJECT_ATTACHMENT_API_BASE}/${encodeURIComponent(attachmentId)}`, {
        method: 'DELETE',
        headers: {
            "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
        }
    });

    if (!response.ok) {
        throw new Error('Failed to delete attachment');
    }
}

async function replaceProjectAttachmentAsync(attachmentId, file) {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${PROJECT_ATTACHMENT_API_BASE}/${encodeURIComponent(attachmentId)}/replace`, {
        method: 'POST',
        headers: {
            "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
        },
        body: formData
    });

    if (!response.ok) {
        throw new Error('Failed to replace attachment');
    }

    return await response.json();
}


// =========================
// Comments / Refresh
// =========================
function addCommentItem(text, contextId, meta) {
    const list = document.getElementById('commentsList');
    if (!list) {
        return;
    }

    const item = document.createElement('div');
    item.className = 'comment-item';
    const createdDate = formatDate(meta.createdDate, "MMM DD, YYYY hh:mm");
    const author = meta.createdBy || 'Unknown';
    const initials = author.split(' ').map(part => part && part[0]).join('').slice(0, 2).toUpperCase() || 'NA';

    item.innerHTML = `
        <div class="avatar"></div>
        <div class="comment-body">
            <div class="comment-header">
                <div class="comment-author"></div>
                <div class="comment-time"></div>
            </div>
            <div class="comment-context"></div>
            <div class="comment-text"></div>
        </div>
      `;

    item.querySelector('.avatar').textContent = initials;
    item.querySelector('.comment-author').textContent = author;
    item.querySelector('.comment-time').textContent = createdDate;
    item.querySelector('.comment-context').textContent = `Context: ${contextId || 'N/A'}`;

    const commentTextElement = item.querySelector('.comment-text');
    const richTextHtml = meta.commentsRichText || '';
    if (richTextHtml) {
        commentTextElement.classList.add('comment-text-rich');
        commentTextElement.innerHTML = sanitizeCommentHtml(richTextHtml);
    } else {
        commentTextElement.textContent = text || '';
    }

    const emptyState = list.querySelector('.side-empty-state');
    if (emptyState) {
        emptyState.remove();
    }

    list.prepend(item);
    updateCommentSummary();
}

function updateCommentSummary() {
    const list = document.getElementById('commentsList');
    const countElement = document.getElementById('comment-count');
    const summaryElement = document.getElementById('comment-summary');

    if (!list) {
        return;
    }

    const count = list.querySelectorAll('.comment-item').length;
    if (countElement) {
        countElement.textContent = String(count);
    }

    if (summaryElement) {
        summaryElement.textContent = count === 0
            ? 'Start the conversation.'
            : `${count} comment${count === 1 ? '' : 's'} in this thread.`;
    }
}


function builderComments(data) {
    const list = document.getElementById('commentsList');
    if (list) {
        list.innerHTML = '';
    }

    data.forEach(f => {
        const metaJson = f.metaJson ? JSON.parse(f.metaJson) : {};
        const meta = metaJson.meta || metaJson;
        const contextLabel = resolveFloatingContextLabel(f.entityType || meta.entityType, f.entitySysId || meta.entitySysId);
        addCommentItem(f.comments || stripHtml(f.commentsRichText || ''), contextLabel, {
            ...meta,
            commentsRichText: f.commentsRichText || meta.commentsRichText,
            entityType: f.entityType || meta.entityType,
            entitySysId: f.entitySysId || meta.entitySysId
        });
    });

    if (list && !list.children.length) {
        list.innerHTML = '<div class="side-empty-state">No comments yet.</div>';
    }

    updateCommentSummary();

}

document.addEventListener('DOMContentLoaded', function () {

    const commentForm = document.getElementById('formAddComment');
    if (!commentForm) return;

    const commentPlainButton = document.getElementById('comment-mode-plain');
    const commentRichButton = document.getElementById('comment-mode-rich');
    const commentPlainEntry = document.getElementById('comment-entry-plain');
    const commentRichEntry = document.getElementById('comment-entry-rich');
    const commentPlainInput = commentForm.querySelector('input[name="commentText"]');
    const $commentRichInput = $('#comment-input-rich');
    let commentMode = 'plain';

    if ($commentRichInput.length) {
        $commentRichInput.summernote({
            height: 140,
            disableDragAndDrop: true,
            toolbar: [
                ['style', ['bold', 'italic', 'underline', 'clear']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['insert', ['link']],
                ['view', ['codeview']]
            ]
        });
    }

    function toggleCommentMode(mode) {
        commentMode = mode === 'rich' ? 'rich' : 'plain';
        const isRichMode = commentMode === 'rich';

        if (commentPlainButton) {
            commentPlainButton.classList.toggle('is-active', !isRichMode);
            commentPlainButton.setAttribute('aria-pressed', String(!isRichMode));
        }

        if (commentRichButton) {
            commentRichButton.classList.toggle('is-active', isRichMode);
            commentRichButton.setAttribute('aria-pressed', String(isRichMode));
        }

        if (commentPlainEntry) {
            commentPlainEntry.classList.toggle('d-none', isRichMode);
        }

        if (commentPlainInput) {
            commentPlainInput.required = !isRichMode;
        }

        if (commentRichEntry) {
            commentRichEntry.classList.toggle('d-none', !isRichMode);
        }

        if (isRichMode && $commentRichInput.length) {
            $commentRichInput.summernote('focus');
        } else if (commentPlainInput) {
            commentPlainInput.focus();
        }
    }

    if (commentPlainButton) {
        commentPlainButton.addEventListener('click', function () {
            toggleCommentMode('plain');
        });
    }

    if (commentRichButton) {
        commentRichButton.addEventListener('click', function () {
            toggleCommentMode('rich');
        });
    }

    toggleCommentMode('plain');

    commentForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const form = e.currentTarget;
        const data = Object.fromEntries(new FormData(form));
        const selectedContext = getSelectedFloatingContext('commentTargetContext');
        const submitBtn = form.querySelector('button[type="submit"]');
        const richTextRaw = $commentRichInput.length ? $commentRichInput.summernote('code') : '';
        const richTextHtml = sanitizeCommentHtml(richTextRaw);
        const richTextPlain = stripHtml(richTextHtml);
        const plainComment = String(data.commentText || '').trim();
        const commentText = commentMode === 'rich' ? richTextPlain : plainComment;
        const commentRichText = commentMode === 'rich' ? richTextHtml : null;

        if (!commentText) {
            alert('Comment text is required.');
            return;
        }

        if (submitBtn) submitBtn.disabled = true;

        $.ajax({
            url: getApiRootPath() + '/api/comments/add',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({
                projectno: $('#projectNo').val(),
                entityType: selectedContext.entityType,
                entitySysId: selectedContext.entitySysId,
                comments: commentText,
                commentsRichText: commentRichText
            }),
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function (result) {
                if (result && result.success) {

                    const meta = {
                        'comments': result.data.comments,
                        'commentsRichText': result.data.commentsRichText,
                        'entityType': result.data.entityType,
                        'entitySysId': result.data.entitySysId,
                        'createdBy': result.data.createdByMeta.firstName,
                        'createdDate': result.data.createdDate
                    }


                    addCommentItem(result.data.comments, resolveFloatingContextLabel(result.data.entityType, result.data.entitySysId), meta);
                    form.reset();
                    if ($commentRichInput.length) {
                        $commentRichInput.summernote('code', '');
                    }
                    toggleCommentMode('plain');
                    const modalElement = document.getElementById('commentModal');
                    if (modalElement) {
                        const modalInstance = bootstrap.Modal.getInstance(modalElement);
                        modalInstance && modalInstance.hide();
                    }
                } else {
                    alert(result && result.error ? result.error : 'Failed to save comment.');
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
});


// =========================
// Notifications / Refresh
// =========================
function addNotificationItem(title, when, contextId, meta) {
    const list = document.getElementById('notificationsList');
    if (!list) {
        return;
    }

    const item = document.createElement('div');
    item.className = 'border rounded-3 p-3 bg-light-subtle mb-2';
    item.innerHTML = `
        <div class="fw-semibold text-dark"><i class="fa fa-bell text-warning me-1"></i>${escapeHtml(title)}</div>
        <div class="meta mt-1">${when ? 'When: ' + escapeHtml(when) + ' • ' : ''}Context: ${escapeHtml(contextId || 'N/A')}</div>
      `;

    const emptyState = document.getElementById('notificationsEmpty');
    if (emptyState) {
        emptyState.classList.add('d-none');
    }

    list.prepend(item);
}

function builderNotifications(data) {
    const list = document.getElementById('notificationsList');
    const emptyState = document.getElementById('notificationsEmpty');

    if (list) {
        list.innerHTML = '';
    }

    data.forEach(f => {
        const metaJson = f.metaJson ? JSON.parse(f.metaJson) : {};
        const meta = metaJson.meta || metaJson;
        const contextLabel = resolveFloatingContextLabel(f.entityType || meta.entityType, f.entitySysId || meta.entitySysId);
        addNotificationItem(f.title, f.notificationDate, contextLabel, {
            ...meta,
            entityType: f.entityType || meta.entityType,
            entitySysId: f.entitySysId || meta.entitySysId
        });
    });

    if (emptyState) {
        if (data && data.length) {
            emptyState.classList.add('d-none');
        } else {
            emptyState.classList.remove('d-none');
            emptyState.textContent = 'No notifications found.';
        }
    }

}

document.addEventListener('DOMContentLoaded', function () {

    const notificationForm = document.getElementById('formAddNotification');
    if (!notificationForm) return;

    notificationForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const form = e.currentTarget;
        const data = Object.fromEntries(new FormData(form));
        const selectedContext = getSelectedFloatingContext('notificationTargetContext');
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        $.ajax({
            url: getApiRootPath() + '/api/notifications/add',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({
                projectno: $('#projectNo').val(),
                entityType: selectedContext.entityType,
                entitySysId: selectedContext.entitySysId,
                title: data.title,
                message: data.message,
                recipients: data.recipients,
                notificationDate: data.when,
                expiryDate: data.endswhen
            }),
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function (result) {
                if (result && result.success) {
                    const data = result.data;
                    const meta = {
                        'title': data.title,
                        'message': data.message,
                        'recipients': data.recipients,
                        'when': data.notificationDate,
                        'entityType': data.entityType,
                        'entitySysId': data.entitySysId,
                        'createdBy': data.createdByMeta.firstName,
                        'createdDate': data.createdDate
                    }

                    /*
                                     Title = model.Title,
                Message = model.Message,
                Recipients = model.Recipients,
                NotificationDate = model.NotificationDate,
                CreatedBy = loggeduser,
                ProjectNo = model.ProjectNo,
                EntitySysId = model.EntitySysId,
                EntityType = model.EntityType,
                CreatedDate = DateTime.Now,
                     */



                    addNotificationItem(data.title, data.notificationDate, resolveFloatingContextLabel(data.entityType, data.entitySysId), meta);
                    form.reset();
                    const modalInstance = bootstrap.Modal.getInstance(document.getElementById('notificationModal'));
                    modalInstance && modalInstance.hide();
                } else {
                    alert(result && result.error ? result.error : 'Failed to save notification.');
                }
            },
            error: function (xhr) {
                console.error('Error adding notification', xhr);
                alert('Error adding notification.');
            },
            complete: function () {
                if (submitBtn) submitBtn.disabled = false;
            }
        });
    });
});


// =========================
// Chats / Refresh
// =========================










 