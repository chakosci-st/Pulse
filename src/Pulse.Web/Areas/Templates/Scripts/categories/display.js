function fetchCategoryInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/categories/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#categoryCode').text(response.categoryCode || '-');
            $('#categoryName').text(response.categoryName || '-');
            $('#categoryDescription').text(response.categoryDescription || '-');
            $('#categoryStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load category details.');
        }
    });
}

$(document).ready(function () {
    fetchCategoryInfo();
});