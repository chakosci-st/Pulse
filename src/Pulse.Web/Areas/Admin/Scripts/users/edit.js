function fetchUserInfo() {
    return $.ajax({
        url: getApiRootPath() + '/api/users/' + id,
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            $('#Id').val(response.userId);
            $('#UserId').val(response.userId);
            $('#UserName').val(response.userName);
            $('#FirstName').val(response.firstName);
            $('#LastName').val(response.lastName);
            $('#Email').val(response.email);
            $('#TransactionKey').val(response.transactionKey);
            $('#isActive').statusSwitch('setChecked', response.isActive);
        }
    });
}

$(document).ready(function () {
    $('#isActive').statusSwitch({
        checked: true,
        onText: 'Active',
        offText: 'Inactive',
        size: 'lg'
    });

    fetchUserInfo();

    $('#userForm').validate({
        rules: {
            UserName: { required: true, minlength: 4 },
            FirstName: { required: true },
            LastName: { required: true },
            Email: { email: true }
        },
        messages: {
            UserName: { required: '' },
            FirstName: { required: '' },
            LastName: { required: '' }
        },
        errorElement: 'span',
        errorPlacement: function () {
            $('[id*=-error]').hide();
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function () {
            var formData = new FormData();
            var user = {
                UserId: $('#Id').val(),
                UserName: $('#UserName').val(),
                FirstName: $('#FirstName').val(),
                LastName: $('#LastName').val(),
                Email: $('#Email').val(),
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#isActive').statusSwitch('getChecked')
            };
            formData.append('user', JSON.stringify(user));

            $.ajax({
                url: getApiRootPath() + '/api/users/' + user.UserId,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    toastr.success('User successfully updated!');
                },
                error: function () {
                    toastr.error('Unable to update user.');
                }
            });

            return false;
        }
    });
});
