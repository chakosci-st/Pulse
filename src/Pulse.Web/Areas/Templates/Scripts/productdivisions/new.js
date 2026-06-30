function ClearField() {
    $('input').val('');
}

$(document).ready(function () {

 
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
            // This function runs only if the form is valid

            // Prepare FormData
            var formData = new FormData();
 
            // Collect productDivision data (example)
            var productDivision = {
                ProductDivisionCode: $('#Code').val(),
                ProductDivisionName: $('#Name').val(),
                ProductDivisionDescription: $('#Description').val(),
                IsActive: 1
            };

            formData.append("productdivision", JSON.stringify(productDivision));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/productDivisions',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Product Division created successfully!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Product Division code does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint (NPITRACK.PRODUCTDIVISIONS_PK) violated') > 0) {
                            toastr.error('Product Division code already exist!');
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