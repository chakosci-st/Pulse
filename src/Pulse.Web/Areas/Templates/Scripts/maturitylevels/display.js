function fetchMaturityLevelInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/maturityLevels/GetById?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#maturityCode').text(response.maturityCode || '-');
            $('#maturityNumber').text(response.maturityNumber != null ? response.maturityNumber : '-');
            $('#sequenceNo').text(response.sequenceNo != null ? response.sequenceNo : '-');
            $('#maturityStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
        },
        error: function () {
            toastr.error('Unable to load maturity level details.');
        }
    });
}

$(document).ready(function () {
    fetchMaturityLevelInfo();
});