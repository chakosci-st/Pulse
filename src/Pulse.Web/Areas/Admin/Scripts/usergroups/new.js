function ClearField() {  
    $('input').val('');  
}

$(document).ready(function () {

 
    // Initialize jQuery Validate
    $('#usergroupForm').validate({
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
 
            // Collect plant data (example)
            var usergroup = { 
                UserGroupName: $('#Name').val(), 
                UserGroupDescription: $('#Description').val(), 
                IsActive: 1
            };
            formData.append("usergroup", JSON.stringify(usergroup));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/usergroups',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('User Group created successfully!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('User group code does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint (NPITRACK.USERGROUPS_PK) violated') > 0) {
                            toastr.error('User group code already exist!');
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