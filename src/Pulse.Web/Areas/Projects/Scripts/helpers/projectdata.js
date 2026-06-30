// =========================
// API helpers (async)
// =========================
function normalizeProjectDataNodeType(nodetype) {
    const normalized = String(nodetype || '').trim().toLowerCase();
    if (normalized === 'task') {
        return 'activity';
    }

    return normalized;
}

function fetchProjectDataAsync(projectno, nodetype, nodeid, alternateNodeIds) {
    const normalizedNodeType = normalizeProjectDataNodeType(nodetype);
    const nodeCandidates = [nodeid].concat(Array.isArray(alternateNodeIds) ? alternateNodeIds : [])
        .map(value => String(value || '').trim())
        .filter((value, index, list) => value && list.indexOf(value) === index);

    const safeProjectNo = encodeURIComponent(String(projectno || '').trim());
    const safeNodeType = encodeURIComponent(normalizedNodeType);

    if (!safeProjectNo || !safeNodeType || !nodeCandidates.length) {
        return $.Deferred().resolve([]).promise();
    }

    const tryFetch = function (index) {
        const safeNodeId = encodeURIComponent(nodeCandidates[index]);

        return $.ajax({
            url: getApiRootPath() + `/api/projects/${safeProjectNo}/data/${safeNodeType}/${safeNodeId}`,
            type: 'GET',
            contentType: 'application/json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            dataType: 'json'
        }).then(resp => {
            const data = (resp && Array.isArray(resp.data)) ? resp.data : [];
            if (!data.length && index + 1 < nodeCandidates.length) {
                return tryFetch(index + 1);
            }

            return data;
        });
    };

    return tryFetch(0);
}