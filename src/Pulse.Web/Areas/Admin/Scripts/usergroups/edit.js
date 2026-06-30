// Function to fetch a single page of plants
function fetchPageInfo() {
    return $.ajax({
        "url": getApiRootPath() + "/api/usergroups/" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Id').val(response.userGroupId);
            $('#Name').val(response.userGroupName); 
            $('#Description').val(response.userGroupDescription); 
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
             
            // Collect data
            var usergroup = {
                UserGroupId: $('#Id').val(),
                UserGroupName: $('#Name').val(),
                UserGroupDescription: $('#Description').val(),
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#isActive').statusSwitch('getChecked')  
            };
            formData.append("usergroup", JSON.stringify(usergroup));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/usergroups/' + usergroup.UserGroupId,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('User Group is successfully updated!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('User Group code does not exist!');

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