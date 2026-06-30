// Function to fetch a single page of productDivisions
function fetchPageInfo(id) {

    return $.ajax({
        "url": getApiRootPath() + "/api/roadmaps/full?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {

 
            $('#Name').val(response.roadmapName);
            $('#Description').val(response.roadmapDescription);
            $('#TransactionKey').val(response.transactionKey);

            $('#Categories').val(response.categoryCode).trigger('change');
            const data = JSON.parse(response.roadmapJson);
            const isCopyMode = $('#IsRoadmapCopyMode').val() === '1';
            // Keep the source roadmap identity so import logic does not regenerate
            // existing node keys/ids during edit mode.
            if (!isCopyMode && !data.roadmapsysid && response.roadmapSysId) {
                data.roadmapsysid = response.roadmapSysId;
            }

            if (isCopyMode) {
                // Copy must always clone nodes with new GUIDs.
                importRoadmapJson(data, { forceRegenerateGuids: true });
            } else {
                importRoadmapJson(data);
            }


            ////window.formFields = JSON.parse(response.formJson).fields;
            ////renderFieldsList();
            ////renderPreviewForm();
            ////renderFormStructure();

            $('#labelName').text(response.roadmapName);
            $('#labelDescription').text(response.roadmapDescription);
            $('#buttonIsActive').text(response.isActive ? 'Deactivate' : 'Activate');
            $('#buttonIsActive').attr('data-action', response.isActive ? 'deactivate' : 'activate');  
            $('#buttonIsActive').removeClass('btn-outline-success');
            $('#buttonIsActive').removeClass('btn-outline-secondary');
            $('#buttonIsActive').addClass(`btn-outline-${response.isActive ? 'secondary' : 'success'}`);


            // Update checked state programmatically
            //$('#isActive').statusSwitch('setChecked', response.isActive);
        },
        error: function (xhr, status, error) {

        }
    });

}



$(document).ready(function () {
    fetchPageInfo($('#RoadmapSysId').val());
});