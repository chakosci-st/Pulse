function fetchAttachmentsAsync(projectno, entitytype, entityid) {
    return $.ajax({
        url: getApiRootPath() + `/api/project/${projectno}/attachments/${entitytype}/${entityid}`,
        type: 'GET',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json'
    }).then(resp => resp.data);
}