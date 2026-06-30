// =========================
// Utility Helpers
// =========================
function escapeHtml(str) {
    return (str || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

function safeJsonParse(str, fallback = null) {
    if (!str || typeof str !== 'string') return fallback;
    try {
        return JSON.parse(str);
    } catch (e) {
        console.warn('JSON parse failed', { str, error: e });
        return fallback;
    }
}

function roundTo(value, decimals) {
    if (value == null || isNaN(value)) return 0;
    const factor = Math.pow(10, decimals);
    return Math.round(value * factor) / factor;
}

function calcPercentCompleted({ total, completed, cancelled }) {
    const t = total || 0;
    if (!t) return 0;
    const closed = (completed || 0) + (cancelled || 0);
    const pct = (closed / t) * 100;
    return roundTo(Math.max(0, Math.min(100, pct)), 2);
}

function formatDate(date, pattern = "MMM DD, YYYY") {
    return date ? moment(date).format(pattern) : '';
}

function formatDateISO(date) {
    return date ? moment(date).format("YYYY-MM-DD") : '';
}

function createBadge({ baseClass, modifierClass, iconClass, text }) {
    return `
        <span class="${baseClass} ${modifierClass}">
            <i class="${iconClass}"></i> ${text}
        </span>`;
}

function htmlToElement(html) {
    const template = document.createElement('template');
    template.innerHTML = html.trim();
    return template.content.firstElementChild;
}


// =========================
// Status & Badge Helpers
// =========================

function mapStatusToLabel(status, enddate, limitdays) {
    return getPulseStatusText(status, {
        targetDate: enddate,
        riskAfterDays: limitdays
    });
}

function projectnodeStatusBadge(uiStatus) {
    return getPulseStatusBadge(uiStatus);
}

function taskStatusBadge(uiStatus) {
    return getPulseStatusBadge(uiStatus);
}

function projectStatusBadge(uiStatus) {
    return getPulseStatusBadge(uiStatus);
}

// =========================
// Loading helpers
// =========================

function showLoadingMilestone() {
    if (DOM.loadingMilestones) DOM.loadingMilestones.style.display = 'block';
}

function hideLoadingMilestone() {
    if (DOM.loadingMilestones) DOM.loadingMilestones.style.display = 'none';
}

function showLoadingTasks() {
    if (!DOM.loadingTasks) return;
    DOM.loadingTasks.classList.remove('d-none');
    DOM.loadingTasks.classList.add('d-block');
}

function hideLoadingTasks() {
    if (!DOM.loadingTasks) return;
    DOM.loadingTasks.classList.remove('d-block', 'd-flex');
    DOM.loadingTasks.classList.add('d-none');
}


// =========================
// API helpers (async)
// ========================= 

function fetchProjectNodeChildrenAsync(projectno, nodetype, nodeid) {
    const d = { projectNo: projectno, nodetype, nodeid };

    return $.ajax({
        url: getApiRootPath() + `/api/projects/children`,
        type: 'POST',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json',
        data: JSON.stringify(d)
    }).then(resp => resp.data);
}

function fetchProjectNodeItemAsync(projectno, nodetype, nodeid) {
    const d = { projectNo: projectno, nodetype, nodeid };

    return $.ajax({
        url: getApiRootPath() + `/api/projects/${projectno}/node`,
        type: 'POST',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json',
        data: JSON.stringify(d)
    });
}

function fetchProjectAsync(id) {
    const d = {
        draw: 1,
        start: 0,
        length: 1000,
        order: [],
        columns: [],
        search: { value: id }
    };

    return $.ajax({
        url: API_URL,
        type: 'POST',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json',
        data: JSON.stringify(d)
    }).then(resp => resp.data);
}



function fetchSubmissionValueAsync(submissionValueSysId) {
    return $.ajax({
        url: getApiRootPath() + `/api/ProjectForms/submissions/value/${submissionValueSysId}`,
        type: 'GET',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    });
}




// =========================
// Tree view builder
// =========================

function buildHierarchyTree() {
    if (!DOM.hierarchyTree) return;

    const treeRoot = DOM.hierarchyTree;
    treeRoot.innerHTML = '';

    const selectedMilestone = document.querySelector('.milestone-item.active');
    if (!selectedMilestone) return;

    const milestoneName = selectedMilestone.dataset.milestoneName || 'Milestone';

    const liMilestone = document.createElement('li');
    liMilestone.className = 'tree-node';
    liMilestone.innerHTML =
        '<i class="fas fa-flag text-primary"></i>' +
        '<span class="label">' + milestoneName + '</span>';
    liMilestone.dataset.scrollTarget = '';
    treeRoot.appendChild(liMilestone);

    const ulActivities = document.createElement('ul');
    ulActivities.className = 'tree-ul';
    treeRoot.appendChild(ulActivities);

    document.querySelectorAll('#activitiesContainer .activity-card').forEach(card => {
        const actId = card.dataset.nodeId;
        const actName = card.dataset.activityName || 'Activity';

        const liAct = document.createElement('li');
        liAct.className = 'tree-node';
        liAct.innerHTML =
            '<i class="fas fa-tasks text-indigo-500"></i>' +
            '<span class="label">' + actName + '</span>';
        liAct.dataset.scrollTarget = '#activity-' + actId;

        ulActivities.appendChild(liAct);
    });

    treeRoot.querySelectorAll('.tree-node').forEach(node => {
        node.addEventListener('click', function () {
            const targetSelector = this.dataset.scrollTarget;
            if (!targetSelector) return;
            const el = document.querySelector(targetSelector);
            if (el) {
                el.classList.add('border-primary');
                el.scrollIntoView({ behavior: 'smooth', block: 'start' });
                setTimeout(() => el.classList.remove('border-primary'), 1200);
            }
        });
    });
}

// =========================
// Filters
// =========================

function applyFilters() {
    if (!DOM.activitiesContainer || !DOM.emptyState) return;

    const statusFilter = DOM.filterStatus ? DOM.filterStatus.value : '';
    const ownerFilter = DOM.filterOwner ? DOM.filterOwner.value.toLowerCase() : '';
    const searchFilter = DOM.filterSearch ? DOM.filterSearch.value.toLowerCase() : '';

    const cards = DOM.activitiesContainer.querySelectorAll('.activity-card');
    let visibleCount = 0;

    cards.forEach(card => {
        const status = card.dataset.activityStatus || '';
        const owner = (card.dataset.activityOwner || '').toLowerCase();
        const name = (card.dataset.activityName || '').toLowerCase();

        let visible = true;

        if (statusFilter && status !== statusFilter) visible = false;
        if (ownerFilter && !owner.includes(ownerFilter)) visible = false;
        if (searchFilter && !name.includes(searchFilter)) visible = false;

        card.style.display = visible ? '' : 'none';
        if (visible) visibleCount++;
    });

    DOM.emptyState.classList.toggle('d-none', visibleCount > 0);
}