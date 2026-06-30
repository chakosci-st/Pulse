function normalizeIsActive(value) {
    if (value === true || value === 1) return true;
    if (typeof value === 'string') {
        var normalized = value.toLowerCase();
        return normalized === 'true' || normalized === '1';
    }

    return value === undefined || value === null;
}

window.reactivatedFieldAudit = window.reactivatedFieldAudit || {};

function renderFieldsAccessTable(fields) {
    var $tableBody = $('#tableFieldsAccess tbody');
    if (!$tableBody.length) return;

    $tableBody.empty();

    if (!Array.isArray(fields) || fields.length === 0) {
        $tableBody.append('<tr><td colspan="4" class="text-muted">No fields found.</td></tr>');
        return;
    }

    fields.forEach(function (field) {
        var isActive = normalizeIsActive(field.isActive);
        var buttonClass = isActive ? 'btn-outline-success' : 'btn-outline-secondary';
        var buttonText = isActive ? 'Active' : 'Inactive';
        var readAccess = field.readAccess || '-';
        var writeAccess = field.writeAccess || '-';
        var fieldId = field.id || '';
        var disabled = fieldId ? '' : 'disabled';
        var nextState = isActive ? 'false' : 'true';
        var isReactivated = fieldId && window.reactivatedFieldAudit[fieldId] === true;
        var titleCell = (field.title || field.name || '-') + (isReactivated ? ' <span class="badge rounded-pill bg-success-subtle text-success-emphasis ms-2">Reactivated</span>' : '');

        $tableBody.append(
            '<tr>' +
                '<td>' + titleCell + '</td>' +
                '<td>' + readAccess + '</td>' +
                '<td>' + writeAccess + '</td>' +
                '<td>' +
                    '<button type="button" class="btn btn-sm rounded-pill ' + buttonClass + ' js-field-status-toggle" data-field-id="' + fieldId + '" data-current-state="' + (isActive ? 'true' : 'false') + '" data-next-state="' + nextState + '" ' + disabled + '>' +
                        buttonText +
                    '</button>' +
                '</td>' +
            '</tr>'
        );
    });
}

// Function to fetch a single page of productDivisions
function fetchPageInfo(id) {


    return $.ajax({
        "url": getApiRootPath() + "/api/forms/GetById?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Name').val(response.formName);
            $('#Description').val(response.formDescription);
            $('#TransactionKey').val(response.transactionKey);

            var parsedFormJson = {};
            try {
                parsedFormJson = JSON.parse(response.formJson || '{}');
            } catch (e) {
                parsedFormJson = {};
            }

            var allFields = Array.isArray(parsedFormJson.fields) ? parsedFormJson.fields : [];
            window.formFieldsAccess = allFields;
            window.formFields = allFields.filter(function (field) {
                return normalizeIsActive(field.isActive);
            }).map(function (field) {
                var fieldId = field.id || '';
                return {
                    ...field,
                    wasReactivated: fieldId ? window.reactivatedFieldAudit[fieldId] === true : false
                };
            });

            renderFieldsList();
            renderPreviewForm();
            renderFormStructure();
            renderFieldsAccessTable(window.formFieldsAccess);

            $('#labelFormName').text(response.formName);
            $('#labelFormDescription').text(response.formDescription);
            $('#buttonIsActive').text(response.isActive ? 'Deactivate' : 'Activate');
            $('#buttonIsActive').attr('data-action', response.isActive ? 'deactivate' : 'activate');  
            $('#buttonIsActive').removeClass('btn-outline-success');
            $('#buttonIsActive').removeClass('btn-outline-secondary');
            $('#buttonIsActive').addClass(`btn-outline-${response.isActive ? 'secondary' : 'success'}`);


            // Update checked state programmatically
            //$('#isActive').statusSwitch('setChecked', response.isActive);
        },
        error: function (xhr, status, error) {

        }
    });

}



$(document).ready(function () {
    fetchPageInfo($('#FormSysId').val());
});