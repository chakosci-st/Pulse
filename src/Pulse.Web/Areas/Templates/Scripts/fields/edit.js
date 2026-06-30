var currentField = null;

function syncStatusButton(isActive) {
    var shouldActivate = !isActive;
    var $button = $("#buttonIsActive");

    $button.attr("data-action", shouldActivate ? "activate" : "deactivate");
    $button.toggleClass("btn-outline-success", shouldActivate);
    $button.toggleClass("btn-outline-secondary", !shouldActivate);
    $button.text(shouldActivate ? "Activate" : "Deactivate");
}

function updateFieldStatus(isActive, onSuccess) {
    var payload = FieldsDesigner.buildApiPayload(currentField || {});
    payload.id = currentField && currentField.id ? currentField.id : id;
    payload.transactionKey = $("#TransactionKey").val() || payload.transactionKey || null;
    payload.isActive = !!isActive;

    var formData = new FormData();
    formData.append("field", JSON.stringify(payload));

    $.ajax({
        url: getApiRootPath() + "/api/fields/" + payload.id,
        type: "PUT",
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
            if (typeof onSuccess === "function") {
                onSuccess(payload.isActive);
            }
            fetchFieldInfo();
        },
        error: function (xhr) {
            if (xhr.status === 404) {
                toastr.error("Field does not exist.");
                return;
            }

            toastr.error("Unable to update field status.");
        }
    });
}

function fetchFieldInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/fields/" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            currentField = FieldsDesigner.normalizeApiField(response || {});

            $("#FieldSysId").val(currentField.id || id);
            $("#TransactionKey").val(currentField.transactionKey || "");
            FieldsDesigner.setDesignerField(currentField);
            syncStatusButton(!!currentField.isActive);
        },
        error: function () {
            toastr.error("Unable to load field.");
        }
    });
}

$(document).ready(function () {
    FieldsDesigner.bindDesignerEvents();
    FieldsDesigner.applyTypeVisibility($("#fieldType").val());

    fetchFieldInfo();

    $("#fieldForm").validate({
        rules: {
            FieldSysId: { required: true },
            FieldJson: { required: true }
        },
        messages: {
            FieldSysId: { required: "" },
            FieldJson: { required: "" }
        },
        errorElement: "span",
        errorPlacement: function () {
            $("[id*=-error]").hide();
        },
        highlight: function (element) {
            $(element).addClass("is-invalid").removeClass("is-valid");
            $(element).next(".invalid-feedback").show();
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid").addClass("is-valid");
            $(element).next(".invalid-feedback").hide();
        },
        submitHandler: function () {
            var payload = FieldsDesigner.buildApiPayload(currentField || {});
            payload.id = currentField && currentField.id ? currentField.id : id;
            payload.isActive = currentField ? !!currentField.isActive : true;

            if (!payload.title) {
                toastr.error("Field label is required.");
                return false;
            }

            var formData = new FormData();
            formData.append("field", JSON.stringify(payload));

            $.ajax({
                url: getApiRootPath() + "/api/fields/" + payload.id,
                type: "PUT",
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    toastr.success("Field updated successfully.");
                    fetchFieldInfo();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error("Field does not exist.");
                        return;
                    }

                    toastr.error("Unable to update field.");
                }
            });

            return false;
        }
    });

    $("#changestatusForm").submit(function () {
        var nextIsActive = $("#buttonIsActive").data("action") === "activate";

        updateFieldStatus(nextIsActive, function (isActive) {
            syncStatusButton(isActive);
            toastr.success(isActive ? "Field is successfully activated!" : "Field is successfully deactivated!");
        });

        return false;
    });

    $("#deleteForm").submit(function () {
        bootbox.confirm({
            title: "Confirm Deletion",
            message: "Are you sure you want to delete this field?",
            buttons: {
                confirm: {
                    label: "Yes",
                    className: "btn-danger"
                },
                cancel: {
                    label: "No",
                    className: "btn-secondary"
                }
            },
            callback: function (result) {
                if (!result) {
                    return;
                }

                $.ajax({
                    url: getApiRootPath() + "/api/fields/" + id,
                    type: "DELETE",
                    success: function () {
                        bootbox.alert({
                            title: "Field Deleted",
                            message: "Field is successfully deleted!",
                            callback: function () {
                                window.location.href = "/templates/fields";
                            }
                        });
                    },
                    error: function (xhr) {
                        if (xhr.status === 404) {
                            toastr.error("Field does not exist!");
                            return;
                        }

                        toastr.error(xhr.responseText || "Unable to delete field.");
                    }
                });
            }
        });

        return false;
    });
});
