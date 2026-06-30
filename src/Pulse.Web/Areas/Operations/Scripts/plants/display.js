function fetchPlantInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/plants/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#plantCode').text(response.plantCode || '-');
            $('#plantName').text(response.plantName || '-');
            $('#plantStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
            $('#plantImage').attr('src', getAppRootPath() + '/Content/uploads/PlantFiles/' + response.plantCode + '.jpg');
        },
        error: function () {
            toastr.error('Unable to load plant details.');
        }
    });
}

$(document).ready(function () {
    fetchPlantInfo();
});