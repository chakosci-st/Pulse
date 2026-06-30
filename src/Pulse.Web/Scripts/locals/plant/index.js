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
const appPath = getAppRootPath();
const apiPath = getApiRootPath();

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
        <img src="${appPath}/uploads/plant/${data.plantCode}.png" alt="User" class="profile-img" onerror="showInitial(this, '${data.plantCode}')">
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
                    return `<a href='${appPath}/admin/plants/edit/${value}' class="nav-link p-0 text-primary">edit</a>`;
                }
            },

        ]
    });

    // Redraw table when filter changes
    $('#isActiveFilter').change(function () {
        table.ajax.reload();
    });


});