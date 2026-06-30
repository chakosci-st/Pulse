function fetchUserInfo() {
    return $.ajax({
        url: getApiRootPath() + '/api/users/' + id,
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            $('#userId').text(response.userId || '-');
            $('#userName').text(response.userName || '-');
            $('#firstName').text(response.firstName || '-');
            $('#lastName').text(response.lastName || '-');
            $('#email').text(response.email || '-');
            $('#userStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load user details.');
        }
    });
}

$(document).ready(function () {
    fetchUserInfo();
});
