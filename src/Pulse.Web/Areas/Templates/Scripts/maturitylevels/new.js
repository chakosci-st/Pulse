function ClearField() {
    $('input').val('');
}

$(document).ready(function () {

 
    // Initialize jQuery Validate
    $('#maturitylevelForm').validate({
        // Add your validation rules if needed
        rules: {
            Code: { required: true },
            MaturityNumber: { required: true },
            SequenceNo: { required: true }
        },
        messages: {
            Code: { required: "" },
            MaturityNumber: { required: "" },
            SequenceNo: { required: "" }
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
 
            // Collect maturityLevel data (example)
            var maturityLevel = {
                MaturityCode: $('#Code').val(),
                MaturityNumber: $('#MaturityNumber').val(),
                SequenceNo: $('#SequenceNo').val(),
                IsActive: 1
            };
            formData.append("maturitylevel", JSON.stringify(maturityLevel));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/maturityLevels',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Maturity Level created successfully!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Maturity Level code does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint (NPITRACK.PLANTS_PK) violated') > 0) {
                            toastr.error('MaturityLevel code already exist!');
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