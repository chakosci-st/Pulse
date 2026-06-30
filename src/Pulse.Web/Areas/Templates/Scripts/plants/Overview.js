var appPath = getAppRootPath();
var apiPath = getApiRootPath();

var firstEmptySelect = true;
var selectedUser = {};
var initial = "";
function showInitial(img) {
    // Remove the broken image
    img.style.display = 'none';
    // Create the initial element
    const initialDiv = document.createElement('div');
    initialDiv.className = 'profile-initial';
    initialDiv.textContent = initial;
    // Insert after the image
    img.parentNode.appendChild(initialDiv);
}

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


// Function to fetch a single page of plants
function fetchPageInfo() {

    return $.ajax({
        "url": getApiRootPath() + "/api/plants/GetById?code=" + id,
        "type": "GET",
        "contentType": "application/json",
        // xhrFields: { withCredentials: true }, //** REMOVED**
        success: function (response) {
            $('#labelPlantCode').text(response.plantCode);
            $('#labelPlantName').text(response.plantName);
            initial = response.plantCode;

            $('#imgPlantBanner').attr('src', appPath + '/Content/uploads/PlantFiles/' + response.plantCode + ".jpg");

            $('#Code').val(response.plantCode);
            $('#Name').val(response.plantName);

            $('#PlantCode').val(response.plantCode);
            $('#PlantName').val(response.plantName);
            $('#TransactionKey').val(response.transactionKey);
            $('#profileImage').attr('src', getAppRootPath() + '/Content/uploads/PlantFiles/' + response.plantCode + ".jpg");

            //$('#isActive').prop('checked', response.isActive).trigger('change');

            $("#IsActive").val(response.isActive);


            if (!response.isActive) {
                let buttonActive = $("#buttonIsActive");
                buttonActive.html("Activate");
                buttonActive.addClass('btn-outline-success');
                buttonActive.removeClass('btn-outline-secondary');
                buttonActive.attr('data-action', 'activate');
            }


            // Update checked state programmatically
            //$('#isActive').statusSwitch('setChecked', );
        },
        error: function (xhr, status, error) {

        }
    });

}


$(document).ready(function () {

    $('#buttonAddMember').on('click', function (e) {

    });

    // Initialize with custom options
    $('#isActive').statusSwitch({
        checked: true,
        onText: 'Online',
        offText: 'Offline',
        size: 'lg'
    });

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



    var tableMembers = $('#tableMembers').DataTable({
        "processing": true, "responsive": true,
        "serverSide": false,
        "ajax": {
            "url": apiPath + '/api/plantmembers/PerPlantDatatables?code=' + id,
            "type": "GET",
            "contentType": "application/json"
        },
        "initComplete": function () {
            document.querySelectorAll(".status-switch .form-check-input").forEach(function (input) {
                input.addEventListener("change", function () {
                    const pill = input.closest("td").querySelector(".status-pill-text");
                    const isOn = input.checked;

                    if (isOn) {
                        pill.textContent = "Active";
                        pill.classList.remove("status-pill-off");
                        pill.classList.add("status-pill-on");
                    } else {
                        pill.textContent = "Inactive";
                        pill.classList.remove("status-pill-on");
                        pill.classList.add("status-pill-off");
                    }

                    ////// Optional: update "last updated" text
                    ////const now = new Date();
                    ////document.getElementById("lastUpdated").textContent =
                    ////    now.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
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
                "data": "userInfo.firstName",
                "render": function (value, type, data) {

                    return data.userInfo.firstName + " " + data.userInfo.lastName;
                }
            },
            {
                "data": "isActive",
                "class": "text-center",
                "render": function (value, type, data) {



                    return `
<div class="d-flex flex-column align-items-center gap-2" id="roadmapselected${data.plantMemberSysId}">
    <label class="float-end form-check-label mb-0">
        <span class="status-pill ${value == 1 ? 'status-pill-on' : 'status-pill-off'} status-pill-text">${value == 1 ? 'Active' : 'Inactive'}</span>
    </label>
    <div class="form-check form-switch status-switch m-0">
        <input class="form-check-input dt-selected-checkbox" data-plantCode="${data.plantCode}" data-transactionKey="${data.transactionKey}" data-plantMemberSysId="${data.plantMemberSysId}" data-userId="${data.userId}" type="checkbox" id="isActive${data.plantMemberSysId}"  ${value == 1 ? 'checked=""' : ''}  >
    </div>
</div>
`



                }
            },
            {
                "data": "plantMemberSysId",
                "render": function (value, type, data) {



                    var _control = `<input class="form-check-input" type="checkbox" id="isActive${data.plantMemberSysId}"  ${value ? 'checked=""' : ''}  >`;



                    return `
<button data-key="${value}" class="setupmember btn btn-sm btn-outline-dark mb-0" type="button">Set up</button>
`

                }
            }

        ]
    });

    $('#tableMembers tbody').on('change', '.dt-selected-checkbox', function () {
        const isChecked = this.checked;

        const rowData = tableMembers.row($(this).closest('tr')).data();


        //const id = rowData.plantRoadmapLinkSysId;
        //const name = rowData.name;
        //const originalIsSelected = rowData.isSelected;
        //const inputId = this.id;
        const plantcode = $(this).data().plantcode
        const userId = $(this).data().userid
        const plantMemberSysId = $(this).data().plantmembersysid
        const transactionKey = $(this).data().transactionkey

        // Collect data
        var _data = { 
            plantCode: plantcode,
            userId: userId,
            plantMemberSysId: plantMemberSysId,
            transactionKey: transactionKey,
            isSelected: isChecked ? 1 : 0
        };

        var formData = new FormData();


        formData.append("plantmember", JSON.stringify(_data));

        // AJAX call
        $.ajax({
            url: getApiRootPath() + `/api/plantmembers/${plantMemberSysId}/${isChecked ? "activate" : "deactivate"}`,
            type: "PUT",
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
                        toastr.error('Member already exist!');
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




    var tableRoadmaps = $('#tableRoadmaps').DataTable({
        "processing": true,
        "responsive": true,
        "serverSide": false,
        "paging": false,
        info: false,
        "ajax": {
            "url": getApiRootPath() + "/api/plantroadmaplinks?code=" + id,
            "type": "GET",
            "contentType": "application/json"
        },
        "initComplete": function () {
            document.querySelectorAll(".status-switch .form-check-input").forEach(function (input) {
                input.addEventListener("change", function () {
                    const pill = input.closest("td").querySelector(".status-pill-text");
                    const isOn = input.checked;

                    if (isOn) {
                        pill.textContent = "Active";
                        pill.classList.remove("status-pill-off");
                        pill.classList.add("status-pill-on");
                    } else {
                        pill.textContent = "Inactive";
                        pill.classList.remove("status-pill-on");
                        pill.classList.add("status-pill-off");
                    }

                    // Optional: update "last updated" text
                    const now = new Date();
                    document.getElementById("lastUpdated").textContent =
                        now.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
                });
            });
        },
        columnDefs: [

            { targets: [2, 3], className: 'text-center', orderable: false },
        ],
        "language": {
            "search": "",
            "searchPlaceholder": "Search...",
            "emptyTable": "No data found.",
            'processing': '<div>Loading data please wait...  </div>',
        },
        "columns": [
            {
                "data": "roadmap.roadmapName",
                "render": function (value, type, data) {


                    return `
<div class="d-flex flex-column">
    <span class="item-name">${data.roadmap.roadmapName}</span>
    <div class="d-flex align-items-center gap-2 mt-1">
        <span class="tag">${data.roadmap.categoryCode}</span>
        <span class="text-muted small">${data.roadmap.category.categoryName}</span>
    </div>
</div>
`
                }
            },
            {
                "data": "roadmap.roadmapDescription",
                "render": function (value, type, data) {

                    return `
<p class="item-desc mb-0">
    ${value}
</p>
`

                }
            },
            {
                "data": "isSelected",
                "render": function (value, type, data) {

                    return `
<div class="d-flex flex-column align-items-center gap-2" id="roadmapselected${data.plantRoadmapLinkSysId}">
    <label class="float-end form-check-label mb-0">
        <span class="status-pill ${value == 1 ? 'status-pill-on' : 'status-pill-off'} status-pill-text">${value == 1 ? 'Active' : 'Inactive'}</span>
    </label>
    <div class="form-check form-switch status-switch m-0">
        <input class="form-check-input dt-selected-checkbox" type="checkbox" data-roadmapsysid="${data.roadmapSysId}" data-plantcode="${data.plantCode}" data-linkid="${data.plantRoadmapLinkSysId}" id="isActive${data.plantRoadmapLinkSysId}"  ${value == 1 ? 'checked=""' : ''}  >
    </div>
</div>
`
                }
            },
            {
                "data": "plantRoadmapLinkSysId",
                "sortable": false,
                "render": function (value, type, data) {
                    return `<a data-key="${value}" class="setupmember btn btn-sm btn-outline-dark mb-0" href="/Templates/Roadmaps/Edit/${data.roadmapSysId}"  >Set up</a>`
                }
            }

        ]
    });

    $('#tableRoadmaps tbody').on('change', '.dt-selected-checkbox', function () {
        const isChecked = this.checked;

        const rowData = tableRoadmaps.row($(this).closest('tr')).data();


        const id = rowData.plantRoadmapLinkSysId;
        const name = rowData.name;
        const originalIsSelected = rowData.isSelected;
        const inputId = this.id;
        const key = $(this).data('linkid')
        const plantcode = $(this).data('plantcode')
        const roadmapsysid = $(this).data('roadmapsysid')

        // Collect data
        var _data = {
            plantRoadmapLinkSysId: key,
            plantCode: plantcode,
            roadmapSysId: roadmapsysid,
            isSelected: isChecked ? 1 : 0
        };

        var formData = new FormData();


        formData.append("plantroadmaplink", JSON.stringify(_data));

        // AJAX call
        $.ajax({
            url: getApiRootPath() + '/api/plantroadmaplinks' + (rowData.plantRoadmapLinkSysId ? `/${rowData.plantRoadmapLinkSysId}` : ''),
            type: key ? "PUT" : "POST",
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
                        toastr.error('Member already exist!');
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

    //// Get checked state
    //var isChecked = $('#mySwitch').statusSwitch('getChecked');
    //console.log(isChecked); // true or false

    //// Update the UI (if you change the checkbox state directly)
    //$('#mySwitch input[type=checkbox]').prop('checked', true);
    //$('#mySwitch').statusSwitch('update');


    fetchPageInfo();


    // Initialize jQuery Validate
    $('#formUpdatePlant').validate({
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
            // This function runs only if the form is valid

            // Prepare FormData
            var formData = new FormData();
            var fileInput = $('#siteImage')[0];
            if (fileInput.files.length > 0) {
                formData.append("file", fileInput.files[0]);
            }
            // Collect data 
            var plant = {
                PlantCode: $('#Code').val(),
                PlantName: $('#Name').val(),
                TransactionKey: $('#TransactionKey').val(),
                IsActive: $('#isActive').val()
            };
            formData.append("plant", JSON.stringify(plant));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/plants/' + plant.PlantCode,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {
                    toastr.success('Plant is successfully updated!');
                    fetchPageInfo();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Plant code does not exist!');

                    } else {
                        toastr.error('Error: ' + xhr.responseText);
                    }
                }
            });

            // Prevent default form submission
            return false;
        }
    });

    // Initialize jQuery Validate
    $('#formAddPlantMember').validate({
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
            // This function runs only if the form is valid

            // Prepare FormData
            var formData = new FormData();

            // Collect data 
            var plantmember = {
                plantCode: $('#Code').val(),
                userId: $('#UserSearch').val(),
                plantInfo: {
                    plantCode: $('#PlantCode').val(),
                    plantName: $('#PlantName').val(),
                },
                userInfo: selectedUser
            };
            formData.append("plantmember", JSON.stringify(plantmember));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/plantmembers',
                type: 'POST',
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
                        toastr.error('Plant Member does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint') > 0) {
                            toastr.error('Member already exist!');
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