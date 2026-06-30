$(document).ready(function () {

    // Initialize jQuery Validate
    $('#formBasicInfo').validate({
        // Add your validation rules if needed
        rules: {
            Name: { required: true },
            Categories: { required: true },
        },
        messages: {
            Name: { required: "" },
            Categories: { required: "Please select a category." }
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
        submitHandler: function (form) {
            // This function runs only if the form is valid


            // Prepare FormData
            var roadmapData = new FormData();
            // Collect productDivision data (example)
            var submitform = {
                roadmapSysId: $('#RoadmapSysId').val(),
                transactionKey: $('#TransactionKey').val(),
                roadmapName: $('#Name').val(),
                roadmapDescription: $('#Description').val(),
                categoryCode: $('#Categories').val()
            };

            roadmapData.append("roadmap", JSON.stringify(submitform));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/roadmaps/' + $('#RoadmapSysId').val(),
                type: 'PUT',
                data: roadmapData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Roadmap is updated successfully!');
                    fetchPageInfo($('#RoadmapSysId').val());
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Roadmap does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                            toastr.error(`Information already exist! ${xhr.responseText}`);
                        } else {
                            toastr.error('Error: ' + xhr.responseText);
                        }

                    }
                },

            });

            // Prevent default form submission
            return false;
        }
    });

    $('#formRoadmap').submit(function () {
        // Prepare FormData



        var formData = new FormData(); 
        const jsonText = buildRoadmapJson(treeData, rootForms);
        // Collect productDivision data (example)
        var submitform = {
            roadmapSysId: $('#RoadmapSysId').val(),
            transactionKey: $('#TransactionKey').val(), 
            roadmapJson: jsonText
        };

        formData.append("roadmap", JSON.stringify(submitform));

        // AJAX call
        $.ajax({
            url: getApiRootPath() + '/api/roadmaps/updatedetails/' + $('#RoadmapSysId').val(),
            type: 'PUT',
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {

                toastr.success('Roadmap is successfully rebuilt!');

                fetchPageInfo($('#RoadmapSysId').val());
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    toastr.error('Form does not exist!');

                } else {
                    if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                        toastr.error('Form already exist!');
                    } else {
                        alert('Error: ' + xhr.responseText);
                    }

                }
            },

        });

        // Prevent default form submission
        return false;
    });
 
    $('#formChangeStatusRoadmap').submit(function () {
        var formData = new FormData();
        
        var submitform = {
            RoadmapSysId: $('#RoadmapSysId').val(),
            TransactionKey: $('#TransactionKey').val(), 
            IsActive: $('#buttonIsActive').data('action') == "deactivate" ? 0 : 1
        };

        formData.append("roadmap", JSON.stringify(submitform));

        // AJAX call
        $.ajax({
            url: getApiRootPath() + '/api/roadmaps/ChangeStatus/' + $('#RoadmapSysId').val(),
            type: 'PUT',
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {
                if ($('#buttonIsActive').text() == 'Deactivate')
                    toastr.success('Roadmap is successfully deactivated!');
                else
                    toastr.success('Roadmap is successfully activated!');


                $('#buttonIsActive').attr('data-action', $('#buttonIsActive').text() == 'Activate' ? 'deactivate' : 'activate');
                $('#buttonIsActive').removeClass('btn-outline-success');
                $('#buttonIsActive').removeClass('btn-outline-secondary');

                var css = $('#buttonIsActive').text() == 'Activate' ? 'secondary' : 'success'

                $('#buttonIsActive').addClass(`btn-outline-${css}`);
                $('#buttonIsActive').text($('#buttonIsActive').text() == 'Activate' ? 'Deactivate' : 'Activate');
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
    });

    $('#formDeleteRoadmap').submit(function () {

        bootbox.confirm({
            title: "Confirm Deletion",
            message: "Are you sure you want to delete this Roadmap?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    // Prepare FormData
                    var formData = new FormData(); 

                    var submitform = {
                        RoadmapSysId: $('#RoadmapSysId').val(),
                        TransactionKey: $('#TransactionKey').val()
                    };

                    formData.append("roadmap", JSON.stringify(submitform));

                    // AJAX call
                    $.ajax({
                        url: getApiRootPath() + '/api/roadmaps/' + $('#RoadmapSysId').val(),
                        type: 'DELETE',
                        data: formData,
                        processData: false,
                        contentType: false,
                        // xhrFields: { withCredentials: true }, //** REMOVED**
                        success: function () {

                            bootbox.alert({
                                title: "Roadmap Deleted",
                                message: "Roadmap is successfully deleted!",
                                callback: function () {
                                    window.location.href = "/templates/Roadmaps";
                                }
                            });
                             


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
                }  
            }
        });





        // Prevent default form submission
        return false;
    });
});