(function () {
    const moduleCatalog = Array.isArray(window.pulseSearchModules) ? window.pulseSearchModules : [];

    function escapeHtml(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function formatDate(value) {
        if (!value) {
            return 'No target date';
        }

        const parsed = moment(value);
        if (!parsed.isValid()) {
            return 'No target date';
        }

        return parsed.format('DD MMM YYYY');
    }

    function getQuery() {
        const url = new URL(window.location.href);
        return (url.searchParams.get('q') || '').trim();
    }

    function setSummary(query, totalCount) {
        const title = document.getElementById('globalSearchSummaryTitle');
        const copy = document.getElementById('globalSearchSummaryCopy');
        const count = document.getElementById('globalSearchSummaryCount');

        if (!title || !copy || !count) {
            return;
        }

        if (!query) {
            title.textContent = 'Global Search';
            copy.textContent = 'Search across project records and only the modules available to you.';
            count.textContent = 'Waiting for query';
            return;
        }

        title.textContent = `Results for "${query}"`;
        copy.textContent = 'Use the results below to jump into matching project records or modules you can access.';
        count.textContent = `${Number(totalCount || 0).toLocaleString('en-US')} matches`;
    }

    function matchModules(query) {
        if (!query) {
            return [];
        }

        const normalizedQuery = query.toLowerCase();
        return moduleCatalog.filter(function (item) {
            const haystack = `${item.title} ${item.description} ${item.keywords}`.toLowerCase();
            return haystack.indexOf(normalizedQuery) >= 0;
        });
    }

    function normalizeResultType(result) {
        const raw = (result.resultType || '').toLowerCase();
        if (raw === 'milestone') {
            return 'Milestone';
        }

        if (raw === 'activity') {
            return 'Task';
        }

        if (raw === 'project') {
            return 'Project';
        }

        return raw ? raw.charAt(0).toUpperCase() + raw.slice(1) : 'Project';
    }

    function renderModuleResults(items) {
        const host = document.getElementById('globalSearchModuleResults');
        if (!host) {
            return;
        }

        if (!items.length) {
            host.innerHTML = '<div class="global-search-empty">No matching modules found for this search.</div>';
            return;
        }

        host.innerHTML = `
            <div class="global-search-grid">
                ${items.map(function (item) {
                    return `
                        <article class="global-search-card">
                            <div class="global-search-card-header">
                                <div>
                                    <div class="global-search-kicker"><i class="bi bi-grid"></i>Module</div>
                                    <h4 class="global-search-card-title">${escapeHtml(item.title)}</h4>
                                    <p class="global-search-card-subtitle">${escapeHtml(item.description)}</p>
                                </div>
                            </div>
                            <div class="global-search-actions">
                                <span class="global-search-meta-item"><i class="bi bi-lightning"></i>Quick access</span>
                                <a class="btn btn-outline-primary btn-sm" href="${escapeHtml(item.url)}">Open</a>
                            </div>
                        </article>`;
                }).join('')}
            </div>`;
    }

    function renderProjectResults(items) {
        const host = document.getElementById('globalSearchProjectResults');
        if (!host) {
            return;
        }

        if (!items.length) {
            host.innerHTML = '<div class="global-search-empty">No matching project, milestone, or task records were found.</div>';
            return;
        }

        host.innerHTML = `
            <div class="global-search-grid">
                ${items.map(function (item) {
                    const resultType = normalizeResultType(item);
                    const title = item.nodeName || item.projectName || item.projectNo;
                    const subtitle = item.nodeName
                        ? `${item.projectNo} · ${item.projectName || 'Project result'}`
                        : `${item.projectNo} · Project workspace`;
                    const path = item.nodeFullPath || item.projectName || item.projectNo;
                    const owner = item.ownerName || 'Unassigned';
                    const statusBadge = typeof getPulseStatusBadge === 'function'
                        ? getPulseStatusBadge(item.status || 'NOT STARTED', { targetDate: item.targetCompletion })
                        : `<span class="badge bg-secondary">${escapeHtml(item.status || 'NOT STARTED')}</span>`;
                    const meta = [
                        item.plantCode ? `<span class="global-search-meta-item"><i class="bi bi-geo-alt"></i>${escapeHtml(item.plantCode)}</span>` : '',
                        item.categoryCode ? `<span class="global-search-meta-item"><i class="bi bi-diagram-3"></i>${escapeHtml(item.categoryCode)}</span>` : '',
                        item.productCodes ? `<span class="global-search-meta-item"><i class="bi bi-box-seam"></i>${escapeHtml(item.productCodes)}</span>` : '',
                        `<span class="global-search-meta-item"><i class="bi bi-person"></i>${escapeHtml(owner)}</span>`,
                        `<span class="global-search-meta-item"><i class="bi bi-calendar-event"></i>${escapeHtml(formatDate(item.targetCompletion))}</span>`
                    ].filter(Boolean).join('');

                    return `
                        <article class="global-search-card">
                            <div class="global-search-card-header">
                                <div>
                                    <div class="global-search-kicker"><i class="bi bi-search"></i>${escapeHtml(resultType)}</div>
                                    <h4 class="global-search-card-title">${escapeHtml(title)}</h4>
                                    <p class="global-search-card-subtitle">${escapeHtml(subtitle)}</p>
                                </div>
                                <div>${statusBadge}</div>
                            </div>
                            <div class="global-search-path">${escapeHtml(path)}</div>
                            <div class="global-search-meta">${meta}</div>
                            <div class="global-search-actions">
                                <span class="global-search-meta-item"><i class="bi bi-folder2-open"></i>${escapeHtml(item.projectNo)}</span>
                                <a class="btn btn-primary btn-sm" href="${escapeHtml(item.linkUrl || `/Projects/${encodeURIComponent(item.projectNo)}/Overview`)}">Open project</a>
                            </div>
                        </article>`;
                }).join('')}
            </div>`;
    }

    function renderLoadingState() {
        const projectHost = document.getElementById('globalSearchProjectResults');
        const moduleHost = document.getElementById('globalSearchModuleResults');

        if (moduleHost) {
            moduleHost.innerHTML = '<div class="global-search-loading">Checking modules...</div>';
        }

        if (projectHost) {
            projectHost.innerHTML = '<div class="global-search-loading">Searching project records...</div>';
        }
    }

    function fetchProjectResults(query) {
        return $.ajax({
            url: getApiRootPath() + '/api/projects/search/global',
            type: 'GET',
            dataType: 'json',
            data: {
                q: query,
                take: 80
            }
        });
    }

    async function loadResults() {
        const query = getQuery();
        const input = document.getElementById('globalSearchInput');
        if (input) {
            input.value = query;
        }

        if (!query) {
            setSummary('', 0);
            renderModuleResults([]);
            const projectHost = document.getElementById('globalSearchProjectResults');
            if (projectHost) {
                projectHost.innerHTML = '<div class="global-search-empty">Enter a search term above to find projects, milestones, tasks, or Pulse modules.</div>';
            }
            return;
        }

        renderLoadingState();

        const moduleResults = matchModules(query);

        try {
            const projectResults = await fetchProjectResults(query);
            setSummary(query, moduleResults.length + projectResults.length);
            renderModuleResults(moduleResults);
            renderProjectResults(projectResults || []);
        } catch (error) {
            console.error('Global search failed', error);
            setSummary(query, moduleResults.length);
            renderModuleResults(moduleResults);

            const projectHost = document.getElementById('globalSearchProjectResults');
            if (projectHost) {
                projectHost.innerHTML = '<div class="global-search-empty">Project search is temporarily unavailable. Module results are still shown above.</div>';
            }
        }
    }

    $(document).ready(function () {
        loadResults();
    });
})();