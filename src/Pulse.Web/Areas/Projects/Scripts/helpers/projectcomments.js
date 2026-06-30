function fetchCommentsAsync(projectno) {
    return $.ajax({
        url: getApiRootPath() + `/api/comments/${projectno}`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}


function fetchCommentsPerEntityAsync(projectno, entitytype, entitysysid) {
    return $.ajax({
        url: getApiRootPath() + `/api/comments/${projectno}/${entitytype}/${entitysysid}`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}
