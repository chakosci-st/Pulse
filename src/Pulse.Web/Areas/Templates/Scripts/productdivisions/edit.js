function ClearField() {
    $('input').val('');
}

// Function to fetch a single page of productDivisions
function fetchPageInfo() {
    return $.ajax({
        "url": getApiRootPath() + "/api/productdivisions/GetById?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Code').val(response.productDivisionCode);
            $('#Name').val(response.productDivisionName);
            $('#Description').val(response.productDivisionDescription); 
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
    $('#productdivisionForm').validate({
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
             
            const code = $('#Code').val();
            var productDivision = {
                productDivisionCode: code,
                productDivisionName: $('#Name').val(),
                productDivisionDescription: $('#Description').val(),
                transactionKey: $('#TransactionKey').val(),
                isActive: $('#isActive').statusSwitch('getChecked')
            };
 

            $.ajax({
                url: getApiRootPath() + '/api/productdivisions/' + encodeURIComponent(code),
                type: "PUT",
                data: JSON.stringify(productDivision),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function () {

                    toastr.success('Product Division is successfully updated!')
                    ClearField();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Update failed:", textStatus, errorThrown);
                    console.error("Status:", jqXHR.status);
                    console.error("Response:", jqXHR.responseText);
                }
            });
             
            // Prevent default form submission
            return false;
        }
    });

});