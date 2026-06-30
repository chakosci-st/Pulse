// Function to fetch a single page of plants
function fetchPageInfo() {
    return $.ajax({
        "url": getApiRootPath() + "/api/modules/GetById?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Code').val(response.moduleCode);
            $('#Name').val(response.moduleName);
            $('#Description').val(response.moduleDescription);
            $('#TransactionKey').val(response.transactionKey); 

            // Update checked state programmatically
            $('#isActive').statusSwitch('setChecked', response.isActive);
        },
        error: function (xhr, status, error) {

        }
    });

}

$(document).ready(function () {
 

    // Initialize with custom options
    $('#isActive').statusSwitch({
        checked: true,
        onText: 'Active',
        offText: 'Inactive',
        size: 'lg'
    });

 
    fetchPageInfo();


    // Initialize jQuery Validate
    $('#moduleForm').validate({
        // Add your validation rules if needed
        rules: { 
            Name: { required: true }
        },
        messages: { 
            Name: { required: "" }
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
            var module = {
                ModuleCode: $('#Code').val(),
                ModuleName: $('#Name').val(),
                ModuleDescription: $('#Description').val(),
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#isActive').statusSwitch('getChecked')  
            };
            formData.append("module", JSON.stringify(module));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/modules/' + module.ModuleCode,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Module is successfully updated!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Module code does not exist!');

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