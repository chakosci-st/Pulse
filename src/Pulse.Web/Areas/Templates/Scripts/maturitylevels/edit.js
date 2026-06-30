// Function to fetch a single page of maturityLevels
function fetchPageInfo() {
    return $.ajax({
        "url": getApiRootPath() + "/api/maturityLevels/GetById?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Code').val(response.maturityCode);
            $('#MaturityNumber').val(response.maturityNumber);
            $('#SequenceNo').val(response.sequenceNo);
            $('#TransactionKey').val(response.transactionKey); 

            // Update checked state programmatically
            $('#isActive').statusSwitch('setChecked', response.isActive);
        },
        error: function (xhr, status, error) {

        }
    });

}

$(document).ready(function () {
    console.log(id)

    // Initialize with custom options
    $('#isActive').statusSwitch({
        checked: true,
        onText: 'Active',
        offText: 'Inactive',
        size: 'lg'
    });
     


    fetchPageInfo();


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
            SequenceNo: { required: true }
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
 
            // Collect data 
            var maturityLevel = {
                MaturityCode: $('#Code').val(),
                MaturityNumber: $('#MaturityNumber').val(),
                SequenceNo: $('#SequenceNo').val(),
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#isActive').statusSwitch('getChecked')  
            };
            formData.append("maturitylevel", JSON.stringify(maturityLevel));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/maturityLevels/' + maturityLevel.MaturityCode,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Maturity Level is successfully updated!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Maturity Level code does not exist!');

                    } else {
                        toastr.error('Error: ' + xhr.responseText);
                    }
                }
            });

            // Prevent default form submission
            return false;
        }
    });

});