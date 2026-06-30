$(document).ready(function () {
    FieldsDesigner.ensureFieldSysId();
    FieldsDesigner.bindDesignerEvents();
    FieldsDesigner.applyTypeVisibility($("#fieldType").val());
    FieldsDesigner.refreshDesignerPreview();

    $("#buttonClear").on("click", function () {
        if (!confirm("Clear all field settings?")) {
            return;
        }

        $("#FieldSysId").val("");
        FieldsDesigner.ensureFieldSysId();
        FieldsDesigner.resetDesigner();
        $("#DesignerSummary").val("Configure field details below");
    });

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
            var payload = FieldsDesigner.buildApiPayload();

            if (!payload.title) {
                toastr.error("Field label is required.");
                return false;
            }

            var formData = new FormData();
            formData.append("field", JSON.stringify(payload));

            $.ajax({
                url: getApiRootPath() + "/api/fields",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    toastr.success("Field created successfully.");
                    $("#FieldSysId").val("");
                    FieldsDesigner.ensureFieldSysId();
                    FieldsDesigner.resetDesigner();
                    $("#DesignerSummary").val("Configure field details below");
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error("Field endpoint not found.");
                        return;
                    }

                    if (xhr.responseText && xhr.responseText.indexOf("ORA-00001") >= 0) {
                        toastr.error("Field code already exists.");
                        return;
                    }

                    toastr.error("Unable to create field.");
                }
            });

            return false;
        }
    });
});
