function fetchProductGroupInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/productGroups/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#productGroupCode').text(response.productGroupCode || '-');
            $('#productGroupName').text(response.productGroupName || '-');
            $('#productGroupDescription').text(response.productGroupDescription || '-');
            $('#productGroupStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load product group details.');
        }
    });
}

$(document).ready(function () {
    fetchProductGroupInfo();
});