//const apiPath = getApiRootPath()

// Function to fetch a single page of usergroup
function fetchPageInfo() {
    return $.ajax({
        "url": apiPath + "/api/usergroups/" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#Id').val(response.userGroupId);
            $('#Name').val(response.userGroupName);

            $('#labelName').text(response.userGroupName);
            $('#labelDescription').text(response.userGroupDescription);


            $('#Description').val(response.userGroupDescription);
            $('#TransactionKey').val(response.transactionKey);

            // Update checked state programmatically
            //$('#isActive').statusSwitch('setChecked', response.isActive);


            $('#IsActive').val(1);

            if (!response.isActive) {
                let buttonActive = $("#buttonIsActive");
                buttonActive.html("Activate");
                buttonActive.addClass('btn-outline-success');
                buttonActive.removeClass('btn-outline-secondary');
                buttonActive.attr('data-action', 'activate');
                $('#IsActive').val(0);
            }
        },
        error: function (xhr, status, error) {

        }
    });

}

$(document).ready(function () {

    fetchPageInfo();


    // ------------  START: MODULES  ----------------
    var tableModules = $('#tableModules').DataTable({
        "processing": true, "responsive": true,
        "serverSide": false,
        "ajax": {
            "url": apiPath + `/api/usergroups/${id}/modules`,
            "type": "GET",
            "contentType": "application/json"
        },
        "initComplete": function () {
            document.querySelectorAll(".status-switch .form-check-input").forEach(function (input) {
                input.addEventListener("change", function () {
                    const pill = input.closest("td").querySelector(".status-pill-text");
                    const isOn = input.checked;

                    if (isOn) {
                        pill.textContent = "Allowed";
                        pill.classList.remove("status-pill-off");
                        pill.classList.add("status-pill-on");
                    } else {
                        pill.textContent = "Restricted";
                        pill.classList.remove("status-pill-on");
                        pill.classList.add("status-pill-off");
                    }
                });
            });


        },
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        columnDefs: [

            { targets: [1, 2], className: 'text-center', orderable: false },
        ],
        "language": {
            "search": "",
            "searchPlaceholder": "Search...",
            "emptyTable": "No data found.",
            'processing': '<div>Loading data please wait...  </div>',
            paginate: {
                previous: "«",
                next: "»"
            }
        },
        "columns": [
            {
                "data": "moduleCode"
            },
            {
                "data": "moduleName"
            },
            {
                "data": "moduleDescription"
            },
            {
                "data": "isSelected",
                "className": "text-center",
                "render": function (value, type, data) {



                    return `
<div class="d-flex flex-column align-items-center gap-2" id="moduleselected${data.moduleCode}">
    <label class="float-end form-check-label mb-0">
        <span class="status-pill ${value == 1 ? 'status-pill-on' : 'status-pill-off'} status-pill-text">${value == 1 ? 'Allowed' : 'Restricted'}</span>
    </label>
    <div class="form-check form-switch status-switch m-0">
        <input class="form-check-input dt-selected-checkbox" data-module-code="${data.moduleCode}"  type="checkbox" id="isAllowed${data.moduleCode}"  ${value == 1 ? 'checked=""' : ''}  >
    </div>
</div>
`



                }
            }

        ]
    });

    $('#tableModules tbody').on('change', '.dt-selected-checkbox', function () {
        const isChecked = this.checked;

        const rowData = tableModules.row($(this).closest('tr')).data();


        //const id = rowData.plantRoadmapLinkSysId;
        //const name = rowData.name;
        //const originalIsSelected = rowData.isSelected;
        //const inputId = this.id;
        const moduleCode = $(this).data().moduleCode;
        const userGroupAccessRightSysId = $(this).data().id;
        const userGroupId = id;
        // Collect data
        var _data = {
            moduleCode: moduleCode,
            userGroupId: userGroupId,
            id: userGroupAccessRightSysId,
            isSelected: isChecked ? 1 : 0
        };

        var formData = new FormData();


        formData.append("usergroupmodule", JSON.stringify(_data));

        // AJAX call
        $.ajax({
            url: apiPath + `/api/usergroups/${id}/module/${isChecked ? "authorize" : "restrict"}/${moduleCode}`,
            type: isChecked ? "POST" : "DELETE",
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {

                //tableRoadmaps.ajax.reload(null, false);
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    toastr.error('Link does not exist!');
                } else {
                    if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                        toastr.error('Module already linked!');
                    } else {
                        alert('Error: ' + xhr.responseText);
                    }

                }
            },

        });



        ////alert(
        ////    'Row ID: ' + id +
        ////    '\nNew checked state: ' + isChecked +
        ////    '\nOriginal isSelected: ' + originalIsSelected
        ////);


    });
    // ------------  END: MODULES  ----------------
    
    // ------------  START: MEMBERS  ----------------
    var firstEmptySelect = true;
    var selectedUser = {};
    function matchCustom(params, data) {
        firstEmptySelect = true;
        if (!query.term) {
            return option;
        }
        var has = true;
        var words = query.term.toUpperCase().split(" ");
        for (var i = 0; i < words.length; i++) {
            var word = words[i];
            has = has && (option.text.toUpperCase().indexOf(word) >= 0);
        }
        if (has) return option;
        return false;
    }

    function formatSelect2Username(result) {
        if (result.userId == null) {

            if (firstEmptySelect) {
                console.log('showing row');
                firstEmptySelect = false;
                return '<div class="row">' +
                    '<div class="col-xs-2"><b>User Name</b></div>' +
                    '<div class="col-xs-2"><b>First Name</b></div>' +
                    '<div class="col-xs-2"><b>Last Name</b></div>' +
                    '<div class="col-xs-2"><b>Employee ID</b></div>' +
                    '<div class="col-xs-2"><b>Email</b></div>' +
                    '</div><hr />';


            } else {

                console.log('skipping row');
                return false;
            }
        }

        if (!result.userId) return result.text;


        var markup = "<div><b>" + result.firstName + " " + result.lastName + "</b><div class='pl-1'><i>User Name:" + result.userName + "; User Id:" + result.userId + "; Email:" + result.email + "</i></div></div>";

        return markup;
    }

    var userSearch = $('#UserSearch').select2({
        ajax: {
            url: apiPath + "/api/ActiveDirectory/Search",
            type: "GET",
            data: function (params) {
                var query = {
                    key: params.term
                };
                return query;
            },
            delay: 250,
            processResults: function (data, params) {

                var __formattedData = $.map(data.data, function (obj) {
                    obj.text = obj.firstName + " " + obj.lastName;
                    obj.id = obj.userId;
                    return obj;
                })

                return {
                    results: __formattedData
                };
            }
        },
        cache: true,
        placeholder: 'Search user to add (First Name, Last Name, Username, Email)',
        escapeMarkup: function (m) { return m; },
        allowClear: true,
        minimumInputLength: 3,
        templateResult: formatSelect2Username,
        templateSelection: function (data, container) {
            if (!data.id) return data.text || '';


            $(data.element).attr('data-custom-attribute', data.customValue);

            if (data.id) {
                selectedUser.UserName = data.userName;
                selectedUser.UserId = data.id;
                selectedUser.FirstName = data.firstName;
                selectedUser.LastName = data.lastName;
                selectedUser.Email = data.email;
            }
            else {
                return "";
            }
            if (data.firstName == undefined) return data.text;

            return data.firstName + " " + data.lastName;
        },
        matcher: matchCustom
    });

    $('#formAddUserGroupMember').validate({
        // Add your validation rules if needed
        rules: {
            UserSearch: { required: true }
        },
        messages: {
            UserSearch: { required: "" }
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
             // Prepare FormData
            var formData = new FormData();
 
            // Collect data
            var _data = {
                userId: $('#UserSearch').val() ,
                userGroupId: id,
                user: selectedUser
            };



            var formData = new FormData();
 
            formData.append("usergroupmember", JSON.stringify(_data));
 
            // AJAX call
            $.ajax({
                url: apiPath + `/api/usergroups/${id}/member/link`,
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {
                    toastr.success('User is successfully registered!')
                    $('#UserSearch').val(null).trigger('change');
                    tableMembers.ajax.reload(null, false);
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Link does not exist!');
                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                            toastr.error('User already linked!');
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

    var tableMembers = $('#tableMembers').DataTable({
        "processing": true, "responsive": true,
        "serverSide": false,
        "ajax": {
            "url": apiPath + `/api/usergroups/${id}/members`,
            "type": "GET",
            "contentType": "application/json"
        },
        "initComplete": function () {
            document.querySelectorAll(".status-switch .form-check-input").forEach(function (input) {
                input.addEventListener("change", function () {
                    const pill = input.closest("td").querySelector(".status-pill-text");
                    const isOn = input.checked;

                    if (isOn) {
                        pill.textContent = "Allowed";
                        pill.classList.remove("status-pill-off");
                        pill.classList.add("status-pill-on");
                    } else {
                        pill.textContent = "Restricted";
                        pill.classList.remove("status-pill-on");
                        pill.classList.add("status-pill-off");
                    }
                });
            });


        },
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        columnDefs: [

            { targets: [1, 2], className: 'text-center', orderable: false },
        ],
        "language": {
            "search": "",
            "searchPlaceholder": "Search...",
            "emptyTable": "No data found.",
            'processing': '<div>Loading data please wait...  </div>',
            paginate: {
                previous: "«",
                next: "»"
            }
        },
        "columns": [
            {
                "data": "user.firstName"
            },
            {
                "data": "user.lastName"
            },
            {
                "data": "user.email"
            },
            {
                "data": "userId",
                "render": function (value, type, data) {
                    return `<button data-key="${value}" class="removemember btn btn-sm btn-outline-dark mb-0" type="button">Remove</button>`;

                }
            }
        ]
    });

    $('#tableMembers tbody').on('click', '.removemember', function () {
        const userId = $(this).data().key;
        const userGroupId = id;

        // Collect data
        var _data = {
            userId: userId,
            userGroupId: userGroupId
        };

        var formData = new FormData();


        formData.append("usergroupmember", JSON.stringify(_data));



        // AJAX call
        $.ajax({
            url: apiPath + `/api/usergroups/${id}/member/unlink`,
            type: "DELETE",
            data: formData,
            processData: false,
            contentType: false,
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function () {
                toastr.success('User is successfully unregistered!')
                tableMembers.ajax.reload(null, false);
            },
            error: function (xhr) {
                if (xhr.status === 404) {
                    toastr.error('Link does not exist!');
                } else {
                    if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                        toastr.error('Module already linked!');
                    } else {
                        alert('Error: ' + xhr.responseText);
                    }

                }
            },

        });



    });

    // ------------  END: MEMBERS  ----------------


    // Initialize jQuery Validate
    $('#formBasicInfo').validate({
        // Add your validation rules if needed
        rules: {
            Name: { required: true }
        },
        messages: {
            Name: { required: "" }
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
            // Collect data
            var usergroup = {
                UserGroupId: $('#Id').val(),
                UserGroupName: $('#Name').val(),
                UserGroupDescription: $('#Description').val(),
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#buttonIsActive').data('action') == "deactivate" ? 1 : 0
            };

            var formData = new FormData();

            formData.append("usergroup", JSON.stringify(usergroup));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/usergroups/' + usergroup.UserGroupId,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('User Group is successfully updated!')
                    fetchPageInfo();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('User Group code does not exist!');

                    } else {
                        toastr.error('Error: ' + xhr.responseText);
                    }
                }
            });


            // Prevent default form submission
            return false;
        }
    });

    $('#formChangeStatus').submit(function () {
        // Collect data
        var usergroup = {
            UserGroupId: $('#Id').val(),
            UserGroupName: $('#labelName').text(),
            UserGroupDescription: $('#labelDescription').text(),
            TransactionKey: $('#TransactionKey').val(),
            IsActive: $('#buttonIsActive').data('action') == "deactivate" ? 0 : 1
        };

        var formData = new FormData();

        formData.append("usergroup", JSON.stringify(usergroup));

        // AJAX call
        $.ajax({
            url: apiPath + '/api/usergroups/' + usergroup.UserGroupId,
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
                    toastr.error('User Group code does not exist!');

                } else {
                    toastr.error('Error: ' + xhr.responseText);
                }
            }
        });

        // Prevent default form submission
        return false;
    });

    $('#formDelete').submit(function () {

        bootbox.confirm({
            title: "Confirm Deletion",
            message: "Are you sure you want to delete this User Group?",
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
                        UserGroupId: $('#Id').val(),
                        TransactionKey: $('#TransactionKey').val()
                    };

                    formData.append("usergroup", JSON.stringify(submitform));

                    // AJAX call
                    $.ajax({
                        url: apiPath + '/api/usergroups/' + $('#RoadmapSysId').val(),
                        type: 'DELETE',
                        data: formData,
                        processData: false,
                        contentType: false,
                        // xhrFields: { withCredentials: true }, //** REMOVED**
                        success: function () {

                            bootbox.alert({
                                title: "User Group Deleted",
                                message: "User Group is successfully deleted!",
                                callback: function () {
                                    window.location.href = "/templates/Roadmaps";
                                }
                            });



                        },
                        error: function (xhr) {
                            if (xhr.status === 404) {
                                toastr.error('User Group does not exist!');

                            } else {
                                if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                                    toastr.error('User Group already exist!');
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