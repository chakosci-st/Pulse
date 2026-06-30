function renderDisplayPreview(field) {
    var previewField = FieldsDesigner.renderFieldPreviewInModes(field, "#previewWriteForm", "#previewReadForm");
    $("#fieldStructurePreview").text(JSON.stringify(previewField, null, 2));
}

function fetchFieldInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/fields/" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            var field = FieldsDesigner.normalizeApiField(response || {});

            $("#fieldCode").text(field.id || "-");
            $("#fieldTitleDisplay").text(field.title || "-");
            $("#fieldNameDisplay").text(field.name || "-");
            $("#fieldTypeDisplay").text(field.type || "-");
            $("#fieldStatus").html(field.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');

            renderDisplayPreview(field);
        },
        error: function () {
            toastr.error("Unable to load field details.");
        }
    });
}

$(document).ready(function () {
    fetchFieldInfo();
});
