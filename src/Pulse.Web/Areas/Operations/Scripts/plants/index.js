resetHeroDecorations();


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
                    //var sortDir = d.order[0].dir;      // 'asc' or 'desc'
                    //var sortBy = d.columns[sortIndex].data; // column name/key

                    var sortDir = "asc";      // 'asc' or 'desc'
                    var sortBy = $('#sortBy').val(); // column name/key

                    // Add to payload
                    d.sortBy = sortBy;
                    d.sortDirection = sortDir;
                }

                return JSON.stringify(d);
            }
        },
        "dom": ' <"search"f><"top"l>rt<"bottom"ip><"clear">',
        "lengthMenu": [[10, 25, 50, 100, -1], ["Entries per page: 10", "Entries per page: 25", "Entries per page: 50", "Entries per page: 100", "Entries per page: All"]],
        columnDefs: [
 

            
        ],
        drawCallback: function (settings) {
            var api = this.api();

            api.rows().every(function () {
                var data = this.data();
                var node = this.node();

                loadAndRenderAvatarGroup({
                    dataSource: {
                        url: apiPath + '/api/plantmembers/PerPlant',
                        method: 'GET',
                        data: { code: data.plantCode }
                    },
                    container: `#${data.plantCode}-avatar-group-container`,
                    maxVisible: 10,
                    avatarSize: 50,
                    avatarSpacing: 20,
                    label: 'Member',
                    emptyText: 'No members found',
                    backgroundColor: '#e0f2fe',
                    fontColor: '#075985',
                    labelBackgroundColor: '#607d8b', // blue-grey for "+N"
                    userInformationUrl: '/Settings/Profile/Index/{id}',
                    userInformationTarget: '_self',
                    labelFontColor: '#fff',
                    sort: 'initials',
                    onMoreClick: function (extraUsers, event) {
                        alert('Show more users:\n' + extraUsers.map(u => u.name).join(', '));
                        // You can open a modal, navigate, etc.
                    },
                    onLabelClick: function (allUsers, event) {
                        alert('All users:\n' + allUsers.map(u => u.name).join(', '));
                        // You can open a modal, navigate, etc.
                    },
                    transform: function (data) {
                        return data.map(function (item) {
                            return {
                                id: item.userId,
                                name: item.userInfo.firstName + " " + item.userInfo.lastName,
                                avatarUrl: "https://calwebapps.cal.st.com/ProfilePhoto/" + item.userId + ".jpg"
                            };
                        });
                    },
                });
            });
        },
        "initComplete": function () {
            var api = this.api();
            var $tableContainer = $(api.table().container());
            var $toolbar = $('.table-filter-toolbar');

            $tableContainer.find('.dt-paging').appendTo('.card-tools-plantsTable-pagination');
            $tableContainer.find('.dt-search').appendTo('.card-tools-plantsTable-filter');
            $tableContainer.find('.dt-length').appendTo('.card-tools-plantsTable-length');
            $tableContainer.find('.dt-info').appendTo('.card-tools-plantsTable-size');

            $toolbar.find('.dt-search').addClass('toolbar-search__control');
            $toolbar.find('.dt-search label').addClass('toolbar-search__label');
            $toolbar.find('.dt-search input').addClass('form-control').attr('type', 'search');
            $toolbar.find('.dt-length select').addClass('form-select');

            document.querySelectorAll('.toolbar-search__label').forEach(el => el.remove());
            $('label[for="dt-length-0"]').remove();

            //$('#plantsTable_buttons').appendTo('.card-tools-plantsTable-buttons');
            //datatableMyTasks.buttons().container().appendTo('.card-tools-plantsTable-buttons');




        },
        "language": {
            "lengthMenu": "_MENU_",
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
                    
////                    return `
////<div class="d-flex align-items-center"> 

////    <div class="user-avatar shadow medium site-md">
////    <div class="profile-circle" title="${value}: ${data.plantName}">
////        <img src="${appPath}/content/uploads/plantfiles/${data.plantCode}.jpg" alt="User" class="profile-img" onerror="showInitial(this, '${data.plantCode}')">
////    </div> 
////    </div>
////    <span class="mx-2 mb-3 mb-md-0 text-body-secondary">${data.plantCode}: ${data.plantName}</span>
////</div>

////`;

                    var status = "";

                    if (data.isActive)
                        status =  `<span class="badge text-bg-success mr-2" style="margin-right:5px">Online</span>`;
                    else
                        status = `<span class="badge text-bg-danger  mr-2" style="margin-right:5px">Offline</span>`;


                    const formattedCreateDate = moment(data.createdDate).format("MMM DD, YYYY HH:mm");
                    const relativeModifiedTime = moment(data.modifiedDate ?? data.createdDate).fromNow();

                    return ` 
    <div class="card request-card mb-2">
      <div class="card-body">
        <div class="d-lg-flex align-items-center">
          <!-- Left: main info -->
           <div class="pr-lg-3 mr-lg-3" style="margin-right:10px">
            <div class="user-avatar shadow medium site-md">
                <div class="profile-circle" title="${value}: ${data.plantName}">
                    <img src="${appPath}/content/uploads/plantfiles/${data.plantCode}.jpg" alt="User" class="profile-img" onerror="showInitial(this, '${data.plantCode}')">
                </div>
            </div>
          </div>
          <div class="flex-fill pr-lg-3">
            <div class="d-flex flex-wrap align-items-center mb-1">
              ${status}
              <a href="/templates/plants/overview/${data.plantCode}" class="btn-list-key font-weight-semibold small text-primary">
                <strong>${data.plantName}</strong>
              </a>
            </div>
            <div class="small text-muted mb-1">
              Code: <strong>${data.plantCode}</strong>
            </div>
            <div class="small text-muted mb-1">
              Created: ${formattedCreateDate}
            </div>
          </div>

          <!-- Middle: requester (you) -->
          <div class="d-flex align-items-center mt-3 mt-lg-0 mr-lg-3">
            <div id="${data.plantCode}-avatar-group-container"></div>
          </div>

          <!-- Right: actions -->
          <div class="request-actions d-flex flex-column align-items-lg-end mt-3 mt-lg-0">
                        <div class="mb-1 d-flex gap-2 justify-content-lg-end">
<a title="Display" href="/Operations/plants/display/${data.plantCode}"
                class="btn btn-outline-secondary rounded-circle d-inline-flex align-items-center justify-content-center p-0"
                style="width: 40px; height: 40px;">
    <i class="bi bi-eye"></i>
</a>
<a title="Config" href="/Operations/plants/configuration/${data.plantCode}"
        class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0"
        style="width: 40px; height: 40px;">
  <i class="bi bi-gear"></i>
</a>  
            </div> 
            <small class="text-muted">⏱ Updated ${relativeModifiedTime}</small>
          </div>
        </div>
      </div>
    </div>
`







                }
            },
 

        ]
    });

 

    $('#sortBy').change(function () {
        table.ajax.reload();
    });

    // Redraw table when filter changes
    $('#isActiveFilter').change(function () {
        table.ajax.reload();
    });


});