// Function to fetch a single page of productGroups
function fetchPageInfo() {
    return $.ajax({
        "url": getApiRootPath() + "/api/productGroups/GetById?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Code').val(response.productGroupCode);
            $('#Name').val(response.productGroupName);
            $('#Description').val(response.productGroupDescription); 
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
    $('#productgroupForm').validate({
        // Add your validation rules if needed
        rules: {
            Code: { required: true },
            Name: { required: true }, 
        },
        messages: {
            Code: { required: "" },
            Name: { required: "" }, 
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
            var productGroup = {
                ProductDivisionCode: $('#Code').val(),
                ProductDivisionName: $('#Name').val(),
                ProductDivisionDescription: $('#Description').val(), 
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#isActive').statusSwitch('getChecked')  
            };
            formData.append("productgroup", JSON.stringify(productGroup));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/productGroups/' + productGroup.ProductDivisionCode,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Product Division is successfully updated!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Product Division code does not exist!');

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