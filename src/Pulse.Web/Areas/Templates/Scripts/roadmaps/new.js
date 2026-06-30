$(document).ready(function () {
    // Initialize jQuery Validate
    $('#formRoadmap').validate({
        // Add your validation rules if needed
        
        rules: {
            Name: { required: true },
            categories: { required: true },
        },
        messages: {
            Name: { required: "" },
            categories: { required: "Please select a category." }
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $('[id*=-error]').hide()
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();

            $(element).next('.select2').find('.select2-selection').removeClass('is-valid').addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
            $(element).next('.select2').find('.select2-selection').removeClass('is-invalid').addClass('is-valid');
        },
        "initComplete": function () {

            $('#tableRoadmaps_info').html(''); 

            //$('#roadmapsTable_buttons').appendTo('.card-tools-roadmapsTable-buttons');
            //datatableMyTasks.buttons().container().appendTo('.card-tools-roadmapsTable-buttons');

        },
        submitHandler: function (form) {
            // This function runs only if the form is valid

            const jsonText = buildRoadmapJson(treeData, rootForms);


            // Prepare FormData
            var formData = new FormData();
            // Collect productDivision data (example)
            var submitform = {
                roadmapName: $('#Name').val(),
                roadmapDescription: $('#Description').val(),
                categoryCode: $('#Categories').val(),
                roadmapJson: jsonText,
                isActive: 1
            };

            formData.append("roadmap", JSON.stringify(submitform));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/roadmaps',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Roadmap created successfully!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Roadmap does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                            toastr.error('Roadmap already exist!');
                        } else {
                            alert('Error: ' + xhr.responseText);
                        }

                    }
                },

            });

            // Prevent default form submission
            return false;
        }
    });

});