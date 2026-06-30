const API_URL = getApiRootPath() + "/api/ProjectTasks/Assigned"; // <-- change to your API

const PAGE_SIZE = 8;


const tasksList = document.getElementById("tasksList");
const pagerInfo = document.getElementById("pagerInfo");
const prevBtn = document.getElementById("prevPage");
const nextBtn = document.getElementById("nextPage");

let allTasks = [];      // all from API
let filteredTasks = []; // after search+filters
let currentPage = 1;
resetHeroDecorations({ clearChips: false });


const searchInput = document.getElementById("taskSearch");
const statusFilter = document.getElementById("statusFilter");
const requiredFilter = document.getElementById("requiredFilter");
const nodeFilter = document.getElementById("nodeFilter");
const nodeFilterToggle = document.getElementById("nodeFilterToggle");
const nodeFilterLabel = document.getElementById("nodeFilterLabel");
const nodeFilterOptions = document.getElementById("nodeFilterOptions");
const nodeFilterSelectAll = document.getElementById("nodeFilterSelectAll");
const nodeFilterClear = document.getElementById("nodeFilterClear");

let selectedNodeNames = new Set();

function getCurrentUserTaskDisplayName() {
    return window.user ?.DisplayName || `${window.user ?.FirstName || ""} ${window.user ?.LastName || ""}`.trim() || "My tasks";
}

function escapeHtml(value) {
    return $('<div>').text(value ?? '').html();
}

function normalizeNodeName(value) {
    return (value || "").trim();
}

function getTaskNodeName(task) {
    return normalizeNodeName(task.nodeName || task.activityName || task.alttaskname || "");
}

function getUniqueNodeNames(tasks) {
    return [...new Set(tasks
        .map(getTaskNodeName)
        .filter(Boolean))]
        .sort((left, right) => left.localeCompare(right));
}

function updateNodeFilterLabel() {
    const count = selectedNodeNames.size;

    if (!count) {
        nodeFilterLabel.textContent = "Node Name: All";
        return;
    }

    if (count === 1) {
        nodeFilterLabel.textContent = `Node Name: ${Array.from(selectedNodeNames)[0]}`;
        return;
    }

    nodeFilterLabel.textContent = `Node Name: ${count} selected`;
}

function renderNodeFilterOptions(tasks) {
    const nodeNames = getUniqueNodeNames(tasks);

    if (!nodeNames.length) {
        nodeFilterOptions.innerHTML = '<div class="node-filter__empty">No node names available.</div>';
        updateNodeFilterLabel();
        return;
    }

    const validNodeNames = new Set(nodeNames);
    selectedNodeNames = new Set(Array.from(selectedNodeNames).filter(name => validNodeNames.has(name)));

    nodeFilterOptions.innerHTML = nodeNames.map(nodeName => `
        <label class="node-filter__option">
            <input type="checkbox" class="form-check-input node-filter__checkbox" value="${escapeHtml(nodeName)}" ${selectedNodeNames.has(nodeName) ? "checked" : ""}>
            <span>${escapeHtml(nodeName)}</span>
        </label>
    `).join("");

    nodeFilterOptions.querySelectorAll(".node-filter__checkbox").forEach((checkbox) => {
        checkbox.addEventListener("change", (event) => {
            const { value, checked } = event.target;

            if (checked) {
                selectedNodeNames.add(value);
            } else {
                selectedNodeNames.delete(value);
            }

            updateNodeFilterLabel();
            applyFilters();
        });
    });

    updateNodeFilterLabel();
}

function statusBadgeClass(status, task) {
    return getPulseStatusClassName(status, {
        targetDate: task ? getTaskTargetDate(task) : null
    });
}

function getTaskTargetDate(task) {
    return task.targetCompletionDate || task.taskWkFiscalDate || task.projectWkFiscalDate || null;
}

function formatStatus(status, task) {
    return getPulseStatusText(status, {
        targetDate: getTaskTargetDate(task)
    });
}

function requiredBadgeClass(isRequired) {
    return isRequired ? "badge-required" : "badge-optional";
}

function requiredLabel(isRequired) {
    return isRequired ? "Required" : "Optional";
}

function formatDate(value) {
    if (!value) return "-";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return value; // already formatted
    return d.toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "2-digit",
    });
}

function projectIconText(projectName) {
    if (!projectName) return "?";
    const parts = projectName.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[1].charAt(0)).toUpperCase();
}

function projectIconColor(color) {
    // if API provides a color, use it; otherwise generate a pastel
    if (color) return color;
    const colors = ["#6366F1", "#0EA5E9", "#10B981", "#EC4899", "#F97316", "#14B8A6"];
    return colors[Math.floor(Math.random() * colors.length)];
}


function renderTasksPage() {
    if (!filteredTasks.length) {
        tasksList.innerHTML = '<div class="empty-message">No tasks found.</div>';
        pagerInfo.textContent = "Page 0 of 0";
        prevBtn.disabled = true;
        nextBtn.disabled = true;
        return;
    }

    const totalPages = Math.ceil(filteredTasks.length / PAGE_SIZE);
    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * PAGE_SIZE;
    const end = start + PAGE_SIZE;
    const pageTasks = filteredTasks.slice(start, end);

    const fragment = document.createDocumentFragment();

    pageTasks.forEach((t) => {
        const row = document.createElement("div");
        row.className = "task-row";
        row.dataset.id = t.projectTaskSysId;

        // Column 1: Activity
        const colActivity = document.createElement("div");
        colActivity.className = "task-main";

        const icon = document.createElement("div");
        icon.className = "project-icon " + t.projectIcon;
        if (!t.projectIcon)
            icon.textContent = projectIconText(t.projectName);
        icon.style.backgroundColor = projectIconColor(t.projectIconColor);

        const text = document.createElement("div");
        text.className = "task-text";

        const title = document.createElement("div");
        title.className = "task-title";
        title.textContent = t.activityName || t.alttaskname || "(No activity name)";

        const meta = document.createElement("div");
        meta.className = "task-meta";
        meta.textContent = t.activityDescription || t.alttaskdescription || "";

        text.appendChild(title);
        if (meta.textContent) text.appendChild(meta);

        colActivity.appendChild(icon);
        colActivity.appendChild(text);

        // Column 2: Project
        const colProject = document.createElement("div");
        colProject.className = "task-project";
        colProject.innerHTML =
            '<div class="label">Project</div>' +
            `<span>${t.projectNo || ""} ${t.projectName || ""}</span>` +
            (t.projectDescription ? `<span style="font-size:11px;color:#6b7280;">${t.projectDescription}</span>` : "");

        // Column 3: Plant / Category
        const colPlant = document.createElement("div");
        colPlant.className = "task-plant";
        const plantLine = [t.plantName, t.productDivisionName].filter(Boolean).join(" • ");
        const catLine = [t.categoryName, t.productGroupName].filter(Boolean).join(" • ");
        colPlant.innerHTML =
            '<div class="label">Plant / Category</div>' +
            `<span>${plantLine || "-"}</span>` +
            (catLine ? `<span>${catLine}</span>` : "");

        // Column 4: Dates
        const colDates = document.createElement("div");
        colDates.className = "task-dates";
        const target = formatDate(t.targetCompletionDate || t.taskWkFiscalDate || t.projectWkFiscalDate);
        const actual = formatDate(t.actualCompletionDate);
        colDates.innerHTML =
            '<div class="label">Dates</div>' +
            `<span>Target: ${target}</span>` +
            `<span>Actual: ${actual}</span>`;

        // Column 5: Status & effort
        const colRight = document.createElement("div");
        colRight.className = "task-right";

        const statusBadge = document.createElement("span");
        statusBadge.className = statusBadgeClass(t.status, t);
        statusBadge.innerHTML = getPulseStatusBadge(t.status, {
            targetDate: getTaskTargetDate(t)
        }).replace(/^<span[^>]*>|<\/span>$/g, '');

        const requiredBadge = document.createElement("span");
        requiredBadge.className = `badge ${requiredBadgeClass(t.isRequired)}`;
        requiredBadge.textContent = requiredLabel(t.isRequired);

        const mdaysBadge = document.createElement("span");
        mdaysBadge.className = "badge badge-mdays";
        mdaysBadge.textContent = `${t.estimatedMandays ?? "-"} mdays`;

        colRight.appendChild(statusBadge);
        colRight.appendChild(requiredBadge);
        //colRight.appendChild(mdaysBadge);

        // Column 6: Buttons
        const colButtons = document.createElement("div");
        const loggedUserId = String(window.user ?.EmployeeId || "").toLowerCase();
        const taskMembers = String(t.members || "").toLowerCase();
        const canManageTask = loggedUserId && taskMembers.includes(loggedUserId);
        const taskPageUrl = canManageTask
            ? `/projects/projecttasks/edit/${t.projectTaskSysId}`
            : `/projects/projecttasks/readonly/${t.projectTaskSysId}`;

        const editButton = canManageTask ? `<a type="button" href="${taskPageUrl}"
        class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0"
        style="width: 40px; height: 40px;">
  <i class="bi bi-pen-fill"></i>
</a>`: `<a type="button" disabled
        class="btn btn-outline-secondary rounded-circle d-inline-flex align-items-center justify-content-center p-0 disabled"
        style="width: 40px; height: 40px;">
  <i class="bi bi-pen-fill"></i>
</a>`

        colButtons.innerHTML = `
    <a
        class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0 btn-open-project-member-tasks"
        style="width: 40px; height: 40px;"
        title="View" href="/projects/projecttasks/readonly/${t.projectTaskSysId}"
        data-project-no="${escapeHtml(t.projectNo || "")}">
      <i class="bi bi-eye"></i>
    </a>
    <button type="button"
        class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0 btn-open-project-member-tasks"
        style="width: 40px; height: 40px;"
        title="Manage my tasks in this project"
        data-project-no="${escapeHtml(t.projectNo || "")}">
      <i class="bi bi-person-workspace"></i>
    </button> ${editButton}` ;


        



        colButtons.className = "task-buttons";


        row.appendChild(colActivity);
        row.appendChild(colProject);
        row.appendChild(colPlant);
        row.appendChild(colDates);
        row.appendChild(colRight);
        row.appendChild(colButtons);

        fragment.appendChild(row);
    });

    tasksList.innerHTML = "";
    tasksList.appendChild(fragment);

    pagerInfo.textContent = `Page ${currentPage} of ${totalPages} • ${filteredTasks.length} task(s)`;
    prevBtn.disabled = currentPage === 1;
    nextBtn.disabled = currentPage === totalPages;
}

function applyFilters() {
    const q = searchInput.value.trim().toLowerCase();
    const statusVal = statusFilter.value.toLowerCase();
    const requiredVal = requiredFilter.value;

    filteredTasks = allTasks.filter((t) => {
        // -- search
        const haystack = [
            t.activityName,
            t.alttaskname,
            t.activityDescription,
            t.projectNo,
            t.projectName,
            t.plantName,
            t.categoryName,
            t.productGroupName
        ]
            .filter(Boolean)
            .join(" ")
            .toLowerCase();

        if (q && !haystack.includes(q)) return false;

        // status filter (simple contains match)
        if (statusVal) {
            const s = formatStatus(t.status, t).toLowerCase();
            if (!s.includes(statusVal)) return false;
        }

        // required filter
        if (requiredVal === "required" && !t.isRequired) return false;
        if (requiredVal === "optional" && t.isRequired) return false;

        const nodeName = getTaskNodeName(t);
        if (selectedNodeNames.size > 0 && !selectedNodeNames.has(nodeName)) return false;

        return true;
    });

    currentPage = 1;
    renderTasksPage();
}

searchInput.addEventListener("input", applyFilters);
statusFilter.addEventListener("change", applyFilters);
requiredFilter.addEventListener("change", applyFilters);

nodeFilterToggle.addEventListener("click", () => {
    nodeFilter.classList.toggle("is-open");
});

nodeFilterSelectAll.addEventListener("click", () => {
    selectedNodeNames.clear();
    renderNodeFilterOptions(allTasks);
    applyFilters();
});

nodeFilterClear.addEventListener("click", () => {
    selectedNodeNames.clear();
    renderNodeFilterOptions(allTasks);
    applyFilters();
});

document.addEventListener("click", (event) => {
    if (!nodeFilter.contains(event.target)) {
        nodeFilter.classList.remove("is-open");
    }
});

prevBtn.addEventListener("click", () => {
    if (currentPage > 1) {
        currentPage--;
        renderTasksPage();
    }
});

nextBtn.addEventListener("click", () => {
    const totalPages = Math.ceil(filteredTasks.length / PAGE_SIZE);
    if (currentPage < totalPages) {
        currentPage++;
        renderTasksPage();
    }
});

tasksList.addEventListener("click", (event) => {
    const button = event.target.closest(".btn-open-project-member-tasks");
    if (!button) {
        return;
    }

    const projectNo = button.getAttribute("data-project-no") || "";
    const memberId = window.user ?.EmployeeId || window.user ?.userId || window.user ?.userid || "";

    if (!projectNo || !memberId) {
        bootbox.alert("Unable to open task maintenance for this project.");
        return;
    }

    openMemberTasksModal(projectNo, memberId, getCurrentUserTaskDisplayName());
});

async function loadTasks() {
    try {
        const res = await fetch(API_URL,
            {
                method: "GET",
                headers: {
                    "Authorization": pulseJwtToken ? `Bearer ${pulseJwtToken}` : ""
                }
            }
        );
        if (!res.ok) throw new Error("Network error");
        const data = await res.json();

        // Map your real API result to the field names you gave
        allTasks = data.map(row => ({
            projectTaskSysId: row.projectTaskSysId,
            nodeName: row.activityName,
            activityName: row.activityName,
            activityDescription: row.activityDescription,
            projectNo: row.projectNo,
            projectName: row.projectName,
            projectDescription: row.projectDescription,
            projectIcon: row.projectIcon,
            projectIconColor: row.projectIconColor,
            plantName: row.plantName,
            members: row.members,
            categoryName: row.categoryName,
            productGroupName: row.productGroupName,
            productDivisionName: row.productDivisionName,
            estimatedMandays: row.estimatedMandays,
            targetStartYear: row.targetStartYear,
            targetStartWorkWeek: row.targetStartWorkWeek,
            targetStartDate: row.targetStartDate,
            targetcompletionyear: row.targetcompletionyear,
            targetcompletionworkweek: row.targetcompletionworkweek,
            targetCompletionDate: row.targetCompletionDate,
            projecttargetcompletionyear: row.projecttargetcompletionyear,
            projectcompletionworkweek: row.projectcompletionworkweek,
            projectWkFiscalDate: row.projectWkFiscalDate,
            taskWkFiscalDate: row.taskWkFiscalDate,
            actualStartDate: row.actualStartDate,
            actualCompletionDate: row.actualCompletionDate,
            status: row.status,
            isRequired: row.isRequired === 1
        }));
    } catch (e) {
        // Fallback demo data shaped like your fields
        allTasks = [

        ];
    }

    renderNodeFilterOptions(allTasks);
    applyFilters();
}

initJwtToken();
loadTasks();
