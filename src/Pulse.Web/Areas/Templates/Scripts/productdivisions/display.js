function fetchProductDivisionInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/productdivisions/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#productDivisionCode').text(response.productDivisionCode || '-');
            $('#productDivisionName').text(response.productDivisionName || '-');
            $('#productDivisionDescription').text(response.productDivisionDescription || '-');
            $('#productDivisionStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load product division details.');
        }
    });
}

$(document).ready(function () {
    fetchProductDivisionInfo();
});