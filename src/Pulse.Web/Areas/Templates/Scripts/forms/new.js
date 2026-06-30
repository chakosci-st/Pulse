window.currentUserCode = "*";

function ClearField() {
    $('input').val('');
    $('textarea').val('');

    window.formFields = [];
    if (typeof renderFieldsList === 'function') renderFieldsList();
    if (typeof renderPreviewForm === 'function') renderPreviewForm();
    if (typeof renderFormStructure === 'function') renderFormStructure();
    $('#addFieldForm')[0].reset();
    $('#fieldType').trigger('change');
    $('#editFieldIdx').val('');
    $('#addFieldBtn').show();
    $('#updateFieldBtn, #cancelEditBtn').hide();
    if (window.RulesBuilder) $('#rulesList').empty();
}




$(document).ready(function () {


    // Initialize jQuery Validate
    $('#formForm').validate({
        // Add your validation rules if needed
        rules: {
            Name: { required: true },
            FormJson: { required: true },
        },
        messages: {
            Name: { required: "" },
            FormJson: { required: "" },
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $('[id*=-error]').hide()
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function (form) {
            // This function runs only if the form is valid

            // Prepare FormData
            var formData = new FormData();
            var jsonText = document.getElementById('formStructure').textContent;
            // Collect productDivision data (example)
            var submitform = {
                FormSysId: $('#FormSysId').val(),
                FormName: $('#Name').val(),
                FormDescription: $('#Description').val(),
                FormJson: jsonText, 
                IsActive: 1
            }; 

            formData.append("form", JSON.stringify(submitform));
 
            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/forms',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Form created successfully!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Form does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                            toastr.error('Form already exist!');
                        } else {
                            alert('Error: ' + xhr.responseText);
                        }

                    }
                },

            });

            // Prevent default form submission
            return false;
        }
    });

});