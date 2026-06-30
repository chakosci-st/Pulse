function showInitial(img, initial) {
    // Remove the broken image
    img.style.display = 'none';
    // Create the initial element
    const initialDiv = document.createElement('div');
    initialDiv.className = 'profile-initial';
    initialDiv.textContent = initial;
    // Insert after the image
    img.parentNode.appendChild(initialDiv);
}
var appPath = getAppRootPath();
var apiPath = getApiRootPath();

$(document).ready(function () {


    var table = $('#plantsTable').DataTable({
        "processing": true, "responsive": true,
        "serverSide": true,
        "ajax": {
            "url": getApiRootPath() + "/api/plants/datatables",
            "type": "POST",
            "contentType": "application/json",
            "data": function (d) {
                // Add custom filter
                d.isActive = $('#isActiveFilter').val();

                // Extract sort info
                if (d.order && d.order.length > 0) {
                    var sortIndex = d.order[0].column; // index of sorted column
                    var sortDir = d.order[0].dir;      // 'asc' or 'desc'
                    var sortBy = d.columns[sortIndex].data; // column name/key

                    // Add to payload
                    d.sortBy = sortBy;
                    d.sortDirection = sortDir;
                }

                return JSON.stringify(d);
            }
        },
        "dom": ' <"search"f><"top"l>rt<"bottom"ip><"clear">',
        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        columnDefs: [
            { targets: [1, 2, 3, 4], className: 'text-center' },
            { targets: 5, className: 'text-center', orderable: false },

            { targets: [0, 1, 5], responsivePriority: 1, },
            { targets: [2, 3, 4], responsivePriority: 2, },

            
        ],
        "initComplete": function () {

            $('.dt-paging').appendTo('.card-tools-plantsTable-pagination');
            $('.dt-search').appendTo('.card-tools-plantsTable-filter');
            $('.dt-length').appendTo('.card-tools-plantsTable-length');

            $('#plantsTable_info').appendTo('.card-tools-plantsTable-size');

            //$('#plantsTable_buttons').appendTo('.card-tools-plantsTable-buttons');
            //datatableMyTasks.buttons().container().appendTo('.card-tools-plantsTable-buttons');

        },
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
                "data": "plantCode",
                "render": function (value, type, data) {
                    
                    return `
<div class="d-flex align-items-center"> 

    <div class="user-avatar shadow medium site-md">
    <div class="profile-circle" title="${value}: ${data.plantName}">
        <img src="${appPath}/content/uploads/plantfiles/${data.plantCode}.jpg" alt="User" class="profile-img" onerror="showInitial(this, '${data.plantCode}')">
    </div>


    </div>
    <span class="mx-2 mb-3 mb-md-0 text-body-secondary">${data.plantCode}: ${data.plantName}</span>
</div>

`;
                }
            },
            {
                "data": "isActive",
                "render": function (value, type, data) {
                    if (value)
                        return `<span class="badge text-bg-success">Online</span>`;
                    else
                        return `<span class="badge text-bg-danger">Offline</span>`;
                }
            },
            { "data": "activeProjectsCount" },
            { "data": "activeTasksCount" },
            { "data": "productCount" },
            {
                "data": "plantCode",
                "render": function (value, type, data) {
                    var targetUrl = canEditPlants ? `${appPath}/sites/plants/Overview?code=${value}` : `${appPath}/sites/plants/Display?code=${value}`;
                    var label = canEditPlants ? 'edit' : 'view';
                    return `<a href='${targetUrl}' class="nav-link p-0 text-primary">${label}</a>`;
                }
            },

        ]
    });

    // Initialize jQuery Validate
    $('#formAddMember').validate({
        // Add your validation rules if needed
        rules: {
            Code: { required: true },
            Name: { required: true }
        },
        messages: {
            Code: { required: "" },
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
            // Collect plant data (example)
            var plant = {
                PlantCode: $('#Code').val(),
                PlantName: $('#Name').val(),
                IsActive: 1
            };
            formData.append("plant", JSON.stringify(plant));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/plants',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success('Plant created successfully!')
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.status === 404) {
                        toastr.error('Plant code does not exist!');

                    } else {
                        if (xhr.responseText.indexOf('ORA-00001: unique constraint (NPITRACK.PLANTS_PK) violated') > 0) {
                            toastr.error('Plant code already exist!');
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




    // Redraw table when filter changes
    $('#isActiveFilter').change(function () {
        table.ajax.reload();
    });


});