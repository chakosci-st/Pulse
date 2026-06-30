var report3ApiPath = getApiRootPath();

const report3State = {
    projects: [],
    filteredProjects: [],
    selectedProjectNos: [],
    nodesCache: {},
    comparisonCache: {},
    supportCache: {},
    taskFormsCache: {},
    submissionValueCache: {},
    compareRequestId: 0
};

function report3EscapeHtml(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function report3ToNumber(value) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
}

function report3FormatDate(value, format) {
    if (!value) {
        return '-';
    }

    const parsed = moment(value);
    return parsed.isValid() ? parsed.format(format || 'YYYY-MM-DD') : '-';
}

function report3SafeJsonParse(value, fallback) {
    if (!value) {
        return fallback;
    }

    if (Array.isArray(value) || typeof value === 'object') {
        return value;
    }

    try {
        return JSON.parse(value);
    } catch (error) {
        return fallback;
    }
}

function report3DecodeReadonlyRichTextHtml(input) {
    let html = String(input || '');
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

function report3SanitizeReadonlyRichTextHtml(input) {
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

function report3RenderReadonlyRichTextAsContainedHtml($container) {
    if (!$container || !$container.length) {
        return;
    }

    $container.find('[data-field-type="richtext"]').each(function () {
        const $field = $(this);
        const $textarea = $field.find('textarea').first();
        const editorHtml = $field.find('.note-editable').first().html() || '';
        const rawHtml = ($textarea.length ? $textarea.val() : '') || editorHtml;
        const sanitizedHtml = report3SanitizeReadonlyRichTextHtml(report3DecodeReadonlyRichTextHtml(rawHtml));

        $field.find('.note-editor').remove();
        $textarea.remove();

        const $display = $('<div class="report3-richtext-readonly report3-comment-text"></div>');
        $display.html(sanitizedHtml);
        $field.append($display);
    });
}

function report3GetOwnerName(record) {
    return `${record.projectOwnerFirstName || ''} ${record.projectOwnerLastName || ''}`.trim() || '-';
}

function report3GetStatusText(status, options) {
    if (typeof getPulseStatusText === 'function') {
        return getPulseStatusText(status, options || {});
    }

    return String(status || 'Unknown');
}

function report3GetStatusBadge(status, options) {
    if (typeof getPulseStatusBadge === 'function') {
        return getPulseStatusBadge(status, options || {});
    }

    return `<span class="badge bg-secondary">${report3EscapeHtml(report3GetStatusText(status, options))}</span>`;
}

function report3GetProjectIcon(record) {
    return record.projectIcon || 'bi bi-kanban-fill';
}

function report3GetProjectColor(record) {
    return record.projectIconColor || '#0f766e';
}

function report3GetNodeTargetDate(node) {
    return node.projectNodeTargetCompletionDate || node.projectNodeTargetCompletion || node.targetCompletion || null;
}

function report3GetNodeStartDate(node) {
    return node.projectNodeTargetStartDate || node.projectNodeTargetStart || node.targetStart || null;
}

function report3NormalizeStatus(statusText) {
    const normalized = String(statusText || '').trim().toUpperCase();

    if (!normalized) {
        return 'UNKNOWN';
    }

    if (normalized.indexOf('NOT STARTED') !== -1) {
        return 'NOT_STARTED';
    }

    if (normalized.indexOf('RISK') !== -1) {
        return 'AT_RISK';
    }

    if (normalized.indexOf('CANCEL') !== -1) {
        return 'CANCELLED';
    }

    if (normalized.indexOf('COMPLETED') !== -1) {
        return 'COMPLETED';
    }

    if (normalized.indexOf('ONGOING') !== -1 || normalized.indexOf('IN PROGRESS') !== -1) {
        return 'ONGOING';
    }

    return normalized.replace(/\s+/g, '_');
}

function report3ParseOwners(rawOwners) {
    const owners = Array.isArray(rawOwners)
        ? rawOwners
        : (report3SafeJsonParse(rawOwners, []) || []);

    return owners.map(function (owner) {
        return {
            userId: owner.userid || owner.userId || owner.EmployeeId || '',
            name: `${owner.firstname || owner.firstName || ''} ${owner.lastname || owner.lastName || ''}`.trim()
        };
    }).filter(function (owner) {
        return owner.name || owner.userId;
    });
}

function report3ParsePrerequisites(rawPrerequisites) {
    const parsed = report3SafeJsonParse(rawPrerequisites, { prerequisites: [] }) || { prerequisites: [] };
    return Array.isArray(parsed.prerequisites) ? parsed.prerequisites : [];
}

function report3FetchProjectDataAsync(projectNo, nodeType, nodeId) {
    return $.ajax({
        url: `${report3ApiPath}/api/projects/${encodeURIComponent(projectNo)}/data/${encodeURIComponent(nodeType)}/${encodeURIComponent(nodeId)}`,
        type: 'GET',
        contentType: 'application/json',
        dataType: 'json'
    }).then(function (response) {
        return response.data || [];
    });
}

function report3FetchSubmissionValueAsync(submissionValueSysId) {
    if (!submissionValueSysId) {
        return $.Deferred().resolve(null).promise();
    }

    if (Object.prototype.hasOwnProperty.call(report3State.submissionValueCache, submissionValueSysId)) {
        return $.Deferred().resolve(report3State.submissionValueCache[submissionValueSysId]).promise();
    }

    return $.ajax({
        url: `${report3ApiPath}/api/ProjectForms/submissions/value/${encodeURIComponent(submissionValueSysId)}`,
        type: 'GET',
        cache: false,
        dataType: 'json'
    }).then(function (response) {
        report3State.submissionValueCache[submissionValueSysId] = response;
        return response;
    });
}

function report3NormalizeFormField(field, formName) {
    return Object.assign({}, field, {
        formName: formName,
        isrequired: field.isrequired === true || field.isrequired === 'true',
        urlIsParameter: field.urlIsParameter === true || field.urlIsParameter === 'true'
    });
}

function report3GetTaskFormCacheKey(projectNo, nodeType, nodeId) {
    return [projectNo, nodeType, nodeId].map(function (value) {
        return String(value == null ? '' : value).trim();
    }).join('::');
}

function report3NormalizeFormComparisonId(value) {
    return String(value == null ? '' : value).trim();
}

function report3NormalizeFormComparisonType(value) {
    const normalized = String(value == null ? '' : value).trim().toLowerCase();
    if (normalized === 'task') {
        return 'activity';
    }

    return normalized;
}

function report3FindSubmissionValueByContext(values, nodeType, nodeIds, fieldId) {
    if (!Array.isArray(values)) {
        return null;
    }

    const normalizedNodeType = report3NormalizeFormComparisonType(nodeType);
    const normalizedFieldId = report3NormalizeFormComparisonId(fieldId);
    const normalizedNodeIds = (Array.isArray(nodeIds) ? nodeIds : [nodeIds])
        .map(report3NormalizeFormComparisonId)
        .filter(Boolean);

    return values.find(function (entry) {
        return normalizedNodeIds.indexOf(report3NormalizeFormComparisonId(entry.entitysysid)) !== -1 &&
            report3NormalizeFormComparisonType(entry.entitytype) === normalizedNodeType &&
            report3NormalizeFormComparisonId(entry.formfieldsysid) === normalizedFieldId;
    });
}

function report3LoadTaskForms(projectNo, task) {
    const nodeType = task.nodeType || 'activity';
    const nodeId = task.nodeId;
    const cacheKey = report3GetTaskFormCacheKey(projectNo, nodeType, nodeId);

    if (Object.prototype.hasOwnProperty.call(report3State.taskFormsCache, cacheKey)) {
        return $.Deferred().resolve(report3State.taskFormsCache[cacheKey]).promise();
    }

    return report3FetchProjectDataAsync(projectNo, nodeType, nodeId)
        .then(function (validForms) {
            let fields = [];

            (validForms || []).forEach(function (formLink) {
                let formObject = null;
                try {
                    formObject = JSON.parse(formLink.formJson);
                } catch (error) {
                    formObject = null;
                }

                if (!formObject || !Array.isArray(formObject.fields)) {
                    return;
                }

                const formName = formObject.name || formObject.title || formObject.formName || formLink.formName || 'Form';
                fields = fields.concat(formObject.fields.map(function (field) {
                    const normalizedField = report3NormalizeFormField(field, formName);
                    normalizedField.formSysId = formLink.formSysId;
                    normalizedField.formEntityLinkSysId = formLink.formEntityLinkSysId;
                    return normalizedField;
                }));
            });

            const submissionFetches = [];
            fields.forEach(function (field) {
                const values = report3SafeJsonParse(field.values, []);
                if (!Array.isArray(values)) {
                    return;
                }

                const submissionValue = report3FindSubmissionValueByContext(values, nodeType, nodeId, field.id);

                if (!submissionValue || !submissionValue.id) {
                    return;
                }

                submissionFetches.push(
                    report3FetchSubmissionValueAsync(submissionValue.id).then(function (data) {
                        const clobValue = data
                            ? (data.fieldValueClob != null
                                ? data.fieldValueClob
                                : (data.FieldValueClob != null
                                    ? data.FieldValueClob
                                    : null))
                            : null;

                        const scalarValue = data
                            ? (data.fieldValue != null
                                ? data.fieldValue
                                : (data.FieldValue != null
                                    ? data.FieldValue
                                    : (data.value != null
                                        ? data.value
                                        : (data.Value != null ? data.Value : null))))
                            : null;

                        field.defaultValue = String(field.type || '').toLowerCase() === 'richtext'
                            ? (clobValue != null ? clobValue : scalarValue)
                            : scalarValue;
                        field.defaultClobValue = clobValue;
                    })
                );
            });

            return $.when.apply($, submissionFetches).then(function () {
                const $containerForms = $('<div></div>');

                $containerForms.dynamicField({
                    fields: fields,
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
                            'field-type': field.type
                        };
                    }
                });

                report3RenderReadonlyRichTextAsContainedHtml($containerForms);

                const formDetails = {
                    markup: $containerForms.html(),
                    withField: fields.length > 0,
                    pendingRequiredFields: fields.some(function (field) {
                        return field.isrequired === true &&
                            (!field.defaultValue && field.defaultValue !== 0 && !field.defaultClobValue && field.defaultClobValue !== 0);
                    })
                };

                report3State.taskFormsCache[cacheKey] = formDetails;
                return formDetails;
            });
        })
        .then(function (formDetails) {
            return formDetails;
        }, function () {
            const fallback = {
                markup: '',
                withField: false,
                pendingRequiredFields: false
            };
            report3State.taskFormsCache[cacheKey] = fallback;
            return fallback;
        });
}

function report3EnrichTimelineForms(projectNo, milestones) {
    const taskLoads = [];

    milestones.forEach(function (milestone) {
        milestone.tasks.forEach(function (task) {
            taskLoads.push(
                report3LoadTaskForms(projectNo, task).then(function (formDetails) {
                    task.formDetails = formDetails || { markup: '', withField: false, pendingRequiredFields: false };
                })
            );
        });
    });

    return $.when.apply($, taskLoads).then(function () {
        return milestones;
    });
}

function report3NormalizeCollection(items) {
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

function report3NormalizeContextType(value) {
    return String(value || '').trim().toUpperCase();
}

function report3NormalizeContextId(value) {
    return String(value || '').trim();
}

function report3ParseMetaJson(rawMeta) {
    if (!rawMeta) {
        return {};
    }

    const parsed = report3SafeJsonParse(rawMeta, {});
    if (!parsed || typeof parsed !== 'object') {
        return {};
    }

    return parsed.meta || parsed;
}

function report3StripHtml(value) {
    const holder = document.createElement('div');
    holder.innerHTML = String(value || '');
    return (holder.textContent || holder.innerText || '').trim();
}

function report3DecodeHtmlEntities(value) {
    const holder = document.createElement('textarea');
    holder.innerHTML = String(value || '');
    return holder.value;
}

function report3SanitizeCommentHtml(input) {
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
                return;
            }

            if (attrName === 'style') {
                const sanitizedStyle = report3SanitizeInlineStyle(attrValue);
                if (!sanitizedStyle) {
                    node.removeAttribute(attr.name);
                } else {
                    node.setAttribute('style', sanitizedStyle);
                }
            }
        });
    });

    return template.innerHTML;
}

function report3SanitizeInlineStyle(styleValue) {
    const rawStyle = String(styleValue || '').trim();
    if (!rawStyle) {
        return '';
    }

    // Guard against scriptable or browser-specific executable CSS fragments.
    const unsafePattern = /(expression\s*\(|javascript\s*:|vbscript\s*:|behavior\s*:|@import|url\s*\()/i;
    if (unsafePattern.test(rawStyle)) {
        return '';
    }

    return rawStyle;
}

function report3BuildAttachmentDownloadUrl(projectNo, attachment) {
    const safeName = attachment.safeName || '';
    if (!safeName) {
        return '';
    }

    return `/files/${encodeURIComponent(projectNo)}/${encodeURIComponent(safeName)}`;
}

function report3MapAttachment(projectNo, item) {
    const meta = report3ParseMetaJson(item.metaJson);
    const attachmentSysId = item.attachmentSysId || item.AttachmentSysId || meta.attachmentSysId || '';
    const altFileName = item.altFileName || item.AltFileName || meta.altFileName || '';
    const safeName = item.safeName || meta.safeName || ((attachmentSysId && altFileName) ? `${attachmentSysId}${altFileName}` : '');
    const entityType = report3NormalizeContextType(item.entityType || item.EntityType || meta.entityType || meta.EntityType);
    const entitySysId = report3NormalizeContextId(item.entitySysId || item.EntitySysId || meta.entitySysId || meta.EntitySysId);
    const createdBy = item.createdBy || meta.createdBy || `${(meta.createdByMeta && meta.createdByMeta.firstName) || ''} ${(meta.createdByMeta && meta.createdByMeta.lastName) || ''}`.trim() || 'Unknown';

    return {
        fileName: item.fileName || meta.fileName || altFileName || 'Attachment',
        createdDate: item.createdDate || meta.createdDate || null,
        createdBy: createdBy,
        entityType: entityType,
        entitySysId: entitySysId,
        safeName: safeName,
        downloadUrl: report3BuildAttachmentDownloadUrl(projectNo, { safeName: safeName })
    };
}

function report3MapComment(item) {
    const meta = report3ParseMetaJson(item.metaJson);
    const entityType = report3NormalizeContextType(item.entityType || item.EntityType || meta.entityType || meta.EntityType);
    const entitySysId = report3NormalizeContextId(item.entitySysId || item.EntitySysId || meta.entitySysId || meta.EntitySysId);
    const richText = item.commentsRichText || item.CommentsRichText || meta.commentsRichText || meta.CommentsRichText || '';
    const decodedRichText = report3DecodeHtmlEntities(richText);
    const decodedRichTextTwice = /&lt;\/?[a-z][^&]*&gt;/i.test(decodedRichText)
        ? report3DecodeHtmlEntities(decodedRichText)
        : decodedRichText;
    const plainText = item.comments || meta.comments || report3StripHtml(richText);
    const createdByMeta = item.createdByMeta || item.CreatedByMeta || meta.createdByMeta || meta.CreatedByMeta || {};
    const creatorProfile = report3GetCommentCreatorProfile(item, meta, createdByMeta);
    const createdDate = item.createdDate || item.CreatedDate || item.createdAt || item.CreatedAt || item.created_at || meta.createdDate || meta.CreatedDate || meta.createdAt || meta.CreatedAt || null;

    return {
        text: plainText || '',
        richTextHtml: report3SanitizeCommentHtml(decodedRichTextTwice),
        createdDate: createdDate,
        createdBy: creatorProfile.fullName,
        createdByUserId: creatorProfile.userId,
        entityType: entityType,
        entitySysId: entitySysId
    };
}

function report3GetCommentInitials(name) {
    const cleaned = String(name || '').trim();
    if (!cleaned) {
        return 'U';
    }

    const parts = cleaned.split(/\s+/).filter(Boolean);
    if (!parts.length) {
        return 'U';
    }

    if (parts.length === 1) {
        return parts[0].slice(0, 2).toUpperCase();
    }

    return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
}

function report3GetCommentCreatorProfile(item, meta, createdByMeta) {
    const creatorUserId = String(
        item.createdByUserId ||
        item.CreatedByUserId ||
        meta.createdByUserId ||
        meta.CreatedByUserId ||
        item.createdBy ||
        item.CreatedBy ||
        ''
    ).trim();

    const firstName =
        createdByMeta.firstName ||
        createdByMeta.FirstName ||
        meta.createdFirstName ||
        meta.CreatedFirstName ||
        item.createdFirstName ||
        item.CreatedFirstName ||
        '';

    const lastName =
        createdByMeta.lastName ||
        createdByMeta.LastName ||
        meta.createdLastName ||
        meta.CreatedLastName ||
        item.createdLastName ||
        item.CreatedLastName ||
        '';

    const fullName = String(
        meta.createdByFullName ||
        meta.CreatedByFullName ||
        `${firstName} ${lastName}`.trim() ||
        ''
    ).trim();

    return {
        userId: creatorUserId,
        fullName: fullName || 'Unknown'
    };
}

function report3BuildCommentAvatarMarkup(displayName, userId) {
    const safeDisplayName = report3EscapeHtml(displayName || 'Unknown');
    const initials = report3GetCommentInitials(displayName || 'Unknown');
    const safeInitials = report3EscapeHtml(initials || 'U');
    const initialsSizeClass = (initials || '').length >= 3 ? ' avatar__initials--small' : '';
    const photoUrl = userId ? `/Settings/Profile/Photo/${encodeURIComponent(userId)}` : '';

    return `
        <div class="avatar__container report3-comment-avatar-container" role="img" aria-label="${safeDisplayName}" aria-hidden="false">
            ${photoUrl ? `<img class="avatar__image avatar__image--custom report3-comment-avatar-image" alt="avatar" src="${report3EscapeHtml(photoUrl)}">` : ''}
            <span class="avatar__initials avatar__initials--sm${initialsSizeClass} report3-comment-avatar-initials"${photoUrl ? ' style="display: none;"' : ''}>${safeInitials}</span>
        </div>`;
}

function report3GetCommentContextLabel(comment, task) {
    const typeMap = {
        PROJECT: 'Project',
        MILESTONE: 'Milestone',
        ROOTACTIVITY: 'Milestone',
        TASK: 'Task'
    };
    const contextType = report3NormalizeContextType(comment && comment.entityType);
    const contextPrefix = typeMap[contextType] || 'Task';
    const fallbackTaskTitle = task && task.title ? task.title : 'Current Task';
    return `${contextPrefix}: ${fallbackTaskTitle}`;
}

function report3EnhanceCommentAvatars() {
    $('#report3Timeline .report3-comment-avatar-image').each(function () {
        const $image = $(this);
        if ($image.data('report3AvatarReady')) {
            return;
        }

        const $container = $image.closest('.report3-comment-avatar-container');
        const $initials = $container.find('.report3-comment-avatar-initials');

        const showImage = function () {
            $container.removeClass('avatar__container--image-error');
            $image.removeClass('avatar__image--error').show();
            $initials.hide();
        };

        const showInitials = function () {
            $container.addClass('avatar__container--image-error');
            $image.addClass('avatar__image--error').hide();
            $initials.show();
        };

        $image.on('load', showImage);
        $image.on('error', showInitials);

        if ($image[0] && $image[0].complete) {
            if ($image[0].naturalWidth > 0) {
                showImage();
            } else {
                showInitials();
            }
        }

        $image.data('report3AvatarReady', true);
    });
}

function report3FetchAttachmentsAsync(projectNo) {
    return $.ajax({
        url: `${report3ApiPath}/api/project/${encodeURIComponent(projectNo)}/attachments`,
        type: 'GET',
        dataType: 'json'
    }).then(function (response) {
        return report3NormalizeCollection(response && response.data ? response.data : response);
    }, function () {
        return [];
    });
}

function report3FetchCommentsAsync(projectNo) {
    return $.ajax({
        url: `${report3ApiPath}/api/comments/${encodeURIComponent(projectNo)}`,
        type: 'GET',
        dataType: 'json'
    }).then(function (response) {
        return report3NormalizeCollection(response && response.data ? response.data : response);
    }, function () {
        return [];
    });
}

function report3EnsureProjectSupportData(projectNo) {
    if (Object.prototype.hasOwnProperty.call(report3State.supportCache, projectNo)) {
        return $.Deferred().resolve(report3State.supportCache[projectNo]).promise();
    }

    return $.when(
        report3FetchAttachmentsAsync(projectNo),
        report3FetchCommentsAsync(projectNo)
    ).then(function (attachments, comments) {
        const supportData = {
            attachments: report3NormalizeCollection(attachments).map(function (item) {
                return report3MapAttachment(projectNo, item);
            }),
            comments: report3NormalizeCollection(comments).map(function (item) {
                return report3MapComment(item);
            })
        };

        report3State.supportCache[projectNo] = supportData;
        return supportData;
    });
}

function report3FilterTaskSupport(items, task) {
    const taskNodeId = report3NormalizeContextId(task.nodeId);
    const taskProjectNodeSysId = report3NormalizeContextId(task.projectNodeSysId);

    return (items || []).filter(function (item) {
        const itemType = report3NormalizeContextType(item.entityType);
        const itemId = report3NormalizeContextId(item.entitySysId);

        if (itemType !== 'TASK') {
            return false;
        }

        if (!itemId) {
            return false;
        }

        return itemId === taskNodeId || (taskProjectNodeSysId && itemId === taskProjectNodeSysId);
    });
}

function report3EnrichTimelineSupport(projectNo, milestones) {
    return report3EnsureProjectSupportData(projectNo).then(function (supportData) {
        milestones.forEach(function (milestone) {
            milestone.tasks.forEach(function (task) {
                task.attachments = report3FilterTaskSupport(supportData.attachments, task);
                task.comments = report3FilterTaskSupport(supportData.comments, task);
            });
        });

        return milestones;
    });
}

function report3HasValue(value) {
    return value !== null && value !== undefined && String(value).trim() !== '';
}

function report3RecordQualityScore(record) {
    let score = 0;

    if (!report3HasValue(record.nodeType)) {
        score += 10;
    }

    if (report3HasValue(record.projectName)) {
        score += 5;
    }

    if (report3HasValue(record.status)) {
        score += 4;
    }

    if (report3HasValue(record.targetCompletion)) {
        score += 3;
    }

    if (report3HasValue(record.projectOwnerFirstName) || report3HasValue(record.projectOwnerLastName)) {
        score += 2;
    }

    if (report3HasValue(record.plantCode)) {
        score += 1;
    }

    if (report3HasValue(record.categoryCode)) {
        score += 1;
    }

    if (report3HasValue(record.productCodes)) {
        score += 1;
    }

    return score;
}

function report3MergeProjectRecords(existingRecord, candidateRecord) {
    const preferredRecord = report3RecordQualityScore(candidateRecord) > report3RecordQualityScore(existingRecord)
        ? candidateRecord
        : existingRecord;
    const fallbackRecord = preferredRecord === candidateRecord ? existingRecord : candidateRecord;
    const mergedRecord = Object.assign({}, preferredRecord);
    const numericMaxKeys = [
        'projectTaskPendingCount',
        'projectTaskAtRiskCount',
        'projectTaskClosedCount',
        'projectTaskClosedDelayedCount',
        'projectCompleteCount',
        'projectCancelCount',
        'projectCount'
    ];

    Object.keys(fallbackRecord).forEach(function (key) {
        if (!report3HasValue(mergedRecord[key]) && report3HasValue(fallbackRecord[key])) {
            mergedRecord[key] = fallbackRecord[key];
        }
    });

    numericMaxKeys.forEach(function (key) {
        mergedRecord[key] = Math.max(report3ToNumber(existingRecord[key]), report3ToNumber(candidateRecord[key]));
    });

    return mergedRecord;
}

function report3BuildUniqueProjects(records) {
    const uniqueProjects = new Map();

    (records || []).forEach(function (record) {
        const projectNo = String(record.projectNo || '').trim();
        if (!projectNo) {
            return;
        }

        if (!uniqueProjects.has(projectNo)) {
            uniqueProjects.set(projectNo, Object.assign({}, record, { projectNo: projectNo }));
            return;
        }

        uniqueProjects.set(
            projectNo,
            report3MergeProjectRecords(uniqueProjects.get(projectNo), Object.assign({}, record, { projectNo: projectNo }))
        );
    });

    return Array.from(uniqueProjects.values()).sort(function (left, right) {
        const leftName = String(left.projectName || left.projectNo || '').toLowerCase();
        const rightName = String(right.projectName || right.projectNo || '').toLowerCase();

        if (leftName < rightName) {
            return -1;
        }

        if (leftName > rightName) {
            return 1;
        }

        return String(left.projectNo || '').localeCompare(String(right.projectNo || ''));
    });
}

function report3BuildProjectItem(project) {
    const isSelected = report3State.selectedProjectNos.indexOf(project.projectNo) !== -1;
    const ownerName = report3GetOwnerName(project);
    const statusBadge = report3GetStatusBadge(project.status, {
        targetDate: project.targetCompletion
    });
    const chips = [
        project.plantCode ? `<span class="report3-project-chip"><i class="bi bi-building"></i>${report3EscapeHtml(project.plantCode)}</span>` : '',
        project.categoryCode ? `<span class="report3-project-chip"><i class="bi bi-bookmark"></i>${report3EscapeHtml(project.categoryCode)}</span>` : '',
        report3ToNumber(project.projectTaskPendingCount) > 0 ? `<span class="report3-project-chip"><i class="bi bi-hourglass-split"></i>${report3ToNumber(project.projectTaskPendingCount)} pending</span>` : '',
        report3ToNumber(project.projectTaskAtRiskCount) > 0 ? `<span class="report3-project-chip"><i class="bi bi-exclamation-triangle"></i>${report3ToNumber(project.projectTaskAtRiskCount)} at risk</span>` : ''
    ].filter(Boolean).join('');

    return `
        <div class="report3-project-item${isSelected ? ' active' : ''}" data-project-no="${report3EscapeHtml(project.projectNo)}" role="button" tabindex="0" aria-pressed="${isSelected ? 'true' : 'false'}">
            <div class="report3-project-item__selection"><i class="bi bi-check-lg"></i></div>
            <div class="report3-project-item__icon" style="background:${report3EscapeHtml(report3GetProjectColor(project))};">
                <i class="${report3EscapeHtml(report3GetProjectIcon(project))}"></i>
            </div>
            <div>
                <h3 class="report3-project-item__name">${report3EscapeHtml(project.projectName || project.projectNo || '-')}</h3>
                <div class="report3-project-item__meta">${report3EscapeHtml(project.projectNo || '-')} · ${report3EscapeHtml(ownerName)}</div>
                <div class="report3-project-item__chips">${chips || '<span class="report3-project-chip"><i class="bi bi-columns-gap"></i>Add to compare</span>'}</div>
                <div class="report3-project-item__footer">
                    <div class="report3-project-item__meta">Due ${report3EscapeHtml(report3FormatDate(project.targetCompletion, 'YYYY-MM-DD'))}</div>
                    <div>${statusBadge}</div>
                </div>
            </div>
        </div>`;
}

function report3GetSelectedProjects() {
    return report3State.selectedProjectNos.map(function (projectNo) {
        return report3State.projects.find(function (project) {
            return project.projectNo === projectNo;
        });
    }).filter(Boolean);
}

function report3RenderProjectList() {
    const countLabel = `${report3State.filteredProjects.length} shown`;
    $('#report3ProjectCount span').text(countLabel);

    if (!report3State.projects.length) {
        $('#report3ProjectStatus').text('No projects were returned for this report.');
        $('#report3ProjectList').html('<div class="report3-sidebar-empty"><div>No project records are available right now.</div></div>');
        return;
    }

    $('#report3ProjectStatus').text(`Showing ${report3State.filteredProjects.length} of ${report3State.projects.length} projects. ${report3State.selectedProjectNos.length} selected for comparison.`);

    if (!report3State.filteredProjects.length) {
        $('#report3ProjectList').html('<div class="report3-sidebar-empty"><div>No projects matched the current search.</div></div>');
        return;
    }

    $('#report3ProjectList').html(report3State.filteredProjects.map(report3BuildProjectItem).join(''));
}

function report3RenderMainEmpty(message) {
    $('#report3MainIcon')
        .show()
        .css('background', '#0f766e')
        .html('<i class="bi bi-columns-gap"></i>');
    $('#report3MainTitle').text('Select projects from the left panel');
    $('#report3MainSubtitle').text(message || 'Selected projects will appear side by side for milestone and task comparison.');
    $('#report3MainChips').html([
        '<span class="report3-main__chip"><i class="bi bi-columns-gap"></i>Multi-project comparison</span>',
        '<span class="report3-main__chip"><i class="bi bi-diagram-3"></i>Timeline alignment</span>'
    ].join(''));
    $('#report3MainStatus').html('<span class="badge bg-light text-dark border">No projects selected</span>');
    $('#report3Insight').text('Use the left pane to toggle multiple projects and compare their timelines without leaving the report.');
    $('#report3Metrics').html([
        report3MetricCard('Projects', '-', 'Select one or more projects to start comparing.'),
        report3MetricCard('Milestones', '-', 'Timeline sections will be totalled across the selection.'),
        report3MetricCard('Tasks', '-', 'Task cards will show due dates, assignees, and forms.'),
        report3MetricCard('At Risk', '-', 'At-risk tasks across all selected projects.')
    ].join(''));
    $('#report3Timeline').html(`
        <div class="report3-main-empty">
            <div>
                <div class="mb-2"><i class="bi bi-chat-left-dots" style="font-size:2rem;color:#0891b2;"></i></div>
                <div class="fw-bold text-dark mb-1">No projects selected yet.</div>
                <div>${report3EscapeHtml(message || 'Choose one or more projects from the left and they will render side by side for comparison.')}</div>
            </div>
        </div>`);
}

function report3RenderMainLoading(selectedProjects) {
    const count = selectedProjects.length;
    $('#report3MainIcon')
        .show()
        .css('background', '#0f766e')
        .html('<i class="bi bi-columns-gap"></i>');
    $('#report3MainTitle').text(`Loading ${count} selected project${count === 1 ? '' : 's'}`);
    $('#report3MainSubtitle').text('Preparing timeline columns, task forms, and comparison metrics.');
    $('#report3MainChips').html(selectedProjects.map(function (project) {
        return `<span class="report3-main__chip"><i class="bi bi-kanban"></i>${report3EscapeHtml(project.projectNo)}</span>`;
    }).join(''));
    $('#report3MainStatus').html(`
        <span class="badge bg-light text-dark border">${count} selected</span>
        <button id="report3ClearSelection" type="button" class="btn btn-outline-secondary btn-sm">Clear selection</button>`);
    $('#report3Insight').text('Loading milestone, task, and form data for the selected projects...');
    $('#report3Metrics').html([
        report3MetricCard('Projects', String(count), 'Selected for side-by-side comparison.'),
        report3MetricCard('Milestones', '-', 'Loading current roadmap structures.'),
        report3MetricCard('Tasks', '-', 'Collecting task cards, ownership, and forms.'),
        report3MetricCard('At Risk', '-', 'Computing risk counts across the selection.')
    ].join(''));
    $('#report3Timeline').html('<div class="report3-main-loading"><div class="report3-spinner"><span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>Loading project comparisons...</div></div>');
}

function report3MetricCard(label, value, meta) {
    return `
        <div class="report3-metric">
            <span class="report3-metric__label">${report3EscapeHtml(label)}</span>
            <div class="report3-metric__value">${report3EscapeHtml(value)}</div>
            <div class="report3-metric__meta">${report3EscapeHtml(meta)}</div>
        </div>`;
}

function report3BuildProjectHeaderChips(project) {
    const chips = [
        project.plantCode ? `<span class="report3-main__chip"><i class="bi bi-building"></i>${report3EscapeHtml(project.plantCode)}</span>` : '',
        project.categoryCode ? `<span class="report3-main__chip"><i class="bi bi-bookmark"></i>${report3EscapeHtml(project.categoryCode)}</span>` : '',
        project.productCodes ? `<span class="report3-main__chip"><i class="bi bi-box-seam"></i>${report3EscapeHtml(project.productCodes)}</span>` : '',
        project.targetCompletion ? `<span class="report3-main__chip"><i class="bi bi-calendar-event"></i>Due ${report3EscapeHtml(report3FormatDate(project.targetCompletion, 'YYYY-MM-DD'))}</span>` : ''
    ].filter(Boolean);

    return chips.join('');
}

function report3SummarizeTasks(tasks) {
    return tasks.reduce(function (summary, task) {
        const normalized = report3NormalizeStatus(report3GetStatusText(task.status, {
            targetDate: task.targetCompletion
        }));

        if (normalized === 'COMPLETED' || normalized === 'CANCELLED') {
            summary.closed += 1;
        } else if (normalized === 'AT_RISK') {
            summary.atRisk += 1;
        } else if (normalized === 'NOT_STARTED') {
            summary.notStarted += 1;
        } else {
            summary.inProgress += 1;
        }

        return summary;
    }, {
        inProgress: 0,
        atRisk: 0,
        notStarted: 0,
        closed: 0
    });
}

function report3BuildTimeline(nodes) {
    const milestoneMap = new Map();

    nodes.forEach(function (node) {
        const nodeType = String(node.nodeType || '').toLowerCase();
        if (nodeType !== 'milestone' && nodeType !== 'rootactivity') {
            return;
        }

        milestoneMap.set(node.nodeId, {
            nodeId: node.nodeId,
            nodeType: nodeType,
            title: node.nodeName || 'Untitled milestone',
            orderIndex: report3ToNumber(node.orderIndex),
            targetStart: report3GetNodeStartDate(node),
            targetCompletion: report3GetNodeTargetDate(node),
            status: node.projectNodeStatus,
            owners: report3ParseOwners(node.jsonNodeOwners || node.jsonMembers),
            totalCount: report3ToNumber(node.projectNodeCount),
            completeCount: report3ToNumber(node.projectNodeCompleteCount),
            cancelCount: report3ToNumber(node.projectNodeCancelCount),
            tasks: []
        });
    });

    nodes.forEach(function (node) {
        const nodeType = String(node.nodeType || '').toLowerCase();
        if (nodeType !== 'activity') {
            return;
        }

        const milestone = milestoneMap.get(node.parentSysId);
        if (!milestone) {
            return;
        }

        milestone.tasks.push({
            nodeId: node.nodeId,
            projectNodeSysId: node.projectNodeSysId || '',
            title: node.nodeName || 'Untitled task',
            orderIndex: report3ToNumber(node.orderIndex),
            targetStart: report3GetNodeStartDate(node),
            targetCompletion: report3GetNodeTargetDate(node),
            actualCompletion: node.projectNodeActualCompletionDate,
            status: node.projectNodeStatus,
            owners: report3ParseOwners(node.jsonNodeOwners || node.jsonMembers),
            prerequisites: report3ParsePrerequisites(node.prerequisitesJson),
            totalCount: report3ToNumber(node.projectNodeCount),
            completeCount: report3ToNumber(node.projectNodeCompleteCount),
            cancelCount: report3ToNumber(node.projectNodeCancelCount)
        });
    });

    const milestones = Array.from(milestoneMap.values()).sort(function (left, right) {
        return left.orderIndex - right.orderIndex;
    });

    milestones.forEach(function (milestone) {
        milestone.tasks.sort(function (left, right) {
            return left.orderIndex - right.orderIndex;
        });
    });

    return milestones;
}

function report3BuildTaskHtml(task) {
    const owners = task.owners.map(function (owner) {
        return owner.name;
    }).filter(Boolean);
    const prerequisites = task.prerequisites.map(function (prerequisite) {
        const prerequisiteName = String(prerequisite.name || '').trim();
        const path = String(prerequisite.path || '').split('/').map(function (segment) {
            return segment.trim();
        }).filter(Boolean);
        return {
            label: prerequisiteName || (path.length ? path[path.length - 1] : 'Prerequisite'),
            isComplete: String(prerequisite.status || '').toUpperCase() === 'COMPLETED'
        };
    });
    const displayStatus = report3GetStatusText(task.status, {
        targetDate: task.targetCompletion
    });
    const metaItems = [
        `<span><i class="bi bi-calendar-range"></i> ${report3EscapeHtml(report3FormatDate(task.targetStart, 'YYYY-MM-DD'))} to ${report3EscapeHtml(report3FormatDate(task.targetCompletion, 'YYYY-MM-DD'))}</span>`
    ];

    if (task.actualCompletion) {
        metaItems.push(`<span><i class="bi bi-check2-circle"></i> Closed ${report3EscapeHtml(report3FormatDate(task.actualCompletion, 'YYYY-MM-DD HH:mm'))}</span>`);
    }

    const formDetails = task.formDetails || { markup: '', withField: false, pendingRequiredFields: false };
    const formsHtml = formDetails.withField && formDetails.markup
        ? `
            <div class="report3-task__forms">
                ${formDetails.markup}
            </div>`
        : '';
    const attachments = Array.isArray(task.attachments) ? task.attachments : [];
    const comments = Array.isArray(task.comments) ? task.comments : [];
    const attachmentsHtml = attachments.length
        ? `
            <div class="report3-task__prereq">
                <strong>Attachments (${attachments.length}):</strong>
                <span class="report3-task__prereq-chips">
                    ${attachments.slice(0, 5).map(function (attachment) {
                        if (attachment.downloadUrl) {
                            return `<a class="report3-prereq-chip" href="${report3EscapeHtml(attachment.downloadUrl)}" target="_blank" rel="noopener noreferrer">${report3EscapeHtml(attachment.fileName || 'Attachment')}</a>`;
                        }

                        return `<span class="report3-prereq-chip">${report3EscapeHtml(attachment.fileName || 'Attachment')}</span>`;
                    }).join('')}
                </span>
            </div>`
        : '';
    const commentsHtml = comments.length
        ? `
            <div class="report3-task__comments">
                <div class="support-card report3-support-card">
                    <div class="section-title">Comments (<span>${comments.length}</span>)</div>
                    <div class="comments-count">${comments.length} ${comments.length === 1 ? 'comment' : 'comments'} in this thread.</div>
                    <div class="support-context-row report3-comment-context-row">
                        <label class="support-context-label">Applies to</label>
                        <div class="report3-comment-context-value">Task: ${report3EscapeHtml(task.title || 'Current Task')}</div>
                    </div>
                    <div class="comment-list report3-comment-list">
                    ${comments.slice(0, 3).map(function (comment) {
                        const bodyHtml = comment.richTextHtml
                            ? comment.richTextHtml
                            : `<p>${report3EscapeHtml(comment.text || 'Comment')}</p>`;
                        const authorDisplayName = comment.createdBy || 'Unknown';
                        const authorName = report3EscapeHtml(authorDisplayName);
                        const avatarMarkup = report3BuildCommentAvatarMarkup(authorDisplayName, comment.createdByUserId || '');
                        const dateText = report3EscapeHtml(report3FormatDate(comment.createdDate, 'YYYY-MM-DD HH:mm'));
                        const contextText = report3EscapeHtml(report3GetCommentContextLabel(comment, task));

                        return `
                            <article class="comment-item report3-comment-item">
                                ${avatarMarkup}
                                <div class="comment-body report3-comment-body">
                                    <div class="comment-header report3-comment-header">
                                        <div class="comment-author report3-comment-author">${authorName}</div>
                                        <div class="comment-time report3-comment-time">${dateText}</div>
                                    </div>
                                    <div class="comment-context report3-comment-context">Context: ${contextText}</div>
                                    <div class="comment-text comment-text-rich report3-comment-text">${bodyHtml}</div>
                                </div>
                            </article>`;
                    }).join('')}
                    </div>
                </div>
            </div>`
        : '';

    return `
        <div class="report3-task">
            <div class="report3-task__header">
                <div>
                    <h4 class="report3-task__title">${report3EscapeHtml(task.title)}</h4>
                    <div class="report3-task__meta">${metaItems.join('')}</div>
                </div>
                <div>${report3GetStatusBadge(task.status, { targetDate: task.targetCompletion })}</div>
            </div>
            <div class="report3-task__owners"><strong>Owners:</strong> ${report3EscapeHtml(owners.length ? owners.join(', ') : 'Unassigned')}</div>
            ${prerequisites.length ? `
                <div class="report3-task__prereq">
                    <strong>Prerequisites:</strong>
                    <span class="report3-task__prereq-chips">
                        ${prerequisites.map(function (prerequisite) {
                            return `<span class="report3-prereq-chip${prerequisite.isComplete ? ' is-complete' : ''}">${report3EscapeHtml(prerequisite.label)}</span>`;
                        }).join('')}
                    </span>
                </div>` : ''}
            ${formsHtml}
            ${attachmentsHtml}
            ${commentsHtml}
            <div class="report3-task__meta">
                <span><i class="bi bi-flag"></i> ${report3EscapeHtml(displayStatus)}</span>
            </div>
        </div>`;
}

function report3BuildMilestoneHtml(milestone, index) {
    const owners = milestone.owners.map(function (owner) {
        return owner.name;
    }).filter(Boolean);
    const totalTasks = milestone.tasks.length;
    const closedTasks = milestone.tasks.filter(function (task) {
        const normalized = report3NormalizeStatus(report3GetStatusText(task.status, {
            targetDate: task.targetCompletion
        }));
        return normalized === 'COMPLETED' || normalized === 'CANCELLED';
    }).length;
    const progress = totalTasks ? Math.round((closedTasks / totalTasks) * 100) : 0;
    const statusBadge = report3GetStatusBadge(milestone.status, {
        targetDate: milestone.targetCompletion
    });
    const label = milestone.nodeType === 'rootactivity' ? 'Launch lane' : `Milestone ${index + 1}`;

    return `
        <article class="report3-milestone">
            <div class="report3-milestone__header">
                <div>
                    <div class="report3-milestone__label"><i class="bi bi-signpost-split"></i>${report3EscapeHtml(label)}</div>
                    <h3 class="report3-milestone__title">${report3EscapeHtml(milestone.title)}</h3>
                    <div class="report3-milestone__meta">
                        <span><i class="bi bi-calendar-event"></i> Due ${report3EscapeHtml(report3FormatDate(milestone.targetCompletion, 'YYYY-MM-DD'))}</span>
                        <span><i class="bi bi-people"></i> ${report3EscapeHtml(owners.length ? owners.join(', ') : 'No owners assigned')}</span>
                        <span><i class="bi bi-list-check"></i> ${totalTasks} task${totalTasks === 1 ? '' : 's'}</span>
                    </div>
                </div>
                <div class="report3-milestone__status">
                    <div class="report3-milestone__progress"><i class="bi bi-bar-chart"></i>${progress}% closed</div>
                    <div>${statusBadge}</div>
                </div>
            </div>
            <div class="report3-task-list">
                ${milestone.tasks.length ? milestone.tasks.map(report3BuildTaskHtml).join('') : '<div class="report3-empty-card">No tasks are attached to this milestone yet.</div>'}
            </div>
        </article>`;
}

function report3BuildComparisonEntry(project, milestones) {
    const allTasks = milestones.reduce(function (sum, milestone) {
        return sum.concat(milestone.tasks);
    }, []);
    return {
        project: project,
        milestones: milestones,
        allTasks: allTasks,
        statusSummary: report3SummarizeTasks(allTasks)
    };
}

function report3BuildCompareColumn(entry) {
    const project = entry.project;

    return `
        <section class="report3-compare-column">
            <div class="report3-compare-column__header">
                <div class="report3-compare-column__headline">
                    <div class="report3-compare-column__title-wrap">
                        <div class="report3-compare-column__icon" style="background:${report3EscapeHtml(report3GetProjectColor(project))};">
                            <i class="${report3EscapeHtml(report3GetProjectIcon(project))}"></i>
                        </div>
                        <div>
                            <h3 class="report3-compare-column__title">${report3EscapeHtml(project.projectName || project.projectNo || '-')}</h3>
                            <div class="report3-compare-column__subtitle">${report3EscapeHtml(project.projectNo || '-')} · ${report3EscapeHtml(report3GetOwnerName(project))}</div>
                        </div>
                    </div>
                    <div class="d-flex align-items-center gap-2">
                        ${report3GetStatusBadge(project.status, { targetDate: project.targetCompletion })}
                        <button type="button" class="btn btn-outline-secondary btn-sm" data-remove-project-no="${report3EscapeHtml(project.projectNo)}">Remove</button>
                    </div>
                </div>
                <div class="report3-compare-column__chips">${report3BuildProjectHeaderChips(project)}</div>
                <div class="report3-compare-column__meta">
                    <div class="report3-compare-column__metric">
                        <span class="report3-compare-column__metric-label">Milestones</span>
                        <div class="report3-compare-column__metric-value">${entry.milestones.length}</div>
                    </div>
                    <div class="report3-compare-column__metric">
                        <span class="report3-compare-column__metric-label">Tasks</span>
                        <div class="report3-compare-column__metric-value">${entry.allTasks.length}</div>
                    </div>
                    <div class="report3-compare-column__metric">
                        <span class="report3-compare-column__metric-label">In Progress</span>
                        <div class="report3-compare-column__metric-value">${entry.statusSummary.inProgress}</div>
                    </div>
                    <div class="report3-compare-column__metric">
                        <span class="report3-compare-column__metric-label">At Risk</span>
                        <div class="report3-compare-column__metric-value">${entry.statusSummary.atRisk}</div>
                    </div>
                </div>
            </div>
            <div class="report3-compare-column__body">
                ${entry.milestones.length ? entry.milestones.map(report3BuildMilestoneHtml).join('') : '<div class="report3-empty-card">This project does not have milestone nodes to display yet.</div>'}
            </div>
        </section>`;
}

function report3RenderComparison(entries) {
    const aggregate = entries.reduce(function (summary, entry) {
        summary.projects += 1;
        summary.milestones += entry.milestones.length;
        summary.tasks += entry.allTasks.length;
        summary.atRisk += entry.statusSummary.atRisk;
        return summary;
    }, {
        projects: 0,
        milestones: 0,
        tasks: 0,
        atRisk: 0
    });

    $('#report3MainIcon')
        .show()
        .css('background', '#0f766e')
        .html('<i class="bi bi-columns-gap"></i>');
    $('#report3MainTitle').text(`${aggregate.projects} project${aggregate.projects === 1 ? '' : 's'} in comparison`);
    $('#report3MainSubtitle').text(aggregate.projects === 1
        ? 'Single project view is active. Select more projects from the left panel to compare them side by side.'
        : 'Scroll horizontally if needed to compare milestones, tasks, forms, and risk signals side by side.');
    $('#report3MainChips').html(entries.map(function (entry) {
        return `<span class="report3-main__chip"><i class="bi bi-kanban"></i>${report3EscapeHtml(entry.project.projectNo)}</span>`;
    }).join(''));
    $('#report3MainStatus').html(`
        <span class="badge bg-light text-dark border">${aggregate.projects} selected</span>
        <button id="report3ClearSelection" type="button" class="btn btn-outline-secondary btn-sm">Clear selection</button>`);
    $('#report3Insight').text(`${aggregate.milestones} milestones and ${aggregate.tasks} tasks are loaded across the selected projects for side-by-side review.`);
    $('#report3Metrics').html([
        report3MetricCard('Projects', String(aggregate.projects), 'Currently included in the comparison board.'),
        report3MetricCard('Milestones', String(aggregate.milestones), 'Total roadmap sections across selected projects.'),
        report3MetricCard('Tasks', String(aggregate.tasks), 'Task cards including form values and prerequisites.'),
        report3MetricCard('At Risk', String(aggregate.atRisk), 'At-risk tasks across all selected projects.')
    ].join(''));
    $('#report3Timeline').html(`<div class="report3-compare-board">${entries.map(report3BuildCompareColumn).join('')}</div>`);
    report3EnhanceCommentAvatars();
}

function report3ApplyProjectSearch() {
    const searchValue = ($('#report3ProjectSearch').val() || '').trim().toLowerCase();

    report3State.filteredProjects = report3State.projects.filter(function (project) {
        const searchText = [
            project.projectNo,
            project.projectName,
            project.projectOwnerFirstName,
            project.projectOwnerLastName,
            project.plantCode,
            project.categoryCode,
            project.productCodes
        ].join(' ').toLowerCase();

        return !searchValue || searchText.indexOf(searchValue) !== -1;
    });

    report3RenderProjectList();
}

function report3FetchProjects() {
    return $.ajax({
        url: report3ApiPath + '/api/projects/datatables',
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({
            draw: 1,
            start: 0,
            length: -1,
            status: 'NOT STARTED,ONGOING,HOLD,COMPLETED,CANCELLED,ARCHIVED',
            order: [],
            columns: [],
            search: { value: '' }
        })
    });
}

function report3FetchNodes(projectNo) {
    return $.ajax({
        url: `${report3ApiPath}/api/projects/${encodeURIComponent(projectNo)}/nodes`,
        type: 'GET',
        dataType: 'json'
    });
}

function report3EnsureProjectComparison(project) {
    const projectNo = project.projectNo;

    if (Object.prototype.hasOwnProperty.call(report3State.comparisonCache, projectNo)) {
        return $.Deferred().resolve(report3State.comparisonCache[projectNo]).promise();
    }

    function finalize(nodes) {
        return report3EnrichTimelineForms(projectNo, report3BuildTimeline(nodes))
            .then(function (milestones) {
                return report3EnrichTimelineSupport(projectNo, milestones);
            })
            .then(function (milestones) {
                const entry = report3BuildComparisonEntry(project, milestones);
                report3State.comparisonCache[projectNo] = entry;
                return entry;
            });
    }

    if (report3State.nodesCache[projectNo]) {
        return finalize(report3State.nodesCache[projectNo]);
    }

    return report3FetchNodes(projectNo).then(function (response) {
        const nodes = Array.isArray(response) ? response : (response.rows || []);
        report3State.nodesCache[projectNo] = nodes;
        return finalize(nodes);
    });
}

function report3LoadSelectedComparisons() {
    const selectedProjects = report3GetSelectedProjects();
    const requestId = ++report3State.compareRequestId;

    // Always rebuild form sections from latest saved values.
    report3State.comparisonCache = {};
    report3State.taskFormsCache = {};
    report3State.submissionValueCache = {};

    if (!selectedProjects.length) {
        report3RenderProjectList();
        report3RenderMainEmpty();
        return;
    }

    report3RenderProjectList();
    report3RenderMainLoading(selectedProjects);

    const entries = [];
    const loads = selectedProjects.map(function (project, index) {
        return report3EnsureProjectComparison(project).then(function (entry) {
            entries[index] = entry;
        });
    });

    $.when.apply($, loads)
        .then(function () {
            if (requestId !== report3State.compareRequestId) {
                return;
            }

            report3RenderComparison(entries.filter(Boolean));
        })
        .fail(function () {
            if (requestId !== report3State.compareRequestId) {
                return;
            }

            $('#report3Insight').text('Unable to load one or more selected project timelines right now.');
            $('#report3Timeline').html('<div class="report3-main-empty"><div>Unable to load the selected project comparisons.</div></div>');
        });
}

function report3ToggleProjectSelection(projectNo) {
    const existingIndex = report3State.selectedProjectNos.indexOf(projectNo);

    if (existingIndex === -1) {
        report3State.selectedProjectNos.push(projectNo);
    } else {
        report3State.selectedProjectNos.splice(existingIndex, 1);
    }

    report3LoadSelectedComparisons();
}

$(document).ready(function () {
    report3RenderMainEmpty();

    $('#report3ProjectSearch').on('input', report3ApplyProjectSearch);

    $('#report3ProjectList').on('click', '.report3-project-item', function () {
        const projectNo = $(this).data('project-no');
        if (!projectNo) {
            return;
        }

        report3ToggleProjectSelection(String(projectNo));
    });

    $('#report3ProjectList').on('keydown', '.report3-project-item', function (event) {
        if (event.key !== 'Enter' && event.key !== ' ') {
            return;
        }

        event.preventDefault();
        const projectNo = $(this).data('project-no');
        if (!projectNo) {
            return;
        }

        report3ToggleProjectSelection(String(projectNo));
    });

    $('#report3MainStatus').on('click', '#report3ClearSelection', function () {
        report3State.selectedProjectNos = [];
        report3LoadSelectedComparisons();
    });

    $('#report3Timeline').on('click', '[data-remove-project-no]', function () {
        const projectNo = $(this).data('remove-project-no');
        if (!projectNo) {
            return;
        }

        const index = report3State.selectedProjectNos.indexOf(String(projectNo));
        if (index !== -1) {
            report3State.selectedProjectNos.splice(index, 1);
            report3LoadSelectedComparisons();
        }
    });

    report3FetchProjects()
        .done(function (response) {
            report3State.projects = report3BuildUniqueProjects(Array.isArray(response.data) ? response.data : []);
            report3State.filteredProjects = report3State.projects.slice();
            report3RenderProjectList();

            if (report3State.filteredProjects.length) {
                report3State.selectedProjectNos = [report3State.filteredProjects[0].projectNo];
                report3LoadSelectedComparisons();
            }
        })
        .fail(function () {
            report3State.projects = [];
            report3State.filteredProjects = [];
            $('#report3ProjectCount span').text('0 shown');
            $('#report3ProjectStatus').text('Unable to load projects right now.');
            $('#report3ProjectList').html('<div class="report3-sidebar-empty"><div>Unable to load project records for Report 3.</div></div>');
            report3RenderMainEmpty('Project data could not be loaded.');
        });
});