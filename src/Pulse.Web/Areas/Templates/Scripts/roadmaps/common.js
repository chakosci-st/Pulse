var __maturityList = [];
var __formList = [];

window.currentUserCode = "*";

// --- Globals / State ---
let treeData = [];
let rootForms = [];

let currentModalNode = null;
let currentModalParent = null;
let currentModalMode = 'add';
let currentModalType = 'milestone';
let currentModalMaturity = null;
let currentFormNode = null;

let activityNameFetchTimeout = null;
let lastActivityNameQuery = '';

// --- DOM / Bootstrap refs ---
const nodeModal = new bootstrap.Modal(document.getElementById('nodeModal'));
const nodeForm = document.getElementById('nodeForm');
const nodeModalTitle = document.getElementById('nodeModalTitle');
const nodeModalBody = document.getElementById('nodeModalBody');
const nodeModalSaveBtn = document.getElementById('nodeModalSaveBtn');

const formModal = new bootstrap.Modal(document.getElementById('formModal'));
const formForm = document.getElementById('formForm');

// --- Utilities ---
function ClearField() {
    $('input').val('');
    $('textarea').val('');
}

function uuidv4() {
    if (window.crypto && crypto.randomUUID) {
        return crypto.randomUUID();
    }

    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0;
        var v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function createRandomDomId(prefix) {
    return prefix + '_' + Math.random().toString(36).substr(2, 9);
}

function ensureArray(value) {
    return Array.isArray(value) ? value : [];
}

function getSortedForms(forms) {
    return ensureArray(forms).slice().sort(function (a, b) {
        if (a.isActive !== b.isActive) {
            return b.isActive - a.isActive;
        }

        return (a.name || '').localeCompare(b.name || '', undefined, { sensitivity: 'base' });
    });
}

function mapFormsToSelect2Data(forms) {
    return getSortedForms(forms).map(function (item) {
        return {
            id: item.id,
            text: (item.name || '') + (item.isActive == 0 ? ' (In Active)' : ''),
            disabled: item.isActive == 0,
            description: item.description || ''
        };
    });
}

function findFormById(formId) {
    if (!formId) return null;

    const id = String(formId);
    return ensureArray(__formList).find(function (item) {
        return String(item.id) === id;
    }) || null;
}

function initOrRefreshModalFormsSelect() {
    const $modalForms = $('#modalForms');
    const options = mapFormsToSelect2Data(__formList);

    if (!$modalForms.hasClass('select2-hidden-accessible')) {
        $modalForms.select2({
            data: options,
            dropdownParent: $('#formModal'),
            placeholder: 'Select a form',
            minimumInputLength: 0,
            width: '100%'
        });
        return;
    }

    $modalForms.empty();
    options.forEach(function (opt) {
        const option = new Option(opt.text, opt.id, false, false);
        if (opt.disabled) {
            option.disabled = true;
        }
        $modalForms.append(option);
    });

    $modalForms.trigger('change.select2');
}

function getCurrentRoadmapSysId() {
    const el = document.getElementById('RoadmapSysId');
    if (!el) return null;

    const value = (el.value || '').trim();
    return value || null;
}

function isActivityLike(node) {
    return !!node && (node.type === 'activity' || node.type === 'task');
}

function getUsedRootMaturities() {
    return ensureArray(treeData).map(node => node ?.data ?.maturity).filter(Boolean);
}

function getMilestonePathLabel(node) {
    let labels = [];
    let cur = node ?.parentRef || null;

    while (cur) {
        if (cur.type === 'milestone') {
            labels.unshift(cur ?.data ?.name || 'Milestone');
        }
        cur = cur.parentRef;
    }

    return labels.join(' / ');
}

function collectAllActivityNodes(nodes, result = []) {
    ensureArray(nodes).forEach(n => {
        if (isActivityLike(n)) result.push(n);
        if (ensureArray(n.children).length > 0) {
            collectAllActivityNodes(n.children, result);
        }
    });
    return result;
}

function restoreParentRefs(nodes, parent) {
    ensureArray(nodes).forEach(n => {
        n.parentRef = parent || null;
        n.children = ensureArray(n.children);
        n.forms = ensureArray(n.forms);
        n.prerequisites = ensureArray(n.prerequisites);

        if (n.children.length > 0) {
            restoreParentRefs(n.children, n);
        }
    });
}

// --- Data Model ---
function createNode(type, data = {}) {
    return {
        key: uuidv4(),
        id: createRandomDomId('node'),
        type,
        data: data || {},
        children: [],
        forms: [],
        prerequisites: [],
        collapsed: false,
        parentRef: null
    };
}

function createPrerequisite(data = {}) {
    return {
        key: uuidv4(),
        value: data.value ?? null
    };
}

function createForm(data = {}) {
    return {
        key: uuidv4(),
        id: createRandomDomId('form'),
        sysid: data.sysid ?? null,
        name: data.name ?? '',
        desc: data.desc ?? ''
    };
}

// --- Import Regeneration Logic ---
function regenerateAllElementGuids(importData) {
    const keyMap = new Map();

    function processNode(node) {
        if (!node) return;

        const oldKey = node.key;
        const newKey = uuidv4();

        if (oldKey) {
            keyMap.set(oldKey, newKey);
        }

        node.key = newKey;
        node.id = createRandomDomId('node');
        node.children = ensureArray(node.children);
        node.forms = ensureArray(node.forms);
        node.prerequisites = ensureArray(node.prerequisites);

        node.forms.forEach(form => {
            form.key = uuidv4();
            form.id = createRandomDomId('form');
        });

        node.children.forEach(processNode);
    }

    importData.treeData = ensureArray(importData.treeData);
    importData.rootForms = ensureArray(importData.rootForms);

    importData.treeData.forEach(processNode);

    importData.rootForms.forEach(form => {
        form.key = uuidv4();
        form.id = createRandomDomId('form');
    });

    function remapPrerequisites(node) {
        if (!node) return;

        node.prerequisites = ensureArray(node.prerequisites)
            .map(oldKey => keyMap.get(oldKey) || oldKey)
            .filter(Boolean);

        ensureArray(node.children).forEach(remapPrerequisites);
    }

    importData.treeData.forEach(remapPrerequisites);

    importData.roadmapsysid = uuidv4();
    return importData;
}

// --- JSON helpers ---
function stripNodeForExport(node) {
    if (!node) return null;

    let { parentRef, ...n } = node;

    n.forms = ensureArray(n.forms).map(f => {
        let { file, ...rest } = f || {};
        return rest;
    });

    n.children = ensureArray(n.children)
        .map(stripNodeForExport)
        .filter(Boolean);

    return n;
}

function buildRoadmapExportObject(tree, forms) {
    return {
        roadmapsysid: getCurrentRoadmapSysId() || uuidv4(),
        treeData: ensureArray(tree).map(stripNodeForExport).filter(Boolean),
        rootForms: ensureArray(forms).map(f => {
            let { file, ...rest } = f || {};
            return rest;
        })
    };
}

function buildRoadmapJson(tree, forms) {
    return JSON.stringify(buildRoadmapExportObject(tree, forms), null, 2);
}

function normalizeImportedData(data) {
    if (Array.isArray(data)) {
        return {
            roadmapsysid: null,
            treeData: data,
            rootForms: []
        };
    }

    return {
        roadmapsysid: data ?.roadmapsysid || null,
        treeData: ensureArray(data ?.treeData),
        rootForms: ensureArray(data ?.rootForms)
    };
}

function importRoadmapJson(data, options = {}) {
    try {
        let importedData = normalizeImportedData(data);
        const forceRegenerateGuids = options && options.forceRegenerateGuids === true;

        const currentRoadmapSysId = getCurrentRoadmapSysId();
        const importedRoadmapSysId = importedData.roadmapsysid
            ? String(importedData.roadmapsysid).trim()
            : null;

        const shouldRegenerateGuids = forceRegenerateGuids ||
            !currentRoadmapSysId ||
            currentRoadmapSysId !== importedRoadmapSysId;

        if (shouldRegenerateGuids) {
            importedData = regenerateAllElementGuids(importedData);
        }

        treeData = ensureArray(importedData.treeData);
        rootForms = ensureArray(importedData.rootForms);

        restoreParentRefs(treeData, null);
        renderTimeline();
    } catch (err) {
        console.error(err);
        alert("Invalid JSON file.");
    }
}

// --- Prerequisite Logic ---
function getAllPriorActivities(tree, targetNode) {
    let result = [];
    let found = false;

    function traverse(nodes) {
        for (let node of ensureArray(nodes)) {
            if (node === targetNode) {
                found = true;
                return;
            }

            if (isActivityLike(node)) {
                result.push(node);
            }

            if (ensureArray(node.children).length > 0) {
                traverse(node.children);
                if (found) return;
            }
        }
    }

    traverse(tree);

    const rootActivities = ensureArray(tree).filter(n => isActivityLike(n));
    const seen = new Set(result.map(n => n.key));

    rootActivities.forEach(ra => {
        if (!seen.has(ra.key) && ra !== targetNode) {
            result.push(ra);
            seen.add(ra.key);
        }
    });

    return result;
}

function findMilestoneNameForActivity(activityNode) {
    let parent = activityNode ?.parentRef || null;
    while (parent && parent.type !== 'milestone') {
        parent = parent.parentRef;
    }
    return parent ? parent ?.data ?.name : '';
}

// --- Activity Autocomplete ---
async function fetchActivityNameSuggestions(query) {
    if (!query || query.trim().length < 2) {
        return [];
    }

    try {
        const url = getApiRootPath() + '/api/activities?search=' + encodeURIComponent(query.trim());
        const data = await getDataAsync(url);

        return ensureArray(data)
            .sort((a, b) => (a.activityName || '').localeCompare(b.activityName || '', undefined, { sensitivity: 'base' }));
    } catch (err) {
        console.error('Error fetching activity name suggestions:', err);
        return [];
    }
}

function attachActivityNameAutocompleteIfNeeded() {
    if (currentModalType !== 'activity') return;

    const $input = $('#modalName');
    const $suggestions = $('#activityNameSuggestions');

    if (!$input.length || !$suggestions.length) return;

    $input.off('.activityName');
    $suggestions.off('.activityName');

    function hideSuggestions() {
        $suggestions.hide().empty();
    }

    function renderSuggestions(items) {
        if (!items || items.length === 0) {
            hideSuggestions();
            return;
        }

        $suggestions.empty();

        items.forEach(item => {
            const $row = $('<button type="button" class="list-group-item list-group-item-action"></button>');
            $row.text(item.activityName || '');
            $row.data('activityItem', item);
            $suggestions.append($row);
        });

        $suggestions.show();
    }

    $input.on('input.activityName', function () {
        const query = $(this).val();

        if (!query || query.length < 2) {
            hideSuggestions();
            return;
        }

        lastActivityNameQuery = query;

        if (activityNameFetchTimeout) {
            clearTimeout(activityNameFetchTimeout);
        }

        activityNameFetchTimeout = setTimeout(async () => {
            const currentQuery = lastActivityNameQuery;
            const results = await fetchActivityNameSuggestions(currentQuery);

            if (currentQuery !== $('#modalName').val()) return;
            renderSuggestions(results);
        }, 300);
    });

    $suggestions.on('click.activityName', '.list-group-item-action', function () {
        const item = $(this).data('activityItem');
        if (!item) return;

        $input.val(item.activityName || '');
        $('#modalDesc').val(item.activityDescription || '');
        hideSuggestions();
    });

    $(document).off('click.activityName').on('click.activityName', function (e) {
        const target = $(e.target);
        if (!target.closest('#modalNameWrapper').length) {
            hideSuggestions();
        }
    });

    $input.on('keydown.activityName', function (e) {
        if (e.key === 'Escape') {
            hideSuggestions();
        }
    });
}

// --- Node Modal ---
function showNodeModal({ mode, type, node = null, parent = null, parentMaturity = null }) {
    currentModalMode = mode;
    currentModalType = type;
    currentModalNode = node;
    currentModalParent = parent;
    currentModalMaturity = parentMaturity;

    let prereqField = '';
    let allPriorActivities = [];

    if (type === 'activity') {
        let workNode = node || createNode('activity', {});

        if (mode === 'add') {
            if (parent) {
                workNode.parentRef = parent;
                parent.children = ensureArray(parent.children);
                parent.children.push(workNode);
            } else {
                treeData.push(workNode);
            }
        }

        allPriorActivities = getAllPriorActivities(treeData, workNode);

        if (mode === 'add') {
            if (parent) {
                parent.children = ensureArray(parent.children).filter(n => n !== workNode);
            } else {
                treeData = ensureArray(treeData).filter(n => n !== workNode);
            }
        }

        const selected = ensureArray(node ?.prerequisites);

        prereqField = `
            <div class="mb-2">
                <label for="modalPrereq" class="form-label">Prerequisite Activities</label>
                <select class="form-select" id="modalPrereq" multiple>
                    ${allPriorActivities.map(a => {
                const path = getMilestonePathLabel(a);
                const label = (path ? path + ' / ' : '') + (a ?.data ?.name || '');
                return `<option value="${a.key}" ${selected.includes(a.key) ? 'selected' : ''}>${label}</option>`;
            }).join('')}
                </select>
                <div class="form-text">You can select any prior activity (including root activities) as a prerequisite.</div>
            </div>
        `;
    }

    nodeModalTitle.textContent =
        (mode === 'add' ? 'Add ' : 'Edit ') + (type === 'milestone' ? 'Milestone' : 'Activity');

    let usedMaturities = getUsedRootMaturities();
    let maturityField = '';

    if (type === 'milestone') {
        if (!parentMaturity) {
            const data = ensureArray(__maturityList);
            let options = [];

            data.forEach(item => {
                const maturityCode = item ?.maturityCode || '';
                options.push(
                    `<option value="${maturityCode}" ` +
                    `${node ?.data ?.maturity === maturityCode ? 'selected' : ''} ` +
                    `${usedMaturities.includes(maturityCode) && (!node || node ?.data ?.maturity !== maturityCode) ? 'disabled' : ''}>` +
                    `${maturityCode}</option>`
                );
            });

            maturityField = `
                <div class="form-floating mb-2">
                    <select class="form-select" id="modalMaturity" required>
                        <option value="" disabled ${!node ?.data ?.maturity ? 'selected' : ''}></option>
                        ${options.join(' ')}
                    </select>
                    <label for="modalMaturity">Maturity</label>
                </div>
            `;
        } else {
            maturityField = `
                <div class="form-floating mb-2">
                    <input type="text" class="form-control" id="modalMaturity" value="${parentMaturity}" readonly>
                    <label for="modalMaturity">Maturity (from parent)</label>
                </div>
            `;
        }
    }

    const isActiveHTML = `
        <div class="form-check mb-2">
            <input class="form-check-input" type="checkbox" id="modalIsActive" ${node ?.data ?.isActive ? 'checked' : ''}>
            <label class="form-check-label" for="modalIsActive">Is Active</label>
        </div>
    `;

    nodeModalBody.innerHTML = `
        ${maturityField}
        <div class="form-floating mb-2 position-relative" id="modalNameWrapper">
            <input type="text" class="form-control" id="modalName" placeholder="Name" value="${node ?.data ?.name || ''}" required autocomplete="off">
            <label for="modalName">${type === 'milestone' ? 'Milestone Name' : 'Activity Name'}</label>
            ${type === 'activity' ? `
                <div id="activityNameSuggestions"
                     class="list-group"
                     style="position:absolute; z-index:1060; top:100%; left:0; right:0; max-height:200px; overflow-y:auto; display:none;">
                </div>
            ` : ''}
        </div>
        <div class="form-floating mb-2">
            <textarea class="form-control" id="modalDesc" placeholder="Description" required>${node ?.data ?.desc || ''}</textarea>
            <label for="modalDesc">${type === 'milestone' ? 'Milestone Description' : 'Activity Description'}</label>
        </div>
        ${type === 'activity' ? `
            <div class="form-floating mb-2">
                <input type="number" min="0" value="${node ?.data ?.mandays || 0}" class="form-control" id="modalMandays" placeholder="Estimated Mandays" required>
                <label for="modalMandays">Estimated Mandays</label>
            </div>
            ${prereqField}
        ` : ''}
        <div class="form-check mb-2">
            <input class="form-check-input" type="checkbox" id="modalIsRequired" ${node ?.data ?.isRequired ? 'checked' : ''}>
            <label class="form-check-label" for="modalIsRequired">Is Required</label>
        </div>
        ${mode === 'add' ? '' : isActiveHTML}
    `;

    nodeModal.show();
    attachActivityNameAutocompleteIfNeeded();
}

nodeForm.onsubmit = function (e) {
    e.preventDefault();

    let data = {
        name: nodeForm.querySelector('#modalName') ?.value || '',
        desc: nodeForm.querySelector('#modalDesc') ?.value || ''
    };

    if (currentModalType === 'milestone') {
        data.maturity = currentModalMaturity || nodeForm.querySelector('#modalMaturity') ?.value || null;
    } else {
        data.mandays = nodeForm.querySelector('#modalMandays') ?.value || 0;
    }

    data.isRequired = !!nodeForm.querySelector('#modalIsRequired') ?.checked;

    if (currentModalMode !== 'add') {
        data.isActive = !!nodeForm.querySelector('#modalIsActive') ?.checked;
    }

    let prerequisites = [];
    if (currentModalType === 'activity') {
        const prereqSelect = nodeForm.querySelector('#modalPrereq');
        if (prereqSelect) {
            prerequisites = Array.from(prereqSelect.selectedOptions).map(opt => opt.value);
        }
    }

    if (currentModalMode === 'add') {
        let newNode = createNode(currentModalType, data);
        if (currentModalType === 'activity') {
            newNode.prerequisites = prerequisites;
        }

        if (currentModalParent) {
            currentModalParent.children = ensureArray(currentModalParent.children);
            newNode.parentRef = currentModalParent;
            currentModalParent.children.push(newNode);
        } else {
            treeData.push(newNode);
        }
    } else if (currentModalMode === 'edit' && currentModalNode) {
        currentModalNode.data = data;
        if (currentModalType === 'activity') {
            currentModalNode.prerequisites = prerequisites;
        }
    }

    nodeModal.hide();
    renderTimeline();
};

// --- Form Modal ---
formForm.onsubmit = function (e) {
    e.preventDefault();

    const select2Data = $('#modalForms').select2('data');
    if (!select2Data || !select2Data.length) return;

    const selected = select2Data[0];
    const sysid = selected.id;
    const name = selected.text;
    const desc = selected.description;

    const newForm = createForm({ sysid, name, desc });

    if (currentFormNode) {
        currentFormNode.forms = ensureArray(currentFormNode.forms);
        currentFormNode.forms.push(newForm);
    } else {
        rootForms.push(newForm);
    }

    $('#containerPreview').html('');
    $('#modalForms').val('').trigger('change');
    formModal.hide();
    formForm.reset();
    renderTimeline();
};

function showFormModal(node) {
    currentFormNode = node || null;
    formForm.reset();
    $('#modalDescription').val('');
    $('#containerPreview').html('');
    $('#modalForms').val('').trigger('change');

    initOrRefreshModalFormsSelect();
    formModal.show();
}

// --- Sortable Logic ---
function makeSortable(container, arr, rerenderFn) {
    if (!container || !Array.isArray(arr)) return;

    Sortable.create(container, {
        animation: 150,
        handle: '.milestone-card, .activity-card',
        ghostClass: 'sortable-ghost',
        draggable: '.milestone-card, .activity-card',
        onEnd: function (evt) {
            if (evt.oldIndex !== evt.newIndex) {
                const moved = arr.splice(evt.oldIndex, 1)[0];
                arr.splice(evt.newIndex, 0, moved);
                rerenderFn();
            }
        }
    });
}

function makeFormsSortable(container, formsArray, rerenderFn) {
    if (!container || !Array.isArray(formsArray)) return;

    Sortable.create(container, {
        animation: 150,
        handle: '.attachment',
        draggable: '.attachment',
        onEnd: function (evt) {
            if (evt.oldIndex === evt.newIndex) return;
            const moved = formsArray.splice(evt.oldIndex, 1)[0];
            formsArray.splice(evt.newIndex, 0, moved);
            rerenderFn();
        }
    });
}

function deleteNode(node, parent) {
    if (parent && Array.isArray(parent.children)) {
        parent.children = parent.children.filter(n => n !== node);
    } else {
        treeData = ensureArray(treeData).filter(n => n !== node);
    }
    renderTimeline();
}

function removeFormFromNode(node, form) {
    node.forms = ensureArray(node.forms).filter(f => f !== form);
    renderTimeline();
}

// --- Rendering Helpers ---
function renderAttachmentList(container, ownerNode, formsArray, isRoot) {
    ensureArray(formsArray).forEach((form, formIndex) => {
        let att = document.createElement('span');
        att.className = 'attachment d-inline-flex align-items-center';
        att.setAttribute('data-form-index', formIndex);

        let paperclip = document.createElement('span');
        paperclip.innerHTML = `<i class="bi bi-code-square"></i> ${form ?.name || ''}`;
        att.appendChild(paperclip);

        let removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.className = 'btn btn-sm btn-link text-danger p-0 ms-1';
        removeBtn.title = 'Remove Form';
        removeBtn.innerHTML = '<i class="bi bi-x-circle"></i>';
        removeBtn.onclick = function (e) {
            e.stopPropagation();

            if (isRoot) {
                rootForms = ensureArray(rootForms).filter(f => f !== form);
            } else if (ownerNode) {
                removeFormFromNode(ownerNode, form);
                return;
            }

            renderTimeline();
        };

        att.appendChild(removeBtn);
        container.appendChild(att);
    });

    setTimeout(() => {
        if (ensureArray(formsArray).length > 1) {
            makeFormsSortable(container, formsArray, renderTimeline);
        }
    }, 0);
}

function renderAnyNode(node, parent = null) {
    return node ?.type === 'milestone'
        ? renderMilestone(node, parent)
        : renderActivity(node, parent);
}

// --- Timeline Rendering ---
function renderTimeline() {
    const timeline = document.getElementById('timeline');
    if (!timeline) return;

    timeline.innerHTML = '';

    const milestonesContainer = document.createElement('div');
    milestonesContainer.id = 'milestonesContainer';

    ensureArray(treeData).forEach(node => {
        milestonesContainer.appendChild(renderAnyNode(node, null));
    });

    timeline.appendChild(milestonesContainer);

    if (treeData.length > 1) {
        makeSortable(milestonesContainer, treeData, renderTimeline);
    }

    if (rootForms.length > 0) {
        const rootFormCard = document.createElement('div');
        rootFormCard.className = 'milestone-card p-3 mt-3';

        const header = document.createElement('div');
        header.className = 'd-flex justify-content-between align-items-center';
        header.innerHTML = '<strong>Root Forms</strong>';
        rootFormCard.appendChild(header);

        const body = document.createElement('div');
        body.className = 'mt-2';

        const attachments = document.createElement('div');
        attachments.className = 'attachments-container';

        renderAttachmentList(attachments, null, rootForms, true);

        body.appendChild(attachments);
        rootFormCard.appendChild(body);
        timeline.appendChild(rootFormCard);
    }
}

// --- Render Milestone ---
function renderMilestone(milestone, parent) {
    let card = document.createElement('div');
    card.className = 'milestone-card p-4' + (milestone ?.collapsed ? ' collapsed' : '');
    card.setAttribute('data-key', milestone ?.key || '');

    let header = document.createElement('div');
    header.className = 'd-flex justify-content-between align-items-center milestone-header';
    header.onclick = function (e) {
        if (e.target.closest('.milestone-actions')) return;
        milestone.collapsed = !milestone.collapsed;
        renderTimeline();
    };

    let chevron = document.createElement('span');
    chevron.className = 'collapse-chevron';
    chevron.innerHTML = milestone.collapsed
        ? '<i class="bi bi-chevron-right"></i>'
        : '<i class="bi bi-chevron-down"></i>';
    header.appendChild(chevron);

    let title = document.createElement('div');
    title.className = 'milestone-header-title';
    title.innerHTML =
        `<h4 class="d-inline">${milestone ?.data ?.name || ''}</h4> ` +
        `<span class="badge bg-secondary">${milestone ?.data ?.maturity || ''}</span> ` +
        `${milestone ?.data ?.isRequired ? '<span class="badge bg-warning text-dark">Required</span>' : ''}`;
    header.appendChild(title);

    let actions = document.createElement('div');
    actions.className = 'milestone-actions';

    let addActivityBtn = document.createElement('button');
    addActivityBtn.className = 'btn btn-sm btn-outline-info';
    addActivityBtn.innerHTML = '<i class="bi bi-list-task"></i>';
    addActivityBtn.title = 'Add Activity';
    addActivityBtn.onclick = function (e) {
        e.stopPropagation();
        showNodeModal({ mode: 'add', type: 'activity', parent: milestone });
    };
    actions.appendChild(addActivityBtn);

    let addFormBtn = document.createElement('button');
    addFormBtn.className = 'btn btn-sm btn-outline-secondary';
    addFormBtn.innerHTML = '<i class="bi bi-file-earmark-plus"></i>';
    addFormBtn.title = 'Add Form';
    addFormBtn.onclick = function (e) {
        e.stopPropagation();
        showFormModal(milestone);
    };
    actions.appendChild(addFormBtn);

    let editBtn = document.createElement('button');
    editBtn.className = 'btn btn-sm btn-outline-success';
    editBtn.innerHTML = '<i class="bi bi-pencil"></i>';
    editBtn.title = 'Edit';
    editBtn.onclick = function (e) {
        e.stopPropagation();
        showNodeModal({ mode: 'edit', type: 'milestone', node: milestone, parent: parent });
    };
    actions.appendChild(editBtn);

    let delBtn = document.createElement('button');
    delBtn.className = 'btn btn-sm btn-outline-danger';
    delBtn.innerHTML = '<i class="bi bi-trash"></i>';
    delBtn.title = 'Delete';
    delBtn.onclick = function (e) {
        e.stopPropagation();
        deleteNode(milestone, parent);
    };
    actions.appendChild(delBtn);

    header.appendChild(actions);
    card.appendChild(header);

    let body = document.createElement('div');
    body.className = 'milestone-body';
    if (milestone.collapsed) body.style.display = 'none';

    let desc = document.createElement('div');
    desc.className = 'text-muted mb-2';
    desc.textContent = milestone ?.data ?.desc || '';
    body.appendChild(desc);

    let attachments = document.createElement('div');
    attachments.className = 'attachments-container';
    renderAttachmentList(attachments, milestone, milestone.forms, false);
    body.appendChild(attachments);

    let childrenContainer = document.createElement('div');
    ensureArray(milestone.children).forEach(child => {
        childrenContainer.appendChild(renderAnyNode(child, milestone));
    });
    body.appendChild(childrenContainer);

    card.appendChild(body);

    setTimeout(() => {
        if (ensureArray(milestone.children).length > 1) {
            makeSortable(childrenContainer, milestone.children, renderTimeline);
        }
    }, 0);

    return card;
}

// --- Render Activity ---
function renderActivity(activity, parent) {
    let card = document.createElement('div');
    card.className = 'activity-card p-3' + (activity ?.collapsed ? ' collapsed' : '');
    card.setAttribute('data-key', activity ?.key || '');

    let header = document.createElement('div');
    header.className = 'd-flex justify-content-between align-items-center';
    header.onclick = function (e) {
        if (e.target.closest('.activity-actions')) return;
        activity.collapsed = !activity.collapsed;
        renderTimeline();
    };

    let chevron = document.createElement('span');
    chevron.className = 'collapse-chevron';
    chevron.innerHTML = activity.collapsed
        ? '<i class="bi bi-chevron-right"></i>'
        : '<i class="bi bi-chevron-down"></i>';
    header.appendChild(chevron);

    let title = document.createElement('div');
    title.className = 'milestone-header-title';
    title.innerHTML =
        `<strong>${activity ?.data ?.name || ''}</strong> ` +
        `<span class="badge bg-info text-dark">${activity ?.data ?.mandays || ''} mandays</span> ` +
        `${activity ?.data ?.isRequired ? '<span class="badge bg-warning text-dark">Required</span>' : ''}`;
    header.appendChild(title);

    let actions = document.createElement('div');
    actions.className = 'activity-actions';

    let addFormBtn = document.createElement('button');
    addFormBtn.className = 'btn btn-sm btn-outline-secondary';
    addFormBtn.innerHTML = '<i class="bi bi-file-earmark-plus"></i>';
    addFormBtn.title = 'Add Form';
    addFormBtn.onclick = function (e) {
        e.stopPropagation();
        showFormModal(activity);
    };
    actions.appendChild(addFormBtn);

    let editBtn = document.createElement('button');
    editBtn.className = 'btn btn-sm btn-outline-success';
    editBtn.innerHTML = '<i class="bi bi-pencil"></i>';
    editBtn.title = 'Edit';
    editBtn.onclick = function (e) {
        e.stopPropagation();
        showNodeModal({ mode: 'edit', type: 'activity', node: activity, parent: parent });
    };
    actions.appendChild(editBtn);

    let delBtn = document.createElement('button');
    delBtn.className = 'btn btn-sm btn-outline-danger';
    delBtn.innerHTML = '<i class="bi bi-trash"></i>';
    delBtn.title = 'Delete';
    delBtn.onclick = function (e) {
        e.stopPropagation();
        deleteNode(activity, parent);
    };
    actions.appendChild(delBtn);

    header.appendChild(actions);
    card.appendChild(header);

    let body = document.createElement('div');
    body.className = 'activity-body';
    if (activity.collapsed) body.style.display = 'none';

    let desc = document.createElement('div');
    desc.className = 'text-muted mb-2';
    desc.textContent = activity ?.data ?.desc || '';
    body.appendChild(desc);

    if (ensureArray(activity.prerequisites).length > 0) {
        let prereqDiv = document.createElement('div');
        prereqDiv.className = 'mb-2';
        prereqDiv.innerHTML = 'Prerequisites: ';

        let allActivities = collectAllActivityNodes(treeData);

        activity.prerequisites.forEach(pid => {
            let target = allActivities.find(a => a.key === pid);
            if (target) {
                let path = getMilestonePathLabel(target);
                let label = (path ? path + ' / ' : '') + (target ?.data ?.name || '');

                let chip = document.createElement('span');
                chip.className = 'chip';
                chip.innerHTML = label;
                prereqDiv.appendChild(chip);
            }
        });

        body.appendChild(prereqDiv);
    }

    let attachments = document.createElement('div');
    attachments.className = 'attachments-container';
    renderAttachmentList(attachments, activity, activity.forms, false);
    body.appendChild(attachments);

    let childrenContainer = document.createElement('div');
    ensureArray(activity.children).forEach(child => {
        childrenContainer.appendChild(renderAnyNode(child, activity));
    });
    body.appendChild(childrenContainer);

    card.appendChild(body);

    setTimeout(() => {
        if (ensureArray(activity.children).length > 1) {
            makeSortable(childrenContainer, activity.children, renderTimeline);
        }
    }, 0);

    return card;
}

// --- Import / Export UI ---
document.getElementById('exportBtn').onclick = function () {
    const json = buildRoadmapJson(treeData, rootForms);
    const blob = new Blob([json], { type: "application/json" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = "project-timeline.json";
    a.click();

    setTimeout(() => URL.revokeObjectURL(url), 1000);
};

document.getElementById('importBtn').onclick = function () {
    document.getElementById('importFile').click();
};

document.getElementById('importFile').onchange = function (e) {
    const file = e.target.files ?.[0];
    if (!file) return;

    const reader = new FileReader();

    reader.onload = function (evt) {
        try {
            const data = JSON.parse(evt.target.result);
            // Imports must always be re-keyed to avoid collisions with source roadmaps.
            importRoadmapJson(data, { forceRegenerateGuids: true });
        } catch (err) {
            console.error(err);
            alert("Invalid JSON file.");
        }
    };

    reader.readAsText(file);
    e.target.value = '';
};

// --- Root Actions ---
document.getElementById('addRootMilestoneBtn').onclick = function () {
    showNodeModal({ mode: 'add', type: 'milestone', parent: null, parentMaturity: null });
};

document.getElementById('addRootActivityBtn').onclick = function () {
    showNodeModal({ mode: 'add', type: 'activity', parent: null, parentMaturity: null });
};

document.getElementById('addRootFormBtn').onclick = function () {
    showFormModal(null);
};

document.getElementById('fabAddMilestone').onclick = function () {
    showNodeModal({ mode: 'add', type: 'milestone', parent: null, parentMaturity: null });
};

// --- Init ---
$(document).ready(function () {
    $.getJSON(getApiRootPath() + '/api/categories', function (data) {
        data = ensureArray(data);

        data.sort(function (a, b) {
            if (a.isActive !== b.isActive) {
                return b.isActive - a.isActive;
            }
            return (a.categoryName || '').localeCompare(b.categoryName || '', undefined, { sensitivity: 'base' });
        });

        $('#Categories').select2({
            data: data.map(function (item) {
                return {
                    id: item.categoryCode,
                    text: (item.categoryName || '') + (item.isActive == 0 ? ' (In Active)' : ''),
                    disabled: item.isActive == 0
                };
            }),
            placeholder: 'Select a category',
            allowClear: true,
            width: '100%'
        });
    });

    initOrRefreshModalFormsSelect();

    $('#modalForms').on('select2:select', function (e) {
        var selectedData = e.params.data;
        var selectedForm = findFormById(selectedData.id);
        $('#modalDescription').val((selectedData.description || (selectedForm && selectedForm.description)) || '');

        async function updatePageContent(id) {
            const data = await getDataAsync(getApiRootPath() + `/api/forms/${id}`);
            let formObj;

            try {
                formObj = JSON.parse(data.formJson);
            } catch (e) {
                bootbox.alert("Invalid form JSON!");
                return;
            }

            const fields = ensureArray(formObj.fields);
            const $formContainer = $('<form class="bootbox-form-container"></form>');
            $formContainer.dynamicField({ fields: fields, userCode: "*" });
            $('#containerPreview').html($formContainer);
        }

        updatePageContent(selectedData.id);
    });

    $('#categories').on('change', function () {
        $(this).valid();
    });

    (async () => {
        try {
            const data = await getDataAsync(getApiRootPath() + '/api/maturityLevels');
            __maturityList = ensureArray(data);
        } catch (error) {
            console.error('Error:', error);
        }
    })();

    (async () => {
        try {
            const data = await getDataAsync(getApiRootPath() + '/api/forms');
            __formList = ensureArray(data);
            initOrRefreshModalFormsSelect();
        } catch (error) {
            console.error('Error:', error);
        }
    })();

    renderTimeline();
});