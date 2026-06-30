function getRoadmapPreviewText(rawJson) {
    if (!rawJson) {
        return '{}';
    }

    try {
        return JSON.stringify(JSON.parse(rawJson), null, 2);
    } catch (error) {
        return rawJson;
    }
}

function fetchRoadmapInfo() {
    return $.ajax({
        url: getApiRootPath() + "/api/roadmaps/full?code=" + id,
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            $('#roadmapSysId').text(id || '-');
            $('#roadmapName').text(response.roadmapName || '-');
            $('#roadmapDescription').text(response.roadmapDescription || '-');
            $('#roadmapCategory').text(response.categoryCode || '-');
            $('#roadmapStatus').html(response.isActive
                ? '<span class="badge text-bg-success">Active</span>'
                : '<span class="badge text-bg-danger">Inactive</span>');
            $('#roadmapStructurePreview').text(getRoadmapPreviewText(response.roadmapJson));
        },
        error: function () {
            toastr.error('Unable to load roadmap details.');
        }
    });
}

$(document).ready(function () {
    fetchRoadmapInfo();
});