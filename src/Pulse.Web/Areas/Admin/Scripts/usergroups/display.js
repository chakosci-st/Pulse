function fetchUserGroupInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/usergroups/" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#userGroupId').text(response.userGroupId || '-');
            $('#userGroupName').text(response.userGroupName || '-');
            $('#userGroupDescription').text(response.userGroupDescription || '-');
            $('#userGroupStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load user group details.');
        }
    });
}

$(document).ready(function () {
    fetchUserGroupInfo();
});