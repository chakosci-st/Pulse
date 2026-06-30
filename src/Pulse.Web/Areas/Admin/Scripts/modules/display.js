function fetchModuleInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/modules/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#moduleCode').text(response.moduleCode || '-');
            $('#moduleName').text(response.moduleName || '-');
            $('#moduleDescription').text(response.moduleDescription || '-');
            $('#moduleCreatedDate').text(response.createdDate ? moment(response.createdDate).format('MMM DD, YYYY HH:mm') : '-');
            $('#moduleModifiedDate').text(response.modifiedDate ? moment(response.modifiedDate).format('MMM DD, YYYY HH:mm') : '-');
            $('#moduleStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load module details.');
        }
    });
}

$(document).ready(function () {
    fetchModuleInfo();
});