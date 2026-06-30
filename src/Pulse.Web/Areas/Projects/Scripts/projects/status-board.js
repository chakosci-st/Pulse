(function () {
    const LANES = ['NOT STARTED', 'ONGOING', 'COMPLETED', 'HOLD', 'CANCELLED', 'ARCHIVED'];
    const MODE_CONFIG = {
        projects: {
            label: 'Project list',
            searchPlaceholder: 'Search by project name, project no, product code, plant code, or category',
            heroCopy: 'Project cards include milestone stats and task stats so you can move project health without leaving the board.',
            summaryTitle: 'Project list summary'
        },
        milestones: {
            label: 'Milestone list',
            searchPlaceholder: 'Search by milestone name, maturity code, project no, product code, plant code, or category',
            heroCopy: 'Milestone cards stay owner-scoped and surface task stats so you can adjust execution phase status directly from the board.',
            summaryTitle: 'Milestone list summary'
        },
        tasks: {
            label: 'Task list',
            searchPlaceholder: 'Search by task name, maturity code, project no, product code, plant code, or category',
            heroCopy: 'Task cards show prerequisites so you can judge readiness before moving work between lanes.',
            summaryTitle: 'Task list summary'
        }
    };

    const state = {
        mode: 'projects',
        items: [],
        filteredItems: [],
        draggingKey: null,
        query: '',
        statusFilter: '',
        permissions: {
            disabledLanes: []
        },
        collapseAll: false,
        collapsedByKey: {}
    };

    function escapeHtml(value) {
        return $('<div>').text(value == null ? '' : String(value)).html();
    }

    function normalizeLaneStatus(status) {
        const normalized = String(status || '').trim().toUpperCase();

        if (!normalized) {
            return 'NOT STARTED';
        }

        if (normalized === 'NOT_STARTED' || normalized === 'NOTSTARTED') {
            return 'NOT STARTED';
        }

        if (normalized === 'COMPLETED') {
            return 'COMPLETED';
        }

        if (normalized === 'CANCEL' || normalized === 'CANCELLED' || normalized === 'CANCELED') {
            return 'CANCELLED';
        }

        if (normalized === 'ARCHIVED') {
            return 'ARCHIVED';
        }

        if (LANES.indexOf(normalized) > -1) {
            return normalized;
        }

        return 'NOT STARTED';
    }

    function displayStatus(item) {
        const rawStatus = item.rawStatus || item.status;
        const targetDate = item.targetDate || null;

        if (typeof window.getPulseStatusMeta === 'function') {
            const meta = window.getPulseStatusMeta(rawStatus, { targetDate: targetDate });
            return normalizeLaneStatus(meta && meta.code ? meta.code : rawStatus);
        }

        const normalizedRaw = String(rawStatus || '').trim().toUpperCase();

        if (normalizedRaw === 'COMPLETED') {
            return 'COMPLETED';
        }

        if (normalizedRaw === 'CANCEL' || normalizedRaw === 'CANCELED' || normalizedRaw === 'CANCELLED') {
            return 'CANCELLED';
        }

        return normalizeLaneStatus(rawStatus);
    }

    function entityKey(item) {
        return [item.entityType || '', item.projectNo || '', item.entitySysId || '', item.nodeId || ''].join('|');
    }

    function setBoardStatus(value) {
        const el = document.getElementById('ownerBoardStatus');
        if (el) {
            el.textContent = value || '';
        }
    }

    function setLoading(isLoading, text) {
        const loadingEl = document.getElementById('ownerBoardLoading');
        const loadingTextEl = document.getElementById('ownerBoardLoadingText');

        if (loadingTextEl && text) {
            loadingTextEl.textContent = text;
        }

        if (!loadingEl) {
            return;
        }

        loadingEl.classList.toggle('d-none', !isLoading);
    }

    function updateBoardChrome() {
        const modeConfig = MODE_CONFIG[state.mode] || MODE_CONFIG.projects;
        const searchInput = document.getElementById('ownerBoardSearch');
        const summaryTitle = document.getElementById('ownerBoardSummaryTitle');
        const summaryCopy = document.getElementById('ownerBoardSummaryCopy');
        const heroCopy = document.getElementById('ownerBoardModeCopy');

        if (searchInput) {
            searchInput.placeholder = modeConfig.searchPlaceholder;
        }

        if (summaryTitle) {
            summaryTitle.textContent = modeConfig.summaryTitle;
        }

        if (summaryCopy) {
            summaryCopy.textContent = modeConfig.heroCopy;
        }

        if (heroCopy) {
            heroCopy.textContent = modeConfig.heroCopy;
        }

        document.querySelectorAll('.project-view-mode-btn[data-board-mode]').forEach(function (button) {
            const active = button.getAttribute('data-board-mode') === state.mode;
            button.classList.toggle('active', active);
            button.setAttribute('aria-pressed', String(active));
        });
    }

    function updateSummaryCounts() {
        const visibleEl = document.getElementById('ownerBoardVisibleCount');
        const totalEl = document.getElementById('ownerBoardTotalCount');

        if (visibleEl) {
            visibleEl.textContent = String(state.filteredItems.length);
        }

        if (totalEl) {
            totalEl.textContent = String(state.items.length);
        }
    }

    function itemSearchBlob(item) {
        const base = [item.name, item.projectNo, item.productCodes, item.plantCode, item.categoryCode];
        if (state.mode !== 'projects') {
            base.push(item.maturityCode);
        }
        if (state.mode === 'tasks') {
            (item.prerequisites || []).forEach(function (value) {
                base.push(value);
            });
        }
        return base.filter(Boolean).join(' ').toLowerCase();
    }

    function applyFilters() {
        const normalizedQuery = String(state.query || '').trim().toLowerCase();
        const normalizedStatus = String(state.statusFilter || '').trim().toUpperCase();

        state.filteredItems = state.items.filter(function (item) {
            const matchesQuery = !normalizedQuery || itemSearchBlob(item).indexOf(normalizedQuery) > -1;
            const itemDisplayStatus = displayStatus(item);
            const matchesStatus = !normalizedStatus || normalizeLaneStatus(itemDisplayStatus) === normalizeLaneStatus(normalizedStatus);
            return matchesQuery && matchesStatus;
        });
    }

    function laneBody(status) {
        return document.querySelector('[data-lane-body="' + status + '"]');
    }

    function laneCountEl(status) {
        return document.querySelector('[data-lane-count="' + status + '"]');
    }

    function normalizePermissionLanes(value) {
        if (!Array.isArray(value)) {
            return [];
        }

        return value
            .map(function (status) {
                return normalizeLaneStatus(status);
            })
            .filter(function (status, index, source) {
                return source.indexOf(status) === index;
            });
    }

    function isLaneDropDisabled(status) {
        const disabledLanes = state.permissions && Array.isArray(state.permissions.disabledLanes)
            ? state.permissions.disabledLanes
            : [];

        return disabledLanes.indexOf(normalizeLaneStatus(status)) > -1;
    }

    function applyLanePermissions() {
        document.querySelectorAll('.status-lane').forEach(function (lane) {
            const laneStatus = normalizeLaneStatus(lane.getAttribute('data-lane-status'));
            const isDisabled = isLaneDropDisabled(laneStatus);

            lane.classList.toggle('status-lane--drop-disabled', isDisabled);
            lane.setAttribute('aria-disabled', String(isDisabled));
            lane.title = isDisabled
                ? 'You are not allowed to move items to this lane for the current mode.'
                : '';
        });
    }

    function isCollapsed(item) {
        const key = entityKey(item);
        if (Object.prototype.hasOwnProperty.call(state.collapsedByKey, key)) {
            return !!state.collapsedByKey[key];
        }
        return !!state.collapseAll;
    }

    function setCollapsed(item, value) {
        state.collapsedByKey[entityKey(item)] = !!value;
    }

    function syncCollapseButton() {
        const button = document.getElementById('ownerBoardToggleCollapse');
        if (!button) {
            return;
        }

        const allCollapsed = state.items.length > 0 && state.items.every(function (item) {
            return isCollapsed(item);
        });

        button.innerHTML = allCollapsed
            ? '<i class="bi bi-arrows-expand"></i> Expand All'
            : '<i class="bi bi-arrows-collapse"></i> Collapse All';
    }

    function buildStats(item) {
        if (state.mode === 'projects') {
            return [
                { label: 'Milestones', value: item.milestoneCount || 0 },
                { label: 'Milestone Completed', value: item.milestoneClosedCount || 0 },
                { label: 'Tasks', value: item.taskCount || 0 },
                { label: 'Task Pending', value: item.taskPendingCount || 0 }
            ];
        }

        if (state.mode === 'milestones') {
            return [
                { label: 'Tasks', value: item.taskCount || 0 },
                { label: 'Pending', value: item.taskPendingCount || 0 },
                { label: 'Ongoing', value: item.taskOngoingCount || 0 },
                { label: 'Completed', value: item.taskClosedCount || 0 }
            ];
        }

        const prerequisiteTotal = Number(item.prerequisitesTotalCount || 0);
        const prerequisiteSatisfied = Number(item.prerequisitesSatisfiedCount || 0);
        const prerequisiteDisplay = prerequisiteTotal > 0
            ? (String(prerequisiteSatisfied) + ' / ' + String(prerequisiteTotal))
            : '0';

        return [
            { label: 'Prereqs', value: prerequisiteDisplay },
            { label: 'Parent', value: item.parentNodeName || '-' }
        ];
    }

    function buildTags(item) {
        if (state.mode === 'tasks') {
            return (item.prerequisites || []).slice(0, 4).map(function (value) {
                return '<span class="status-card__tag"><i class="bi bi-link-45deg"></i>' + escapeHtml(value) + '</span>';
            }).join('');
        }

        return [
            item.maturityCode ? '<span class="status-card__tag"><i class="bi bi-diagram-3"></i>' + escapeHtml(item.maturityCode) + '</span>' : '',
            item.productCodes ? '<span class="status-card__tag"><i class="bi bi-box-seam"></i>' + escapeHtml(item.productCodes) + '</span>' : ''
        ].join('');
    }

    function buildCollapsedPreview(item) {
        if (state.mode === 'milestones') {
            return [
                '<div><strong>Project Name:</strong> ' + escapeHtml(item.projectName || '-') + '</div>',
                '<div><strong>Plant / Category:</strong> ' + escapeHtml((item.plantCode || '-') + ' / ' + (item.categoryCode || '-')) + '</div>'
            ].join('');
        }

        if (state.mode === 'tasks') {
            return [
                '<div><strong>Project Name:</strong> ' + escapeHtml(item.projectName || '-') + '</div>',
                '<div><strong>Plant / Category:</strong> ' + escapeHtml((item.plantCode || '-') + ' / ' + (item.categoryCode || '-')) + '</div>',
                '<div><strong>Parent Node:</strong> ' + escapeHtml(item.parentNodeName || '-') + '</div>'
            ].join('');
        }

        return '';
    }

    function cardTemplate(item) {
        const statsMarkup = buildStats(item).map(function (entry) {
            return '' +
                '<div class="status-card__stat">' +
                '  <div class="status-card__stat-value">' + escapeHtml(entry.value) + '</div>' +
                '  <div class="status-card__stat-label">' + escapeHtml(entry.label) + '</div>' +
                '</div>';
        }).join('');

        const metaLines = [
            state.mode === 'projects'
                ? '<div><strong>Project No:</strong> ' + escapeHtml(item.projectNo || '-') + '</div>'
                : '<div><strong>Project:</strong> ' + escapeHtml((item.projectNo || '-') + ' · ' + (item.projectName || '')) + '</div>',
            item.maturityCode ? '<div><strong>Maturity:</strong> ' + escapeHtml(item.maturityCode) + '</div>' : '',
            '<div><strong>Plant / Category:</strong> ' + escapeHtml((item.plantCode || '-') + ' / ' + (item.categoryCode || '-')) + '</div>'
        ].filter(Boolean).join('');

        const collapsed = isCollapsed(item);
        const collapsedPreview = buildCollapsedPreview(item);

        return '' +
            '<article class="status-card ' + (collapsed ? 'is-collapsed' : '') + '" draggable="true" data-item-key="' + escapeHtml(entityKey(item)) + '">' +
            '   <div class="status-card__head">' +
            '       <h3 class="status-card__title">' + escapeHtml(item.name || item.projectNo) + '</h3>' +
            '       <button type="button" class="status-card__collapse-btn" aria-label="Toggle card details"><i class="bi bi-chevron-down"></i></button>' +
            '   </div>' +
            (collapsedPreview ? '   <div class="status-card__collapsed-preview">' + collapsedPreview + '</div>' : '') +
            '   <div class="status-card__body">' +
            '       <div class="status-card__meta">' + metaLines + '</div>' +
            '       <div class="status-card__stats">' + statsMarkup + '</div>' +
            (buildTags(item) ? '<div class="status-card__tags">' + buildTags(item) + '</div>' : '') +
            '   </div>' +
            '</article>';
    }

    function bindCardDnD(card) {
        card.addEventListener('dragstart', function () {
            state.draggingKey = card.getAttribute('data-item-key');
            card.classList.add('dragging');
        });

        card.addEventListener('dragend', function () {
            state.draggingKey = null;
            card.classList.remove('dragging');
            document.querySelectorAll('.status-lane.drag-over').forEach(function (lane) {
                lane.classList.remove('drag-over');
            });
        });
    }

    function renderBoard() {
        const grouped = {
            'NOT STARTED': [],
            'ONGOING': [],
            'HOLD': [],
            'COMPLETED': [],
            'CANCELLED': [],
            'ARCHIVED': []
        };

        state.filteredItems.forEach(function (item) {
            grouped[normalizeLaneStatus(item.status || item.rawStatus)].push(item);
        });

        LANES.forEach(function (lane) {
            const body = laneBody(lane);
            const countEl = laneCountEl(lane);
            if (!body) {
                return;
            }

            const laneItems = grouped[lane] || [];
            if (countEl) {
                countEl.textContent = String(laneItems.length);
            }

            if (!laneItems.length) {
                body.innerHTML = '<div class="status-lane__empty">No owner items in this lane.</div>';
                return;
            }

            body.innerHTML = laneItems.map(cardTemplate).join('');
            body.querySelectorAll('.status-card').forEach(bindCardDnD);
        });

        updateSummaryCounts();
        syncCollapseButton();
    }

    function findItemByKey(itemKey) {
        return state.items.find(function (item) {
            return entityKey(item) === itemKey;
        });
    }

    function promptForComment(item, currentStatus, newStatus) {
        const itemLabel = item.name || item.projectNo || 'this item';
        if (window.bootbox && typeof window.bootbox.dialog === 'function') {
            return new Promise(function (resolve) {
                window.bootbox.dialog({
                    title: 'Comment is required',
                    message: '' +
                        '<div class="mb-2">Move <strong>' + escapeHtml(itemLabel) + '</strong> from <strong>' + escapeHtml(currentStatus) + '</strong> to <strong>' + escapeHtml(newStatus) + '</strong>.</div>' +
                        '<textarea id="ownerBoardStatusComment" class="form-control" rows="5" placeholder="Enter your comment"></textarea>' +
                        '<div id="ownerBoardStatusCommentError" class="text-danger small mt-2 d-none">Comment is required.</div>',
                    buttons: {
                        cancel: {
                            label: 'Cancel',
                            className: 'btn-secondary',
                            callback: function () {
                                resolve(null);
                            }
                        },
                        confirm: {
                            label: 'Save Status',
                            className: 'btn-primary',
                            callback: function () {
                                const commentEl = document.getElementById('ownerBoardStatusComment');
                                const errorEl = document.getElementById('ownerBoardStatusCommentError');
                                const value = commentEl ? String(commentEl.value || '').trim() : '';

                                if (!value) {
                                    if (errorEl) {
                                        errorEl.classList.remove('d-none');
                                    }
                                    return false;
                                }

                                resolve(value);
                                return true;
                            }
                        }
                    }
                });
            });
        }

        const fallback = window.prompt('Add comment (required) for status change from ' + currentStatus + ' to ' + newStatus + ':', '');
        const trimmed = String(fallback || '').trim();
        return Promise.resolve(trimmed || null);
    }

    function updateItemStatus(item, newStatus, comment) {
        return $.ajax({
            url: getApiRootPath() + '/api/projects/owner-board/status',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                entityType: item.entityType,
                entitySysId: item.entitySysId,
                nodeId: item.nodeId,
                projectNo: item.projectNo,
                newStatus: newStatus,
                comment: comment,
                transactionKey: item.transactionKey
            })
        });
    }

    async function handleLaneDrop(targetStatus) {
        if (isLaneDropDisabled(targetStatus)) {
            if (window.toastr) {
                window.toastr.warning('You are not allowed to move items to this lane.');
            }
            return;
        }

        const item = findItemByKey(state.draggingKey);
        if (!item) {
            return;
        }

        const currentStatus = normalizeLaneStatus(item.status || item.rawStatus);
        if (currentStatus === targetStatus) {
            return;
        }

        const comment = await promptForComment(item, currentStatus, targetStatus);
        if (!comment) {
            if (window.toastr) {
                window.toastr.warning('Comment is required to move an item to another lane.');
            }
            return;
        }

        setBoardStatus('Saving status update...');
        setLoading(true, 'Saving status...');

        try {
            const result = await updateItemStatus(item, targetStatus, comment);
            item.entitySysId = result && result.entitySysId ? result.entitySysId : item.entitySysId;
            item.status = result && result.status ? result.status : targetStatus;
            item.rawStatus = result && result.rawStatus ? result.rawStatus : item.rawStatus;
            item.transactionKey = result && result.transactionKey ? result.transactionKey : item.transactionKey;

            applyFilters();
            renderBoard();
            setBoardStatus('Updated ' + (item.name || item.projectNo || 'item'));

            if (window.toastr) {
                window.toastr.success('Status updated successfully.');
                if (result && result.cascadeMessage) {
                    window.toastr.info(result.cascadeMessage);
                }
            }
        } catch (xhr) {
            const apiMessage = xhr && xhr.responseJSON && xhr.responseJSON.message
                ? xhr.responseJSON.message
                : (xhr && xhr.responseText ? xhr.responseText : 'Failed to update status.');
            setBoardStatus('Update failed');
            if (window.toastr) {
                window.toastr.error(apiMessage);
            } else {
                alert(apiMessage);
            }
        } finally {
            setLoading(false);
        }
    }

    function bindLaneDnD() {
        document.querySelectorAll('.status-lane').forEach(function (lane) {
            const laneStatus = lane.getAttribute('data-lane-status');
            lane.addEventListener('dragover', function (event) {
                if (isLaneDropDisabled(laneStatus)) {
                    return;
                }

                event.preventDefault();
                lane.classList.add('drag-over');
            });
            lane.addEventListener('dragleave', function () {
                lane.classList.remove('drag-over');
            });
            lane.addEventListener('drop', function (event) {
                if (isLaneDropDisabled(laneStatus)) {
                    return;
                }

                event.preventDefault();
                lane.classList.remove('drag-over');
                handleLaneDrop(laneStatus);
            });
        });
    }

    function bindCardCollapse() {
        const lanes = document.getElementById('ownerBoardLanes');
        if (!lanes || lanes.dataset.collapseBound === 'true') {
            return;
        }

        lanes.dataset.collapseBound = 'true';
        lanes.addEventListener('click', function (event) {
            const toggleButton = event.target.closest('.status-card__collapse-btn');
            if (!toggleButton) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();

            const card = toggleButton.closest('.status-card[data-item-key]');
            if (!card) {
                return;
            }

            const item = findItemByKey(card.getAttribute('data-item-key'));
            if (!item) {
                return;
            }

            const nextCollapsed = !isCollapsed(item);
            setCollapsed(item, nextCollapsed);
            card.classList.toggle('is-collapsed', nextCollapsed);
            syncCollapseButton();
        });
    }

    async function loadBoardMode(mode) {
        state.mode = mode;
        updateBoardChrome();
        setBoardStatus('Loading ' + (MODE_CONFIG[mode] ? MODE_CONFIG[mode].label.toLowerCase() : 'board items') + '...');
        setLoading(true, 'Loading ' + (MODE_CONFIG[mode] ? MODE_CONFIG[mode].label.toLowerCase() : 'items') + '...');

        try {
            const result = await $.ajax({
                url: getApiRootPath() + '/api/projects/owner-board',
                method: 'GET',
                cache: false,
                data: { mode: mode }
            });

            state.permissions = {
                disabledLanes: normalizePermissionLanes(result && result.permissions ? result.permissions.disabledLanes : [])
            };
            applyLanePermissions();

            state.items = Array.isArray(result && result.data)
                ? result.data.map(function (item) {
                    item.status = normalizeLaneStatus(item.status || item.rawStatus);
                    item.prerequisites = Array.isArray(item.prerequisites) ? item.prerequisites : [];
                    return item;
                })
                : [];

            applyFilters();
            renderBoard();
            setBoardStatus(state.items.length ? 'Ready' : 'No owner items found');
        } catch (xhr) {
            state.permissions = { disabledLanes: [] };
            applyLanePermissions();
            state.items = [];
            state.filteredItems = [];
            renderBoard();
            setBoardStatus('Failed to load board');
            const apiMessage = xhr && xhr.responseJSON && xhr.responseJSON.message
                ? xhr.responseJSON.message
                : 'Unable to load owner board data.';
            if (window.toastr) {
                window.toastr.error(apiMessage);
            } else {
                alert(apiMessage);
            }
        } finally {
            setLoading(false);
        }
    }

    function bindToolbar() {
        const searchInput = document.getElementById('ownerBoardSearch');
        const statusFilter = document.getElementById('ownerBoardStatusFilter');
        const collapseButton = document.getElementById('ownerBoardToggleCollapse');

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                state.query = String(searchInput.value || '');
                applyFilters();
                renderBoard();
            });
        }

        if (statusFilter) {
            statusFilter.addEventListener('change', function () {
                state.statusFilter = String(statusFilter.value || '');
                applyFilters();
                renderBoard();
            });
        }

        document.querySelectorAll('.project-view-mode-btn[data-board-mode]').forEach(function (button) {
            button.addEventListener('click', function () {
                const nextMode = button.getAttribute('data-board-mode') || 'projects';
                if (nextMode === state.mode) {
                    return;
                }
                loadBoardMode(nextMode);
            });
        });

        if (collapseButton) {
            collapseButton.addEventListener('click', function () {
                const allCollapsed = state.items.length > 0 && state.items.every(function (item) {
                    return isCollapsed(item);
                });

                const nextValue = !allCollapsed;
                state.collapseAll = nextValue;
                state.items.forEach(function (item) {
                    setCollapsed(item, nextValue);
                });

                renderBoard();
            });
        }
    }

    $(document).ready(function () {
        bindToolbar();
        bindLaneDnD();
        bindCardCollapse();
        updateBoardChrome();
        loadBoardMode(state.mode);
    });
})();
