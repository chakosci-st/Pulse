let user = null;
//var currentUserRoom = null;

function initializeNavbarSearch() {
    const shell = document.getElementById('pulseNavbarSearchShell');
    const toggle = document.getElementById('pulseNavbarSearchToggle');
    const closeBtn = document.getElementById('pulseNavbarSearchClose');
    const form = document.getElementById('pulseNavbarSearchForm');
    const input = document.getElementById('pulseNavbarSearchInput');

    if (!shell || !toggle || !closeBtn || !form || !input) {
        return;
    }

    const openSearch = function () {
        shell.classList.add('is-open');
        toggle.setAttribute('aria-expanded', 'true');
        window.requestAnimationFrame(function () {
            input.focus();
            input.select();
        });
    };

    const closeSearch = function () {
        shell.classList.remove('is-open');
        toggle.setAttribute('aria-expanded', 'false');
        input.blur();
    };

    toggle.addEventListener('click', function (event) {
        event.preventDefault();

        if (!shell.classList.contains('is-open')) {
            openSearch();
            return;
        }

        if ((input.value || '').trim().length > 0) {
            if (typeof form.requestSubmit === 'function') {
                form.requestSubmit();
            } else {
                form.submit();
            }
            return;
        }

        openSearch();
    });

    closeBtn.addEventListener('click', function () {
        input.value = '';
        closeSearch();
    });

    form.addEventListener('submit', function (event) {
        const normalizedQuery = (input.value || '').trim();
        if (!normalizedQuery) {
            event.preventDefault();
            openSearch();
            return;
        }

        input.value = normalizedQuery;
    });

    input.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            event.preventDefault();
            closeSearch();
        }
    });

    document.addEventListener('click', function (event) {
        if (!shell.classList.contains('is-open')) {
            return;
        }

        if (shell.contains(event.target)) {
            return;
        }

        if ((input.value || '').trim().length === 0) {
            closeSearch();
        }
    });
}

function formatUserMenuCount(value) {
    const number = Number(value);
    if (!Number.isFinite(number)) {
        return '--';
    }

    return number.toLocaleString('en-US');
}

function setUserMenuSummaryState(label) {
    const summaryState = document.getElementById('userMenuSummaryState');
    if (!summaryState) {
        return;
    }

    summaryState.textContent = label;
}

function setUserMenuCounter(id, value) {
    const element = document.getElementById(id);
    if (!element) {
        return;
    }

    element.textContent = formatUserMenuCount(value);
}

function syncUserMenuChatCount(count) {
    const element = document.getElementById('userMenuMessagesCount');
    if (!element) {
        return;
    }

    const normalizedCount = Number.isFinite(Number(count)) ? Number(count) : 0;
    element.textContent = normalizedCount.toLocaleString('en-US');
}

function fetchUserMenuCountersAsync() {
    return $.ajax({
        url: getApiRootPath() + '/api/projects/dashboard/counter',
        type: 'GET',
        dataType: 'json'
    });
}

function populateUserMenuCounters(counter) {
    const safeCounter = counter || {};

    setUserMenuCounter('userMenuProjectsCount', safeCounter.activeProjects);
    setUserMenuCounter('userMenuTasksCount', safeCounter.inProgress);
    setUserMenuCounter('userMenuDueCount', safeCounter.overdue);
    setUserMenuSummaryState('Updated');
}

function fetchLoggedUserAsync() {
    return $.ajax({
        url: "/Account/Me",
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    });
}
$(document).ready(async function () { 
    initializeNavbarSearch();

    try {
        user = await fetchLoggedUserAsync(); 
        window.user = user;

        const counter = await fetchUserMenuCountersAsync();
        populateUserMenuCounters(counter);
    } catch (err) {
        console.error('Error initializing page', err);
        setUserMenuSummaryState('Unavailable');
    } finally { 
    }
});