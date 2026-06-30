function clearFields() {
    $('#DirectorySearch').val(null).trigger('change');
    $('#UserId').val('');
    $('#UserName').val('');
    $('#FirstName').val('');
    $('#LastName').val('');
    $('#Email').val('');
}

$(document).ready(function () {
    $('#DirectorySearch').select2({
        ajax: {
            url: getApiRootPath() + '/api/ActiveDirectory/Search',
            type: 'GET',
            data: function (params) {
                return { key: params.term };
            },
            delay: 250,
            processResults: function (data) {
                var formatted = $.map(data.data || [], function (obj) {
                    obj.text = (obj.firstName || '') + ' ' + (obj.lastName || '');
                    obj.id = obj.userId;
                    return obj;
                });

                return { results: formatted };
            }
        },
        cache: true,
        placeholder: 'Search by first name, last name, username, or email',
        allowClear: true,
        minimumInputLength: 3,
        templateSelection: function (data) {
            if (data && data.userId) {
                $('#UserId').val(data.userId || '');
                $('#UserName').val(data.userName || '');
                $('#FirstName').val(data.firstName || '');
                $('#LastName').val(data.lastName || '');
                $('#Email').val(data.email || '');
                return (data.firstName || '') + ' ' + (data.lastName || '');
            }

            return data && data.text ? data.text : '';
        }
    });

    $('#userForm').validate({
        rules: {
            UserId: { required: true },
            UserName: { required: true, minlength: 4 },
            FirstName: { required: true },
            LastName: { required: true },
            Email: { email: true }
        },
        messages: {
            UserId: { required: '' },
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
                UserId: $('#UserId').val(),
                UserName: $('#UserName').val(),
                FirstName: $('#FirstName').val(),
                LastName: $('#LastName').val(),
                Email: $('#Email').val(),
                IsActive: true
            };

            formData.append('user', JSON.stringify(user));

            $.ajax({
                url: getApiRootPath() + '/api/users',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    toastr.success('User created successfully!');
                    clearFields();
                },
                error: function (xhr) {
                    if (xhr.responseText && xhr.responseText.indexOf('ORA-00001') > -1) {
                        toastr.error('User already exists.');
                    } else {
                        toastr.error('Unable to create user.');
                    }
                }
            });

            return false;
        }
    });
});
