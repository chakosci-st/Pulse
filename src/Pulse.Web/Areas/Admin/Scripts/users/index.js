resetHeroDecorations();

$(document).ready(function () {
    var table = $('#usersTable').DataTable({
        processing: true,
        responsive: true,
        serverSide: true,
        ajax: {
            url: getApiRootPath() + '/api/users/datatables',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                d.isActive = $('#isActiveFilter').val();

                if (d.order && d.order.length > 0) {
                    var sortIndex = d.order[0].column;
                    var sortDir = d.order[0].dir;
                    var sortBy = (d.columns[sortIndex].data || '').toUpperCase();

                    d.sortBy = sortBy;
                    d.sortDirection = sortDir;
                }

                return JSON.stringify(d);
            }
        },
        dom: ' <"search"f><"top"l>rt<"bottom"ip><"clear">',
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
        initComplete: function () {
            var api = this.api();
            var $tableContainer = $(api.table().container());
            var $toolbar = $('.table-filter-toolbar');

            $tableContainer.find('.dt-paging').appendTo('.card-tools-usersTable-pagination');
            $tableContainer.find('.dt-search').appendTo('.card-tools-usersTable-filter');
            $tableContainer.find('.dt-length').appendTo('.card-tools-usersTable-length');
            $tableContainer.find('.dt-info').appendTo('.card-tools-usersTable-size');

            $toolbar.find('.dt-search').addClass('toolbar-search__control');
            $toolbar.find('.dt-search label').addClass('toolbar-search__label');
            $toolbar.find('.dt-search input').addClass('form-control').attr('type', 'search');
            $toolbar.find('.dt-length select').addClass('form-select');

            document.querySelectorAll('.toolbar-search__label').forEach(function (el) { el.remove(); });
            $('label[for="dt-length-0"]').remove();
        },
        language: {
            lengthMenu: '_MENU_',
            search: '',
            searchPlaceholder: 'Search users...',
            emptyTable: 'No data found.',
            processing: '<div>Loading data please wait...  </div>',
            paginate: {
                previous: '«',
                next: '»'
            }
        },
        columns: [
            {
                data: 'userName',
                render: function (_, __, data) {
                    var status = data.isActive
                        ? '<span class="badge text-bg-success mr-2" style="margin-right:5px">Active</span>'
                        : '<span class="badge text-bg-danger mr-2" style="margin-right:5px">Inactive</span>';

                    var fullName = ((data.firstName || '') + ' ' + (data.lastName || '')).trim();
                    var createdAt = data.createdDate ? moment(data.createdDate).format('MMM DD, YYYY HH:mm') : 'n/a';
                    var relativeModified = moment(data.modifiedDate || data.createdDate).fromNow();

                    return `
<div class="card user-row-card mb-2">
  <div class="card-body">
    <div class="d-lg-flex align-items-center">
      <div class="flex-fill pr-lg-3">
        <div class="d-flex flex-wrap align-items-center mb-1">${status}<strong>${fullName}</strong></div>
        <div class="small text-muted mb-1">User Name: ${data.userName || ''} | User ID: ${data.userId || ''}</div>
        <div class="small text-muted mb-1">Email: ${(data.email || '-')}</div>
        <div class="small text-muted mb-1">Created: ${createdAt}</div>
      </div>
      <div class="request-actions d-flex flex-column align-items-lg-end mt-3 mt-lg-0">
        <div class="mb-1 d-flex gap-2 justify-content-lg-end">
          <a title="Display" href="/Settings/Profile/index/${(data.userId || '')}" class="btn btn-outline-secondary rounded-circle d-inline-flex align-items-center justify-content-center p-0" style="width: 40px; height: 40px;"><i class="bi bi-eye"></i></a>
          <a title="Configure" href="/admin/users/configure/${(data.userId || '')}" class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0" style="width: 40px; height: 40px;"><i class="bi bi-gear"></i></a>
          <a title="Edit" href="/admin/users/edit/${(data.userId || '')}" class="btn btn-outline-dark rounded-circle d-inline-flex align-items-center justify-content-center p-0" style="width: 40px; height: 40px;"><i class="bi bi-pencil"></i></a>
        </div>
        <small class="text-muted">Updated ${relativeModified}</small>
      </div>
    </div>
  </div>
</div>`;
                }
            }
        ]
    });

    $('#isActiveFilter').change(function () {
        table.ajax.reload();
    });
});
