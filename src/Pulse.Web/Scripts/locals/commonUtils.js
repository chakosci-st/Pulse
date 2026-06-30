var loadingDialog = null;

function clearHeroRegionIfAllowed(selector) {
    const region = document.querySelector(selector);
    if (!region) {
        return;
    }

    if (region.dataset && region.dataset.preserveDefault === 'true') {
        return;
    }

    region.innerHTML = '';
}

function resetHeroDecorations(options) {
    const settings = options || {};

    if (settings.clearChips !== false) {
        clearHeroRegionIfAllowed('#chips');
    }

    if (settings.clearSubcontent !== false) {
        clearHeroRegionIfAllowed('#subcontent');
    }
}

function roundTo(num, decimals) {
    const factor = Math.pow(10, decimals);
    return Math.round(num * factor) / factor;
}

async function loadCurrentUser() {
    const res = await fetch("/Account/Me");

    if (!res.ok) {
        console.error("Not authenticated");
        return null;
    }

    const user = await res.json(); 
    return user;
}


function showLoading(message) {
    const msg = message || 'Creating project, please wait...';
    if (loadingDialog) {
        try { loadingDialog.modal('hide'); } catch (e) { }
    }

    loadingDialog = bootbox.dialog({
        message: `
            <div class="text-center py-3">
                <div class="spinner-border text-primary mb-2" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div>${msg}</div>
            </div>
        `,
        closeButton: false,
        backdrop: 'static',
        onEscape: false,
        size: 'small',
        centerVertical: true
    });
}

function hideLoading() {
    if (loadingDialog) {
        try {
            loadingDialog.modal('hide');
        } catch (e) { }
        loadingDialog = null;
    }
}



async function getDataAsync(url) {
    const headers = {};
    if (typeof apiPath !== 'undefined' && url.indexOf(apiPath) === 0) {
        if (!pulseJwtToken && typeof initJwtToken === 'function') {
            initJwtToken();
        }

        if (pulseJwtToken) {
            headers.Authorization = 'Bearer ' + pulseJwtToken;
        }
    }

    const response = await fetch(url, {
        headers: headers
    });
    if (!response.ok) {
        throw new Error('Network response was not ok');
    }
    const data = await response.json();
    return data;
}

// Convert a single string (e.g. "root_forms" or "rootForms") to "rootForms"
function toCamel(str) {
    if (!str || typeof str !== 'string') return str;
    return str
        // handle snake_case or kebab-case
        .replace(/[-_](.)/g, (_, ch) => ch.toUpperCase())
        // ensure first char is lowercase
        .replace(/^(.)/, (m) => m.toLowerCase());
}

// Recursively convert all object keys to camelCase
function keysToCamel(input) {
    if (Array.isArray(input)) {
        return input.map(keysToCamel);
    }
    if (input && typeof input === 'object') {
        const obj = {};
        Object.keys(input).forEach((key) => {
            const camelKey = toCamel(key);
            obj[camelKey] = keysToCamel(input[key]);
        });
        return obj;
    }
    return input;
}

async function populateSelect(url, selectId) {
    const select = document.getElementById(selectId);
    select.innerHTML = '<option value="">Loading...</option>'; // Optional: show loading

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error('Network error');
        const data = await response.json();

        // Clear existing options
        select.innerHTML = '';

        // Populate options
        data.forEach(item => {
            const option = document.createElement('option');
            option.value = item.id;
            option.textContent = item.name;
            select.appendChild(option);
        });
    } catch (error) {
        select.innerHTML = '<option value="">Failed to load</option>';
        console.error(error);
    }
}

function getData(url) {
    fetch(url) // Replace with your API endpoint
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json(); // Parse JSON data
        })
        .then(data => {
            console.log('Success:', data);
            // Handle the data here
        })
        .catch(error => {
            console.error('Error:', error);
        })
}

function normalizePulseStatus(status) {
    const normalized = (status || '')
        .toString()
        .trim()
        .replace(/[_-]+/g, ' ')
        .replace(/\s+/g, ' ')
        .toUpperCase();

    if (!normalized) {
        return 'NOT STARTED';
    }

    switch (normalized) {
        case 'NOT STARTED':
        case 'TODO':
        case 'TO DO':
        case 'OPEN':
        case 'PENDING':
            return 'NOT STARTED';
        case 'ONGOING':
        case 'IN PROGRESS':
        case 'INPROGRESS':
            return 'ONGOING';
        case 'COMPLETED':
            return 'COMPLETED';
        case 'CANCELLED':
        case 'CANCELED':
            return 'CANCELLED';
        case 'HOLD':
        case 'ON HOLD':
        case 'ON-HOLD':
        case 'INACTIVE':
            return 'HOLD';
        case 'ARCHIVED':
            return 'ARCHIVED';
        default:
            return normalized || 'UNKNOWN';
    }
}

function getPulseStatusMeta(status, options) {
    const settings = options || {};
    const baseStatus = normalizePulseStatus(status);
    let resolvedStatus = baseStatus;

    const statusMap = {
        'NOT STARTED': {
            code: 'NOT STARTED',
            label: 'Not Started',
            modifierClass: 'badge-status-not-started',
            iconClass: 'far fa-pause-circle'
        },
        'ONGOING': {
            code: 'ONGOING',
            label: 'Ongoing',
            modifierClass: 'badge-status-ongoing',
            iconClass: 'fas fa-spinner'
        },
        'CANCELLED': {
            code: 'CANCELLED',
            label: 'Cancelled',
            modifierClass: 'badge-status-cancelled',
            iconClass: 'fas fa-ban'
        },
        'COMPLETED': {
            code: 'COMPLETED',
            label: 'Completed',
            modifierClass: 'badge-status-closed',
            iconClass: 'fas fa-check'
        },
        'HOLD': {
            code: 'HOLD',
            label: 'On-Hold',
            modifierClass: 'badge-status-hold',
            iconClass: 'fas fa-hand'
        },
        'ARCHIVED': {
            code: 'ARCHIVED',
            label: 'Archived',
            modifierClass: 'badge-status-unknown',
            iconClass: 'fas fa-box-archive'
        },
        'UNKNOWN': {
            code: 'UNKNOWN',
            label: 'Unknown',
            modifierClass: 'badge-status-unknown',
            iconClass: 'fas fa-question-circle'
        }
    };

    return statusMap[resolvedStatus] || statusMap.UNKNOWN;
}

function getPulseStatusText(status, options) {
    return getPulseStatusMeta(status, options).label;
}

function getPulseStatusClassName(status, options) {
    const meta = getPulseStatusMeta(status, options);
    return `badge badge-status ${meta.modifierClass}`;
}

function getPulseStatusBadge(status, options) {
    const settings = options || {};
    const meta = getPulseStatusMeta(status, settings);
    const iconHtml = settings.showIcon === false ? '' : `<i class="${meta.iconClass} me-1"></i>`;
    const title = settings.title ? ` title="${String(settings.title).replace(/"/g, '&quot;')}"` : '';
    const extraClasses = settings.extraClasses ? ` ${settings.extraClasses}` : '';

    return `<span class="badge badge-status ${meta.modifierClass}${extraClasses}"${title}>${iconHtml}${meta.label}</span>`;
}