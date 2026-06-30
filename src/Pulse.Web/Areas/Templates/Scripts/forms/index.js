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
    var table = $('#formsTable').DataTable({
        "processing": true, "responsive": true,
        "serverSide": true,
        "ajax": {
            "url": getApiRootPath() + "/api/forms/datatables",
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
        "lengthMenu": [[10, 25, 50, 100, -1], ["Entries per page: 10", "Entries per page: 25", "Entries per page: 50", "Entries per page: 100", "Entries per page: All"]],
        columnDefs: [



        ],

        "initComplete": function () {
            var api = this.api();
            var $tableContainer = $(api.table().container());
            var $toolbar = $('.table-filter-toolbar');

            $tableContainer.find('.dt-paging').appendTo('.card-tools-formsTable-pagination');
            $tableContainer.find('.dt-search').appendTo('.card-tools-formsTable-filter');
            $tableContainer.find('.dt-length').appendTo('.card-tools-formsTable-length');
            $tableContainer.find('.dt-info').appendTo('.card-tools-formsTable-size');

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
                "data": "name",
                "render": function (value, type, data, meta) {

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
                        status = `<span class="badge text-bg-success mr-2" style="margin-right:5px">Active</span>`;
                    else
                        status = `<span class="badge text-bg-danger  mr-2" style="margin-right:5px">Inactive</span>`;


                    const formattedCreateDate = moment(data.createdDate).format("MMM DD, YYYY HH:mm");
                    const relativeModifiedTime = moment(data.modifiedDate ?? data.createdDate).fromNow();

                    return ` 
    <div class="card request-card mb-2">
      <div class="card-body">
        <div class="d-lg-flex align-items-center">
          <!-- Left: main info -->
           <div class="pr-lg-3 mr-lg-3" style="margin-right:10px">
         
          </div>

          <div class="flex-fill pr-lg-3">
            <div class="d-flex flex-wrap align-items-center mb-1">
              ${status}

                <strong>${data.name}</strong>
             
            </div>
            <div class="small text-muted mb-1">
            Description: ${data.description ?? ''}
            </div>
            <div class="small text-muted mb-1">
              Created: ${formattedCreateDate}
            </div>
          </div>
        


         <div class="flex-fill pr-lg-3 text-center">
            <button
                class="btn btn-link nav-link text-primary p-0 show-form-btn"
                data-row="${meta.row}" 
                type="button">
              <div class="form-thumb" 
                   data-row="${meta.row}" 
                   style="width:140px;height:90px;background:#f8f9fa;border-radius:8px;display:flex;align-items:center;justify-content:center;">
                <span class="spinner-border spinner-border-sm"></span>
              </div>
              <div class="small text-muted mt-1">Preview</div>
            </button>
          </div>
         

          <!-- Right: actions -->
          <div class="request-actions d-flex flex-column align-items-lg-end mt-3 mt-lg-0">
                        <div class="mb-1 d-flex gap-2 justify-content-lg-end">
<a title="Display" href="/templates/forms/display/${data.id}"
                class="btn btn-outline-secondary rounded-circle d-inline-flex align-items-center justify-content-center p-0"
                style="width: 40px; height: 40px;">
    <i class="bi bi-eye"></i>
</a>
<a title="Copy"  href="/templates/forms/copy/${data.id}"
        class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0"
        style="width: 40px; height: 40px;">
  <i class="bi bi-copy"></i>
</a>   
<a title="Config" href="/templates/forms/edit/${data.id}"
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
`;


                }
            },


        ]
    });
    $('#formsTable').on('draw.dt', function () {
        const tableData = table.rows({ page: 'current' }).data().toArray();

        $('#formsTable .form-thumb').each(function () {
            const rowIdx = $(this).data('row');
            const formjsonStr = tableData[rowIdx].formJson;
            let formObj;

            try {
                formObj = JSON.parse(formjsonStr);
                if (formObj.fields == null) {
                    $(this).html(`
    <span style="display:flex;align-items:center;justify-content:center;width:100%;height:100%;">
      <svg width="40" height="40" viewBox="0 0 40 40">
        <circle cx="20" cy="20" r="18" stroke="#bbb" stroke-width="3" fill="none"/>
        <line x1="10" y1="10" x2="30" y2="30" stroke="#bbb" stroke-width="3"/>
      </svg>
    </span>
`);
                    return;
                }
            } catch (e) {
                $(this).html('<span style="font-size:2em;color:#bbb;display:flex;align-items:center;justify-content:center;width:100%;height:100%;">&#128683;</span>');
                return;
            }

            const fields = formObj.fields || [];
            const $thumb = $(this);

            $.generateFormThumbnail(fields, { userCode: "*" }, function (imgData) {
                $thumb.html(`
<img src="${imgData}" style="max-width:100%;max-height:80px;border-radius:6px;box-shadow:0 1px 4px rgba(0,0,0,0.07);">
`);
            });
        });
    });

    // === BOOTBOX PREVIEW (same as old, tied to show-form-btn) ===
    $('#formsTable').on('click', '.show-form-btn', function () {
        const tableData = table.rows({ page: 'current' }).data().toArray();
        const rowIdx = $(this).data('row');
        const formjsonStr = tableData[rowIdx].formJson;

        let formObj;
        try {
            formObj = JSON.parse(formjsonStr);
        } catch (e) {
            bootbox.alert("Invalid form JSON!");
            return;
        }

        const fields = formObj.fields || [];
        const $formContainer = $('<form class="bootbox-form-container"></form>');

        const normalizedFields = fields.map(fld => ({
            ...fld,
            isrequired: fld.isrequired === "true",
            urlIsParameter: fld.urlIsParameter === "true"
        }));

        $formContainer.dynamicField({ fields: normalizedFields, userCode: "*" });

        bootbox.dialog({
            title: "Form Preview",
            message: $formContainer,
            size: 'large',
            buttons: {
                cancel: {
                    label: "Close",
                    className: 'btn-secondary'
                },
                submit: {
                    label: "Submit",
                    className: 'btn-primary',
                    callback: function () {
                        var values = $formContainer.dynamicFieldGetValues();
                        bootbox.alert({
                            title: "Form Values",
                            message: `<pre>${JSON.stringify(values, null, 2)}</pre>`
                        });
                        return false;
                    }
                }
            }
        });
    });



    $('#sortBy').change(function () {
        table.ajax.reload();
    });

    // Redraw table when filter changes
    $('#isActiveFilter').change(function () {
        table.ajax.reload();
    });


});