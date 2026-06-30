// ======================================================
// CONFIG – change these to your real endpoints
// ======================================================
const TASK_ID = $('#ProjectTaskSysId').val();
const $PROJECT_NO = $('#ProjectNo');
const $ROADMAPACTIVITYSYSID = $('#RoadmapActivitySysId');
const ROOT_API_URL = getApiRootPath();
const ROOT_WEB_URL = getAppRootPath();
const API_TASK_DETAILS_URL = `${ROOT_API_URL}/api/ProjectTasks/Details/${TASK_ID}`;
const API_TASK_DETAILS_READONLY_URL = `${ROOT_API_URL}/api/ProjectTasks/DetailsReadonly/${TASK_ID}`;
let IS_TASK_READONLY_MODE = Boolean(window.isTaskReadOnlyPage);
const ADD_COMMENTS_API_BASE = `${ROOT_API_URL}/api/comments/add`
const UPLOAD_ATTACHMENT_API_BASE = `${ROOT_API_URL}/api/files/upload`
const ATTACHMENT_API_BASE = `${ROOT_API_URL}/api/files`
// ======================================================
// Helpers
// ======================================================
function formatDate(iso) {
    if (!iso) return "--";
    const d = new Date(iso);
    return d.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric"
    });
}

function stripHtml(input) {
    const container = document.createElement("div");
    container.innerHTML = input || "";
    return (container.textContent || container.innerText || "").trim();
}

function sanitizeCommentHtml(input) {
    if (!input) {
        return "";
    }

    const template = document.createElement("template");
    template.innerHTML = input;

    const blockedTags = ["script", "style", "iframe", "object", "embed", "form", "input", "button", "textarea", "select", "option", "link", "meta"];
    blockedTags.forEach(tag => {
        template.content.querySelectorAll(tag).forEach(node => node.remove());
    });

    template.content.querySelectorAll("*").forEach(node => {
        Array.from(node.attributes).forEach(attr => {
            const attrName = attr.name.toLowerCase();
            const attrValue = attr.value || "";
            if (attrName.startsWith("on")) {
                node.removeAttribute(attr.name);
                return;
            }

            if ((attrName === "href" || attrName === "src") && /^\s*javascript:/i.test(attrValue)) {
                node.removeAttribute(attr.name);
            }
        });
    });

    return template.innerHTML;
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
 
// Map raw API values to class names and display labels
function getPriorityChip(priority) {
    const p = (priority || "").toLowerCase();
    switch (p) {
        case "high":
            return { label: "High", className: "chip-priority-high" };
        case "medium":
            return { label: "Medium", className: "chip-priority-medium" };
        case "low":
            return { label: "Low", className: "chip-priority-low" };
        default:
            return { label: priority || "N/A", className: "chip-priority-low" };
    }
}

// Map raw API values to class names and display labels
function getIsRequiredChip(required) {
    return required ? { label: "Required", className: "chip-priority-high" } : { label: "Optional", className: "chip-priority-low" };
}

function getStatusChip(status) {
    return {
        label: getPulseStatusText(status, {
            targetDate: currentTask ? (currentTask.targetCompletionDate || currentTask.taskWkFiscalDate || currentTask.projectWkFiscalDate) : null
        }),
        className: getPulseStatusClassName(status, {
            targetDate: currentTask ? (currentTask.targetCompletionDate || currentTask.taskWkFiscalDate || currentTask.projectWkFiscalDate) : null
        })
    };
}

 

function renderAttachment(att) {
    const wrapper = document.createElement("div");
    wrapper.className = "attachment-item";
    const ext = (att.fileName || "").split(".").pop().toUpperCase();
    wrapper.innerHTML = `
        <div class="attachment-main">
          <div class="attachment-icon">${ext || "FILE"}</div>
          <div>
            <div class="attachment-name">${att.fileName}</div>
            <div class="attachment-meta">
              ${att.size || ""} ${att.size ? "&middot;" : ""} 
              ${att.addedBy ? "Added by " + att.addedBy : ""}
            </div>
          </div>
        </div>
        <div class="attachment-actions">
          ${att.canPreview ? `<a class="btn btn-link btn-sm p-0 action-link" href="${att.url}" target="_blank">Preview</a>` : ""}
          <a class="btn btn-link btn-sm p-0 action-link" href="${att.url}" download>Download</a>
          ${att.canManageAttachment ? `<button type="button" class="btn btn-link btn-sm p-0 action-link" data-attachment-action="replace">Reupload</button>` : ""}
          ${att.canManageAttachment ? `<button type="button" class="btn btn-link btn-sm p-0 text-danger action-link" data-attachment-action="delete">Delete</button>` : ""}
        </div>
      `;

    if (att.canManageAttachment) {
        const replaceBtn = wrapper.querySelector('[data-attachment-action="replace"]');
        const deleteBtn = wrapper.querySelector('[data-attachment-action="delete"]');

        if (replaceBtn) {
            replaceBtn.addEventListener('click', async function () {
                const file = await promptAttachmentFileSelectionAsync();
                if (!file) {
                    return;
                }

                try {
                    await replaceAttachmentAsync(att.id, file);
                    await loadAttachments();
                } catch (error) {
                    console.error(error);
                    alert('Failed to replace attachment.');
                }
            });
        }

        if (deleteBtn) {
            deleteBtn.addEventListener('click', async function () {
                if (!window.confirm(`Delete ${att.fileName || 'this attachment'}?`)) {
                    return;
                }

                try {
                    await deleteAttachmentAsync(att.id);
                    await loadAttachments();
                } catch (error) {
                    console.error(error);
                    alert('Failed to delete attachment.');
                }
            });
        }
    }

    return wrapper;
}

function renderComment(c) {
    const wrapper = document.createElement("div");
    wrapper.className = "comment-item";
    const initials = (c.author || "?")
        .split(" ")
        .map(p => p[0])
        .join("")
        .slice(0, 2)
        .toUpperCase();
        wrapper.innerHTML = `
                <div class="avatar"></div>
                <div class="comment-body">
                    <div class="comment-header">
                        <span class="comment-author"></span>
                        <span class="comment-time"></span>
                    </div>
                    <div class="comment-text"></div>
                </div>
            `;

        wrapper.querySelector(".avatar").textContent = initials;
        wrapper.querySelector(".comment-author").textContent = c.author || "Unknown";
        wrapper.querySelector(".comment-time").textContent = c.createdAtLabel || "";

        const commentTextElement = wrapper.querySelector(".comment-text");
        if (c.isRichText && c.richTextHtml) {
                commentTextElement.classList.add("comment-text-rich");
                commentTextElement.innerHTML = sanitizeCommentHtml(c.richTextHtml);
        } else {
                commentTextElement.textContent = c.text || "";
        }

    return wrapper;
}

// =========================
// DataEntry (Forms) / Refresh
// =========================
async function refreshDataEntryUI() {
    const $containerForms = $('#formUpdateForm .section-body');
    const dataEntryButton = document.getElementById('btnDataEntryUpdate');

    if (IS_TASK_READONLY_MODE && dataEntryButton) {
        dataEntryButton.style.display = 'none';
    }

    if (currentDataEntries.length === 0) {
        if (dataEntryButton) {
            dataEntryButton.style.display = 'none';
        }
        $containerForms.html('No Data entry required');
        return;
    }
    if (!IS_TASK_READONLY_MODE && dataEntryButton) {
        dataEntryButton.style.display = '';
    }
    $containerForms.html('<i class="fas fa-spinner fa-spin me-1"></i> Updating data content...');
    let formObj;
    let fields = [];
    currentDataEntries.forEach(f => {
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

    const editableFields = IS_TASK_READONLY_MODE ? normalizedFields : normalizedFields.filter(isFieldActiveForEditing);

    const fetches = [];
    editableFields.forEach(f => {
        const jsonparsed = safeJsonParse(f.values, []);
        if (!Array.isArray(jsonparsed)) return;

        const submissionValue = jsonparsed.find(a =>
            a.entitysysid === $ROADMAPACTIVITYSYSID.val() &&
            a.entitytype === 'activity' &&
            a.formfieldsysid === f.id
        );

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
        mode: IS_TASK_READONLY_MODE ? "READONLY" : undefined,
        emptyMessage: 'No Link Form found for this node.',
        buildFieldDataAttributes: function (field) {
            return {
                "field-form-field-sys-id": field.id ?? field.fieldFormFieldSysId,
                "field-type": field.type ?? field.fieldType,
                "field-is-active": isFieldActiveForEditing(field) ? "true" : "false",
                "field-form-sys-id": field.formSysId ?? field.fieldFormSysId,
                "field-form-entity-link-sys-id": field.formEntityLinkSysId ?? field.fieldFormEntityLinkSysId,
                "field-name": field.name ?? field.fieldName,
                "field-element-sys-id": $('#RoadmapActivitySysId').val(),
                "field-element-type": 'activity',
                "field-submission-sys-id": field.submissionSysId ?? field.fieldSubmissionSysId ?? "",
                "field-submission-transaction-key": field.submissionTransactionKey ?? field.fieldSubmissionTransactionKey ?? "",
                "field-submission-value-sys-id": field.submissionValueSysId ?? field.fieldSubmissionValueSysId ?? "",
                "field-submission-value-transaction-key": field.submissionValueTransactionKey ?? field.fieldSubmissionValueTransactionKey ?? ""
            };
        },
        includeDataAttributesInValues: true,
        includeBuiltDataAttributesInValues: true
    });


  
}
// ======================================================
// API calls
// ======================================================

async function fetchTask() {
    let taskPayload = null;

    async function fetchTaskByUrlAsync(url) {
        const res = await fetch(url,
        {
            method: "GET",
            headers: {
                "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
            }
        }
        );

        if (!res.ok) {
            throw new Error("Network error");
        }

        return await res.json();
    }

    taskPayload = await fetchTaskByUrlAsync(IS_TASK_READONLY_MODE ? API_TASK_DETAILS_READONLY_URL : API_TASK_DETAILS_URL);

    const normalizedTask = normalizeTaskDetailsResponse(taskPayload);
    if (!normalizedTask && !IS_TASK_READONLY_MODE) {
        taskPayload = await fetchTaskByUrlAsync(API_TASK_DETAILS_READONLY_URL);
        IS_TASK_READONLY_MODE = true;
    }

    return taskPayload;
}
 
// GET /comments
async function fetchComments() { 
    const list = await fetchCommentsPerEntityAsync($PROJECT_NO.val(), "TASK", TASK_ID);

    // Map from your API shape to UI shape

    return list.map(mapApiComment);
}

// POST /comments
async function createComment(text, richTextHtml) {
     
    const res = await fetch(ADD_COMMENTS_API_BASE, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
           "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
        },
        body: JSON.stringify({
            projectno: $PROJECT_NO.val(),
            entityType: "TASK",
            entitySysId: TASK_ID,
            comments: text,
            commentsRichText: richTextHtml || null
        })
    });
    if (!res.ok) throw new Error("Failed to create comment");
    const created = await res.json();
    return mapApiComment(created.data || {});
}
 
// { id, authorName, createdAt, text }
function mapApiComment(apiComment) {
    const parsedMeta = JSON.parse(apiComment.metaJson || "{}");
    const obj = parsedMeta.meta || parsedMeta || {};
    const createdByMeta = apiComment.createdByMeta || {};
    const createdAt = apiComment.createdAt || apiComment.created_at || apiComment.createdDate;
    const date = createdAt ? new Date(createdAt) : null;
    const createdAtLabel = date
        ? date.toLocaleString("en-US", {
            month: "short",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        })
        : "";

    const richTextHtml = apiComment.commentsRichText || obj.commentsRichText || "";
    const plainText = apiComment.text || apiComment.comments || obj.comments || stripHtml(richTextHtml);
    const fallbackAuthor = `${createdByMeta.firstName || createdByMeta.FirstName || obj.createdFirstName || ""} ${createdByMeta.lastName || createdByMeta.LastName || obj.createdLastName || ""}`.trim();

    return {
        id: apiComment.id,
        author: apiComment.authorName || apiComment.author || obj.createdBy || fallbackAuthor || "Unknown",
        text: plainText,
        richTextHtml,
        isRichText: Boolean(richTextHtml),
        createdAt: createdAt,
        createdAtLabel
    };
}

// Map ProjectAttachment (from Web API) to renderAttachment() model
function mapApiAttachment(api) {
    const bytes = api.fileSize || api.FileSize || 0;
    const sizeKB = bytes ? Math.round(bytes / 1024) + " KB" : "";

    let meta = {};
    try {
        meta = JSON.parse(api.metaJson || api.MetaJson || "{}");
        meta = meta.meta || meta;
    } catch (error) {
        meta = {};
    }

    const createdMeta = api.createdByMeta || api.CreatedByMeta || {};
    const firstName = createdMeta.firstName || createdMeta.FirstName || "";
    const lastName = createdMeta.lastName || createdMeta.LastName || "";
    const addedBy = (firstName + " " + lastName).trim() || meta.createdBy || api.createdBy || api.CreatedBy || "";

    // Build a URL to download the file.
    // Adjust this according to your download endpoint.
    const attachmentId = api.attachmentSysId || api.AttachmentSysId;
    const safeName = api.altFileName || api.AltFileName || "";
    const downloadUrl =
        `${ROOT_WEB_URL}/files/${encodeURIComponent($PROJECT_NO.val())}/${encodeURIComponent(attachmentId)}${encodeURIComponent(safeName)}`;

    return {
        id: attachmentId,
        fileName: api.fileName || api.FileName || meta.fileName || safeName,
        size: sizeKB,
        addedBy,
        url: downloadUrl,
        canPreview: (api.fileType || api.FileType || "").startsWith("image/") ||
            (api.fileType || api.FileType || "").includes("pdf"),
        canManageAttachment: Boolean(api.canManageAttachment ?? api.CanManageAttachment)
    };
}

function promptAttachmentFileSelectionAsync() {
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

async function deleteAttachmentAsync(attachmentId) {
    const response = await fetch(`${ATTACHMENT_API_BASE}/${encodeURIComponent(attachmentId)}`, {
        method: 'DELETE',
        headers: {
            "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
        }
    });

    if (!response.ok) {
        throw new Error('Failed to delete attachment');
    }
}

async function replaceAttachmentAsync(attachmentId, file) {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${ATTACHMENT_API_BASE}/${encodeURIComponent(attachmentId)}/replace`, {
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

// ======================================================
// State + UI refresh
// ======================================================
let currentTask = null;
let currentComments = [];
let currentDataEntries = null;

function normalizeTaskDetailsResponse(taskResponse) {
    if (Array.isArray(taskResponse)) {
        return taskResponse[0] || null;
    }

    if (!taskResponse || typeof taskResponse !== "object") {
        return null;
    }

    if (Array.isArray(taskResponse.data)) {
        return taskResponse.data[0] || null;
    }

    if (Array.isArray(taskResponse.items)) {
        return taskResponse.items[0] || null;
    }

    return taskResponse.data || taskResponse.item || taskResponse;
}

async function loadTask() {
    const taskResponse = await fetchTask();
    currentTask = normalizeTaskDetailsResponse(taskResponse);

    if (!currentTask || typeof currentTask !== "object") {
        document.getElementById("task-title").textContent = "Task details unavailable";
        document.getElementById("task-description").textContent = "The requested task could not be found or you may no longer have access.";
        document.getElementById("detail-due-date").textContent = "--";
        document.getElementById("detail-status").textContent = "--";
        return false;
    }

    const dataEntryButton = document.getElementById("btnDataEntryUpdate");
    if (dataEntryButton && IS_TASK_READONLY_MODE) {
        dataEntryButton.style.display = "none";
    }



    // Basic fields
    document.getElementById("task-title").textContent = currentTask.activityName || "Task not found";
    document.getElementById("task-description").textContent = currentTask.activityDescription || "";

    // Due date
    document.getElementById("detail-due-date").textContent = formatDate(currentTask.targetCompletionDate || currentTask.taskWkFiscalDate || currentTask.projectWkFiscalDate);

    // Project Info
    document.getElementById("proj-no-right").textContent = currentTask.projectNo;
    document.getElementById("proj-name-right").textContent = currentTask.projectName || "";
    document.getElementById("proj-desc-right").textContent = currentTask.projectDescription || "";
    document.getElementById("proj-plant-right").textContent = currentTask.plantName || "";
    document.getElementById("proj-category-right").textContent = currentTask.categoryName || "";
    document.getElementById("proj-division-right").textContent = currentTask.productGroupName || "";
    document.getElementById("proj-prod-group-right").textContent = currentTask.productDivisionName || ""; 

 

    // ---- set chips based on API values ----
    //const priorityChip = getPriorityChip(task.priority);
    const isRequiredChip = getIsRequiredChip(currentTask.isRequired === 1);
    const statusChip = getStatusChip(currentTask.status);

    //const chipPriorityEl = document.getElementById("chip-priority");
    const chipIsRequiredEl = document.getElementById("chip-isrequired");
    const chipStatusEl = document.getElementById("chip-status");

    // reset classes first (keep base "chip")
    //chipPriorityEl.className = "chip " + priorityChip.className;
    chipIsRequiredEl.className = "chip " + isRequiredChip.className;
    chipStatusEl.className = statusChip.className;

    //chipPriorityEl.textContent = priorityChip.label;
    chipIsRequiredEl.textContent = isRequiredChip.label;
    chipStatusEl.textContent = statusChip.label;

    // (optional) also update detail fields in the sidebar
    //document.getElementById("detail-priority").textContent = priorityChip.label;
    //document.getElementById("detail-isrequired").textContent = isRequiredChip.label;
    document.getElementById("detail-status").textContent = statusChip.label;

    $PROJECT_NO.val(currentTask.projectNo);
    $ROADMAPACTIVITYSYSID.val(currentTask.roadmapActivitySysId);



     
    const attachmentList = document.getElementById("attachment-list");
    attachmentList.innerHTML =
        '<div class="section-body">No attachments</div>';

    return true;
}

async function loadComments() {
    currentComments = await fetchComments();
    refreshCommentsUI();
}

async function loadDataEntry() {
    currentDataEntries = await fetchProjectDataAsync($('#ProjectNo').val(), "activity", $('#RoadmapActivitySysId').val())
    refreshDataEntryUI();
}

async function loadAttachments() {
    const attachmentList = document.getElementById("attachment-list");
    attachmentList.innerHTML = '<div class="section-body">Loading attachments...</div>';

    try {
        const projectNo = $PROJECT_NO.val();
        // Your GET: /api/project/{projectno}/attachments/{entitytype}/{entitysysid}
        const url = `${ROOT_API_URL}/api/project/${encodeURIComponent(projectNo)}/attachments/${encodeURIComponent("TASK")}/${encodeURIComponent(TASK_ID)}`;

        const res = await fetch(url, {
            method: "GET",
            headers: {
                "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
            }
        });

        if (!res.ok) throw new Error("Failed to load attachments");

        const json = await res.json();
        const items = (json.data || []).map(mapApiAttachment);

        if (!items.length) {
            attachmentList.innerHTML = '<div class="section-body">No attachments</div>';
            return;
        }

        attachmentList.innerHTML = "";
        items.forEach(att => attachmentList.appendChild(renderAttachment(att)));
    } catch (err) {
        console.error(err);
        attachmentList.innerHTML =
            '<div class="section-body">Failed to load attachments</div>';
    }
}


function refreshCommentsUI() {
    const commentList = document.getElementById("comment-list");
    commentList.innerHTML = "";
    currentComments.forEach(c =>
        commentList.appendChild(renderComment(c))
    );

    const countEl = document.getElementById("comment-count");
    const summaryEl = document.getElementById("comment-summary");
    countEl.textContent = currentComments.length;
    summaryEl.textContent =
        currentComments.length === 0
            ? "No comments yet"
            : "Latest feedback from the team";
}

// ======================================================
// Dropzone
// ======================================================
Dropzone.autoDiscover = false;

function setupDropzone() {
    const dzElement = document.getElementById("upload-dropzone");

    const dz = new Dropzone(dzElement, {
        url: UPLOAD_ATTACHMENT_API_BASE,        // /api/files/upload
        maxFilesize: 10,
        headers: {
            ...(pulseJwtToken
                ? { Authorization: `Bearer ${pulseJwtToken}` }
                : {})
        },
        paramName: "file",
        parallelUploads: 3,
        acceptedFiles: ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.zip",
        addRemoveLinks: true,
        init: function () {
            this.on("sending", function (file, xhr, formData) {
                // your API requires these form fields
                formData.append("projectno", $PROJECT_NO.val());
                formData.append("entitytype", "TASK");
                formData.append("entitysysid", TASK_ID);
            });

            this.on("success", function (file, response) {
                // response = { ProjectNo, EntityType, EntitySysId, filesMeta, Files, Failed }
                if (!response || !response.filesMeta || !response.filesMeta.length) {
                    // fallback: reload all attachments
                    loadAttachments();
                    return;
                }

                const attachmentList = document.getElementById("attachment-list");

                // Remove "No attachments" placeholder if present
                if (attachmentList.textContent.includes("No attachments")) {
                    attachmentList.innerHTML = "";
                }

                response.filesMeta
                    .map(mapApiAttachment)
                    .forEach(att => attachmentList.appendChild(renderAttachment(att)));
            });

            this.on("error", function (file, errorMessage) {
                console.error("Upload error:", errorMessage);
                alert("Failed to upload file.");
            });
        }
    });
}

function setupUploadToggle() {
    const btn = document.getElementById("upload-toggle");
    const dzForm = document.getElementById("upload-dropzone");
    let visible = false;
    btn.addEventListener("click", () => {
        visible = !visible;
        dzForm.style.display = visible ? "block" : "none";
    });
}

// ======================================================
// Data Entry
// ======================================================
function setupDataEntry() {
    if (IS_TASK_READONLY_MODE) {
        return;
    }

    document.getElementById('formUpdateForm').addEventListener('submit', function (e) {
        e.preventDefault();

        const values = $('#formUpdateForm .section-body').dynamicFieldGetValues();
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
            return;
        }

        const submission = fieldsToSubmit[0];
        const submissionSysId = submission.submissionSysId;
        const trasactionKey = submission.submissionTransactionKey;
        const formData = new FormData();
        const projectNo = $PROJECT_NO.val();

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
                await finalizeAction(autoCloseTask, _saveNode);
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

        builderDataEntry();
    });
}

// ======================================================
// Comment input
// ======================================================
function setupCommentInput() {
    const input = document.getElementById("comment-input");
    const sendBtn = document.getElementById("comment-send");
    const btnPlain = document.getElementById("comment-mode-plain");
    const btnRich = document.getElementById("comment-mode-rich");
    const plainEntry = document.getElementById("comment-entry-plain");
    const richEntry = document.getElementById("comment-entry-rich");
    const $richInput = $("#comment-input-rich");
    let commentMode = "plain";

    if ($richInput.length) {
        $richInput.summernote({
            height: 140,
            disableDragAndDrop: true,
            toolbar: [
                ["style", ["bold", "italic", "underline", "clear"]],
                ["para", ["ul", "ol", "paragraph"]],
                ["insert", ["link"]],
                ["view", ["codeview"]]
            ]
        });
    }

    function toggleCommentMode(mode) {
        commentMode = mode === "rich" ? "rich" : "plain";
        const isRichMode = commentMode === "rich";

        if (btnPlain) {
            btnPlain.classList.toggle("is-active", !isRichMode);
            btnPlain.setAttribute("aria-pressed", String(!isRichMode));
        }

        if (btnRich) {
            btnRich.classList.toggle("is-active", isRichMode);
            btnRich.setAttribute("aria-pressed", String(isRichMode));
        }

        if (plainEntry) {
            plainEntry.classList.toggle("d-none", isRichMode);
        }

        if (richEntry) {
            richEntry.classList.toggle("d-none", !isRichMode);
        }

        if (isRichMode && $richInput.length) {
            $richInput.summernote("focus");
        } else if (input) {
            input.focus();
        }
    }

    async function submitComment() {
        const richTextHtmlRaw = $richInput.length ? $richInput.summernote("code") : "";
        const richTextHtml = sanitizeCommentHtml(richTextHtmlRaw);
        const richTextPlain = stripHtml(richTextHtml);
        const plainText = (input && input.value ? input.value.trim() : "");

        const text = commentMode === "rich" ? richTextPlain : plainText;
        const payloadRichText = commentMode === "rich" ? richTextHtml : null;

        if (!text) {
            return;
        }

        try {
            // POST to API
            const created = await createComment(text, payloadRichText);
            // add newest at top
            currentComments.unshift(created);
            refreshCommentsUI();
            if (input) {
                input.value = "";
            }

            if ($richInput.length) {
                $richInput.summernote("code", "");
            }

            toggleCommentMode("plain");
        } catch (err) {
            console.error("Failed to create comment", err);
            // here you could show a toast / inline error
            alert("Failed to add comment.");
        }
    }

    sendBtn.addEventListener("click", e => {
        e.preventDefault();
        submitComment();
    });

    if (input) {
        input.addEventListener("keydown", e => {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                submitComment();
            }
        });
    }

    if (btnPlain) {
        btnPlain.addEventListener("click", function () {
            toggleCommentMode("plain");
        });
    }

    if (btnRich) {
        btnRich.addEventListener("click", function () {
            toggleCommentMode("rich");
        });
    }

    toggleCommentMode("plain");
}
// ======================================================
// Init
// ======================================================
document.addEventListener("DOMContentLoaded", async () => {
    let hasTaskDetails = false;

    try {
        initJwtToken();

        hasTaskDetails = await loadTask();
        if (hasTaskDetails) {
            await loadDataEntry();
            await loadComments();
            await loadAttachments();
        }
    } catch (err) {
        console.error(err);
    }

    if (hasTaskDetails) {
        setupDropzone();
        setupUploadToggle();
        setupCommentInput();
        if (!IS_TASK_READONLY_MODE) {
            setupDataEntry();
        }
    }
});