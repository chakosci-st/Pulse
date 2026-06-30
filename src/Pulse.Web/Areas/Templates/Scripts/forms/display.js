function escapeHtml(value) {
    return $('<div>').text(value == null ? '' : value).html();
}

function renderFormFields(fields) {
    const list = $('#formFieldList');

    if (!Array.isArray(fields) || !fields.length) {
        list.html('<li class="list-group-item text-muted">No form fields configured.</li>');
        $('#formFieldCount').text('0');
        return;
    }

    $('#formFieldCount').text(fields.length);
    list.html(fields.map(function (field, index) {
        const label = field.label || field.name || field.type || ('Field ' + (index + 1));
        const type = field.type || 'unknown';
        return '<li class="list-group-item d-flex justify-content-between align-items-center"><span>' + escapeHtml(label) + '</span><span class="badge text-bg-light">' + escapeHtml(type) + '</span></li>';
    }).join(''));
}

function fetchFormInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/forms/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            let parsed = {};

            try {
                parsed = response.formJson ? JSON.parse(response.formJson) : {};
            } catch (error) {
                parsed = {};
            }

            $('#formSysId').text(id || '-');
            $('#formName').text(response.formName || '-');
            $('#formDescription').text(response.formDescription || '-');
            $('#formStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
            $('#formStructurePreview').text(response.formJson || '{}');

            renderFormFields(parsed.fields || []);
        },
        error: function () {
            toastr.error('Unable to load form details.');
        }
    });
}

$(document).ready(function () {
    fetchFormInfo();
});