let cachedProjectNo = null;
let cachedNodes = null;
let cachedMilestones = null;

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/\"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

async function parseApiError(response) {
    try {
        const body = await response.json();
        return body.message || body.Message || `Request failed with status ${response.status}.`;
    } catch (err) {
        return `Request failed with status ${response.status}.`;
    }
}

function buildApiRequestOptions(options) {
    const settings = options || {};
    const headers = Object.assign({}, settings.headers || {});

    if (window.pulseJwtToken) {
        headers.Authorization = `Bearer ${window.pulseJwtToken}`;
    }

    return Object.assign({}, settings, {
        headers,
        credentials: settings.credentials || "same-origin"
    });
}

function resetMemberTaskCache(projectNo) {
    if (!projectNo || String(cachedProjectNo) === String(projectNo)) {
        cachedProjectNo = null;
        cachedNodes = null;
        cachedMilestones = null;
    }
}

function normalizeProjectMembers(projectMembersSource) {
    if (Array.isArray(projectMembersSource)) {
        return projectMembersSource;
    }

    if (typeof projectMembersSource === "string" && projectMembersSource.trim()) {
        try {
            return JSON.parse(projectMembersSource);
        } catch (err) {
            return [];
        }
    }

    return [];
}

function getCurrentTaskMemberContext(projectMembersSource) {
    const memberId = window.user?.EmployeeId || window.user?.userId || window.user?.userid || "";
    if (!memberId) {
        return null;
    }

    const members = normalizeProjectMembers(projectMembersSource);
    const matchedMember = members.find(member => {
        const candidateId = member?.userid || member?.userId || member?.EmployeeId || "";
        return String(candidateId).toLowerCase() === String(memberId).toLowerCase();
    });

    const memberName = matchedMember
        ? `${matchedMember.firstname || matchedMember.firstName || ""} ${matchedMember.lastname || matchedMember.lastName || ""}`.trim()
        : (window.user?.DisplayName || `${window.user?.FirstName || ""} ${window.user?.LastName || ""}`.trim() || "My tasks");

    return {
        memberId,
        memberName: memberName || "My tasks"
    };
}

function openCurrentUserProjectTasksModal(projectNo, projectMembersSource) {
    const context = getCurrentTaskMemberContext(projectMembersSource);
    if (!context) {
        bootbox.alert("Unable to determine the current user for task maintenance.");
        return;
    }

    return openMemberTasksModal(projectNo, context.memberId, context.memberName);
}

async function fetchPulseNodes(projectNo) {
    if (cachedProjectNo && String(cachedProjectNo) !== String(projectNo)) {
        resetMemberTaskCache(cachedProjectNo);
    }

    if (cachedNodes) {
        return cachedNodes;
    }

    const url = getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/nodes`;
    const response = await fetch(url, buildApiRequestOptions({
        method: "GET",
        headers: { "Accept": "application/json" }
    }));

    if (!response.ok) {
        throw new Error(`Failed to fetch tasks: ${response.status}`);
    }

    const body = await response.json();
    const nodes = Array.isArray(body) ? body : (body.rows || []);

    cachedProjectNo = projectNo;
    cachedNodes = nodes;
    return nodes;
}

function buildMilestonesFromNodes(nodes) {
    if (cachedMilestones) {
        return cachedMilestones;
    }

    const milestoneMap = new Map();
    let rootActivityMilestone = null;

    nodes.forEach(node => {
        if (node.nodeType === "milestone" || node.nodeType === "rootactivity") {
            const milestone = {
                id: node.nodeId,
                name: node.nodeName,
                orderIndex: node.orderIndex,
                tasks: []
            };

            milestoneMap.set(node.nodeId, milestone);

            if (node.nodeType === "rootactivity") {
                rootActivityMilestone = milestone;
            }
        }
    });

    function parseJsonMembers(jsonStr) {
        if (!jsonStr) {
            return [];
        }

        try {
            const parsed = JSON.parse(jsonStr);
            return parsed
                .map(member => String(member.userid || member.userId || member.EmployeeId || ""))
                .filter(Boolean);
        } catch (err) {
            console.warn("Failed to parse task owner payload.", err);
            return [];
        }
    }

    nodes.forEach(node => {
        if (node.nodeType !== "activity") {
            return;
        }

        const milestone = milestoneMap.get(node.parentSysId) || rootActivityMilestone;
        if (!milestone) {
            return;
        }

        milestone.tasks.push({
            id: node.nodeId,
            projectNodeSysId: node.projectNodeSysId,
            title: node.nodeName,
            description: node.nodeDescription || "",
            status: node.projectNodeStatus,
            orderIndex: node.orderIndex,
            estimatedMandays: node.estimatedMandays || 0,
            isRequired: String(node.isRequired) === "1" || node.isRequired === 1 || node.isRequired === true,
            transactionKey: node.transactionKey,
            parentMilestoneId: node.parentSysId,
            isCustom: String(node.projectNodeSysId || "") === String(node.nodeId || ""),
            memberIds: parseJsonMembers(node.jsonNodeOwners)
        });
    });

    const milestones = Array.from(milestoneMap.values()).sort((left, right) => (left.orderIndex ?? 0) - (right.orderIndex ?? 0));
    milestones.forEach(milestone => {
        milestone.tasks.sort((left, right) => (left.orderIndex ?? 0) - (right.orderIndex ?? 0));
    });

    cachedMilestones = milestones;
    return milestones;
}

function getCustomTaskMilestones(milestones, currentParentMilestoneId) {
    return milestones.filter(Boolean);
}

function statusLabel(status) {
    return getPulseStatusText(status);
}

function statusBadgeClass(status) {
    return getPulseStatusClassName(status);
}

function buildTaskMeta(task) {
    const parts = [];
    parts.push(task.isRequired ? "Required" : "Optional");
    parts.push(`${task.estimatedMandays || 0} mandays`);
    return parts.join(" • ");
}

function renderTaskItem(task, memberId) {
    const isAssigned = task.memberIds.includes(String(memberId));

    return `
        <div class="member-task-item border rounded-3 p-3 mb-2"
             data-task-id="${escapeHtml(task.projectNodeSysId)}"
             data-initial-assigned="${isAssigned ? "1" : "0"}"
             data-current-assigned="${isAssigned ? "1" : "0"}">
            <div class="d-flex flex-wrap align-items-start justify-content-between gap-2">
                <div class="flex-grow-1">
                    <div class="d-flex flex-wrap align-items-center gap-2 mb-1">
                        <span class="fw-semibold">${escapeHtml(task.title)}</span>
                        <span class="${statusBadgeClass(task.status)}">${statusLabel(task.status)}</span>
                        ${task.isCustom ? '<span class="badge bg-light text-dark border">Custom</span>' : ''}
                    </div>
                    ${task.description ? `<div class="small text-muted mb-1">${escapeHtml(task.description)}</div>` : ""}
                    <div class="small text-muted">${escapeHtml(buildTaskMeta(task))}</div>
                </div>
                <div class="d-flex flex-wrap align-items-center gap-2">
                    <label class="form-check form-switch d-inline-flex align-items-center gap-2 mb-0">
                        <input class="form-check-input task-assignment-toggle" type="checkbox" ${isAssigned ? "checked" : ""}>
                        <span class="task-assignment-label">${isAssigned ? "Assigned" : "Not assigned"}</span>
                    </label>
                    ${task.isCustom ? `
                        <button type="button" class="btn btn-sm btn-outline-secondary btn-edit-custom-task" data-task-id="${escapeHtml(task.projectNodeSysId)}">Edit</button>
                        <button type="button" class="btn btn-sm btn-outline-danger btn-delete-custom-task" data-task-id="${escapeHtml(task.projectNodeSysId)}">Delete</button>
                    ` : ""}
                </div>
            </div>
        </div>
    `;
}

function renderTaskGroupsForMember(memberId, milestones) {
    if (!milestones.length) {
        return '<div class="small text-muted">No milestones are available for this project yet.</div>';
    }

    return milestones.map(milestone => {
        const tasksMarkup = milestone.tasks.length
            ? milestone.tasks.map(task => renderTaskItem(task, memberId)).join("")
            : '<div class="small text-muted border rounded-3 p-3">No tasks in this milestone yet.</div>';

        return `
            <section class="member-task-group mb-3" data-milestone-id="${escapeHtml(milestone.id)}">
                <div class="d-flex align-items-center justify-content-between mb-2">
                    <h6 class="mb-0">${escapeHtml(milestone.name)}</h6>
                    <span class="small text-muted">${milestone.tasks.length} task${milestone.tasks.length === 1 ? "" : "s"}</span>
                </div>
                ${tasksMarkup}
            </section>
        `;
    }).join("");
}

function buildMemberTasksContent(memberName, memberId, milestones) {
    const customTaskMilestones = getCustomTaskMilestones(milestones, null);

    return `
        <div>
            <p class="mb-3 small text-muted">
                Assign or unassign tasks for <strong>${escapeHtml(memberName)}</strong>. Saving will add or remove project owner records for task ownership.
            </p>
            <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                <div class="small text-muted">Use the switch to toggle assignment.</div>
                <button id="btnAddCustomTask" type="button" class="btn btn-sm btn-outline-primary" ${customTaskMilestones.length ? "" : "disabled"}>
                    <i class="bi bi-plus-lg me-1"></i>Add custom task
                </button>
            </div>
            <div id="taskList" style="max-height: 460px; overflow-y: auto; padding-right: 0.25rem;">
                ${renderTaskGroupsForMember(memberId, milestones)}
            </div>
        </div>
    `;
}

function buildMilestoneOptions(milestones, selectedMilestoneId) {
    return milestones.map(milestone => `
        <option value="${escapeHtml(milestone.id)}" ${String(milestone.id) === String(selectedMilestoneId) ? "selected" : ""}>
            ${escapeHtml(milestone.name)}
        </option>
    `).join("");
}

function buildCustomTaskEditor(memberName, milestones, task, defaultMilestoneId) {
    const customTaskMilestones = getCustomTaskMilestones(milestones, task?.parentMilestoneId);
    const selectedMilestoneId = task?.parentMilestoneId || defaultMilestoneId || customTaskMilestones[0]?.id;

    return `
        <div>
            <p class="small text-muted mb-3">
                ${task ? "Update the task details below." : `Create a project task for <strong>${escapeHtml(memberName)}</strong>.`}
            </p>
            <div class="form-floating mb-2">
                <select id="customTaskMilestone" class="form-select">
                    ${buildMilestoneOptions(customTaskMilestones, selectedMilestoneId)}
                </select>
                <label for="customTaskMilestone">Milestone</label>
            </div>
            <div class="form-floating mb-2">
                <input id="customTaskTitle" class="form-control" maxlength="250" placeholder="Task title" value="${escapeHtml(task?.title || "")}" />
                <label for="customTaskTitle">Task title</label>
            </div>
            <div class="form-floating mb-2">
                <textarea id="customTaskDescription" class="form-control" placeholder="Task description" style="height: 120px;">${escapeHtml(task?.description || "")}</textarea>
                <label for="customTaskDescription">Task description</label>
            </div>
            <div class="row g-2 align-items-center">
                <div class="col-sm-6">
                    <div class="form-floating">
                        <input id="customTaskMandays" class="form-control" type="number" min="0" step="1" placeholder="0" value="${escapeHtml(task?.estimatedMandays ?? 0)}" />
                        <label for="customTaskMandays">Estimated mandays</label>
                    </div>
                </div>
                <div class="col-sm-6">
                    <div class="form-check form-switch mt-2 pt-2">
                        <input id="customTaskRequired" class="form-check-input" type="checkbox" ${task?.isRequired ? "checked" : ""} />
                        <label class="form-check-label" for="customTaskRequired">Required task</label>
                    </div>
                </div>
            </div>
        </div>
    `;
}

function openCustomTaskDialog(projectNo, memberId, memberName, milestones, task, defaultMilestoneId) {
    const customTaskMilestones = getCustomTaskMilestones(milestones, task?.parentMilestoneId);

    if (!customTaskMilestones.length) {
        bootbox.alert("Add a milestone before creating project tasks.");
        return;
    }

    const dialog = bootbox.dialog({
        title: task ? `Edit task for ${memberName}` : `Add task for ${memberName}`,
        message: buildCustomTaskEditor(memberName, milestones, task, defaultMilestoneId),
        buttons: {
            cancel: {
                label: "Cancel",
                className: "btn btn-sm btn-outline-secondary"
            },
            save: {
                label: task ? "Save task" : "Create task",
                className: "btn btn-sm btn-primary",
                callback: function () {
                    const $dialog = $(dialog);
                    const title = ($dialog.find("#customTaskTitle").val() || "").trim();
                    const parentNodeId = $dialog.find("#customTaskMilestone").val();
                    const description = ($dialog.find("#customTaskDescription").val() || "").trim();
                    const estimatedMandays = Number($dialog.find("#customTaskMandays").val() || 0);
                    const isRequired = $dialog.find("#customTaskRequired").is(":checked");

                    if (!title) {
                        bootbox.alert("Task title is required.");
                        return false;
                    }

                    if (!parentNodeId) {
                        bootbox.alert("Select a milestone.");
                        return false;
                    }

                    const url = task
                        ? getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/custom-tasks/${encodeURIComponent(task.projectNodeSysId)}`
                        : getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/custom-tasks`;

                    const payload = {
                        projectNo,
                        parentNodeId,
                        title,
                        description,
                        estimatedMandays: Number.isFinite(estimatedMandays) ? Math.max(0, Math.trunc(estimatedMandays)) : 0,
                        isRequired,
                        transactionKey: task?.transactionKey || null,
                        ownerIds: [String(memberId)]
                    };

                    return fetch(url, buildApiRequestOptions({
                        method: task ? "PUT" : "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(payload)
                    })).then(async response => {
                        if (!response.ok) {
                            throw new Error(await parseApiError(response));
                        }

                        bootbox.hideAll();
                        refreshMemberTaskModal(projectNo, memberId, memberName);
                        return true;
                    }).catch(err => {
                        bootbox.alert(err.message || "Failed to save task.");
                        return false;
                    });
                }
            }
        }
    });
}

function deleteCustomTask(projectNo, memberId, memberName, task) {
    bootbox.confirm({
        title: "Delete task",
        message: `Delete <strong>${escapeHtml(task.title)}</strong>? This removes the custom task from the project.`,
        buttons: {
            confirm: {
                label: "Delete",
                className: "btn btn-sm btn-danger"
            },
            cancel: {
                label: "Cancel",
                className: "btn btn-sm btn-outline-secondary"
            }
        },
        callback: function (confirmed) {
            if (!confirmed) {
                return;
            }

            const url = getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/custom-tasks/${encodeURIComponent(task.projectNodeSysId)}`;
            fetch(url, buildApiRequestOptions({ method: "DELETE" }))
                .then(async response => {
                    if (!response.ok) {
                        throw new Error(await parseApiError(response));
                    }

                    bootbox.hideAll();
                    refreshMemberTaskModal(projectNo, memberId, memberName);
                })
                .catch(err => {
                    bootbox.alert(err.message || "Failed to delete task.");
                });
        }
    });
}

function setTaskAssignmentState($item, isAssigned) {
    $item.attr("data-current-assigned", isAssigned ? "1" : "0");

    $item.find(".task-assignment-toggle").prop("checked", isAssigned);
    $item.find(".task-assignment-label").text(isAssigned ? "Assigned" : "Not assigned");
}

function setupMemberTasksEvents(modal, projectNo, memberId, memberName, milestones) {
    const $modal = $(modal);
    const $taskList = $modal.find("#taskList");

    $modal.on("click", "#btnAddCustomTask", function () {
        const customTaskMilestones = getCustomTaskMilestones(milestones, null);
        openCustomTaskDialog(projectNo, memberId, memberName, milestones, null, customTaskMilestones[0]?.id || null);
    });

    $taskList.on("change", ".task-assignment-toggle", function () {
        const $item = $(this).closest(".member-task-item");
        setTaskAssignmentState($item, $(this).is(":checked"));
    });

    $taskList.on("click", ".btn-edit-custom-task", function (event) {
        event.preventDefault();
        event.stopPropagation();

        const task = findTaskByProjectNodeId(milestones, $(this).data("task-id"));
        if (!task) {
            bootbox.alert("Task details are no longer available. Refresh and try again.");
            return;
        }

        openCustomTaskDialog(projectNo, memberId, memberName, milestones, task, task.parentMilestoneId);
    });

    $taskList.on("click", ".btn-delete-custom-task", function (event) {
        event.preventDefault();
        event.stopPropagation();

        const task = findTaskByProjectNodeId(milestones, $(this).data("task-id"));
        if (!task) {
            bootbox.alert("Task details are no longer available. Refresh and try again.");
            return;
        }

        deleteCustomTask(projectNo, memberId, memberName, task);
    });
}

function computeSelectionChanges($modal) {
    const changes = {
        newlySelected: [],
        newlyUnselected: []
    };

    $modal.find(".member-task-item").each(function () {
        const $item = $(this);
        const taskId = $item.attr("data-task-id");
        const initialAssigned = $item.attr("data-initial-assigned") === "1";
        const currentAssigned = $item.attr("data-current-assigned") === "1";

        if (initialAssigned === currentAssigned) {
            return;
        }

        if (currentAssigned) {
            changes.newlySelected.push(taskId);
        } else {
            changes.newlyUnselected.push(taskId);
        }
    });

    return changes;
}

function refreshMemberTaskModal(projectNo, memberId, memberName) {
    resetMemberTaskCache(projectNo);
    bootbox.hideAll();
    return openMemberTasksModal(projectNo, memberId, memberName);
}

async function openMemberTasksModal(projectNo, memberId, memberName) {
    try {
        const nodes = await fetchPulseNodes(projectNo);
        const milestones = buildMilestonesFromNodes(nodes);

        const dialog = bootbox.dialog({
            title: `Tasks for ${memberName}`,
            message: buildMemberTasksContent(memberName, memberId, milestones),
            size: "large",
            onShown: function (event) {
                setupMemberTasksEvents(event.target, projectNo, memberId, memberName, milestones);
            },
            buttons: {
                cancel: {
                    label: "Close",
                    className: "btn btn-sm btn-outline-secondary"
                },
                save: {
                    label: "Save changes",
                    className: "btn btn-sm btn-primary",
                    callback: function () {
                        const $modal = $(dialog);
                        const changes = computeSelectionChanges($modal);

                        if (!changes.newlySelected.length && !changes.newlyUnselected.length) {
                            return true;
                        }

                        const payload = {
                            projectNo: projectNo,
                            memberId: String(memberId),
                            milestoneId: null,
                            newlySelectedTaskIds: changes.newlySelected,
                            newlyUnselectedTaskIds: changes.newlyUnselected
                        };

                        const url = getApiRootPath() + `/api/projects/${encodeURIComponent(projectNo)}/member-tasks/update`;
                        return fetch(url, buildApiRequestOptions({
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(payload)
                        })).then(async response => {
                            if (!response.ok) {
                                throw new Error(await parseApiError(response));
                            }

                            resetMemberTaskCache(projectNo);
                            return true;
                        }).catch(err => {
                            bootbox.alert(err.message || "Failed to save task assignments.");
                            return false;
                        });
                    }
                }
            }
        });
    } catch (err) {
        console.error(err);
        bootbox.alert("Failed to load tasks from server.");
    }
}
