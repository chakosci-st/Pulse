$(document).ready(function () {

    // Initialize jQuery Validate
    $('#basicinfoForm').validate({
        // Add your validation rules if needed
        rules: {
            Name: { required: true },
            FormJson: { required: true },
        },
        messages: {
            Name: { required: "" },
            FormJson: { required: "" },
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $('[id*=-error]').hide()
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function (form) {
            // This function runs only if the form is valid

            // Prepare FormData
            var formData = new FormData(); 
            // Collect productDivision data (example)
            var submitform = {
                FormSysId: $('#FormSysId').val(),
                TransactionKey: $('#TransactionKey').val(),
                FormName: $('#Name').val(),
                FormDescription: $('#Description').val() 
            }; 

            formData.append("form", JSON.stringify(submitform));
 
            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/forms/' + $('#FormSysId').val(),
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Form is updated successfully!');
                    fetchPageInfo($('#FormSysId').val());
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
        }
    });

    $('#formfieldsForm').submit(function () {
        // Prepare FormData
        var formData = new FormData();
        var jsonText = document.getElementById('formStructure').textContent;
        // Collect productDivision data (example)
        var submitform = {
            FormSysId: $('#FormSysId').val(),
            TransactionKey: $('#TransactionKey').val(),
            FormJson: jsonText
        };

        formData.append("form", JSON.stringify(submitform));

        // AJAX call
        $.ajax({
            url: getApiRootPath() + '/api/forms/UpdateFields/' + $('#FormSysId').val(),
            type: 'PUT',
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {

                toastr.success('Form field(s) are updated successfully!');
                fetchPageInfo($('#FormSysId').val());
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

    $(document).on('click', '.js-field-status-toggle', function () {
        var $button = $(this);
        var fieldId = $button.data('field-id');
        var currentState = String($button.data('current-state')).toLowerCase() === 'true';
        var nextState = String($button.data('next-state')).toLowerCase() === 'true';

        if (!fieldId) {
            toastr.error('Field id is missing.');
            return;
        }

        $button.prop('disabled', true);

        $.ajax({
            url: getApiRootPath() + '/api/forms/fields/' + fieldId + '/status',
            type: 'PUT',
            data: JSON.stringify({
                formSysId: $('#FormSysId').val(),
                isActive: nextState
            }),
            contentType: 'application/json',
            success: function () {
                if (!currentState && nextState && window.reactivatedFieldAudit) {
                    window.reactivatedFieldAudit[fieldId] = true;
                }
                toastr.success('Field status updated successfully.');
                fetchPageInfo($('#FormSysId').val());
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    toastr.error('Field or form no longer exists.');
                } else {
                    toastr.error('Unable to update field status.');
                }
            },
            complete: function () {
                $button.prop('disabled', false);
            }
        });
    });
 
    $('#changestatusForm').submit(function () {
        var formData = new FormData();
        // Collect productDivision data (example)
        var submitform = {
            FormSysId: $('#FormSysId').val(),
            TransactionKey: $('#TransactionKey').val(), 
            IsActive: $('#buttonIsActive').data('action') == "deactivate" ? 0 : 1
        };

        formData.append("form", JSON.stringify(submitform));

        // AJAX call
        $.ajax({
            url: getApiRootPath() + '/api/forms/ChangeStatus/' + $('#FormSysId').val(),
            type: 'PUT',
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {
                if ($('#buttonIsActive').text() == 'Deactivate')
                    toastr.success('Form is successfully deactivated!');
                else
                    toastr.success('Form is successfully activated!');


                $('#buttonIsActive').attr('data-action', $('#buttonIsActive').text() == 'Activate' ? 'deactivate' : 'activate');
                $('#buttonIsActive').removeClass('btn-outline-success');
                $('#buttonIsActive').removeClass('btn-outline-secondary');

                var css = $('#buttonIsActive').text() == 'Activate' ? 'secondary' : 'success'

                $('#buttonIsActive').addClass(`btn-outline-${css}`);
                $('#buttonIsActive').text($('#buttonIsActive').text() == 'Activate' ? 'Deactivate' : 'Activate');
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

    $('#deleteForm').submit(function () {

        bootbox.confirm({
            title: "Confirm Deletion",
            message: "Are you sure you want to delete this form?",
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
                    // Collect productDivision data (example)
                    var submitform = {
                        FormSysId: $('#FormSysId').val(),
                        TransactionKey: $('#TransactionKey').val()
                    };

                    formData.append("form", JSON.stringify(submitform));

                    // AJAX call
                    $.ajax({
                        url: getApiRootPath() + '/api/forms/' + $('#FormSysId').val(),
                        type: 'DELETE',
                        data: formData,
                        processData: false,
                        contentType: false,
                        // xhrFields: { withCredentials: true }, //** REMOVED**
                        success: function () {

                            bootbox.alert({
                                title: "Form Deleted",
                                message: "Form is successfully deleted!",
                                callback: function () {
                                    window.location.href = "/templates/forms";
                                }
                            });
                             


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
                }  
            }
        });





        // Prevent default form submission
        return false;
    });
});