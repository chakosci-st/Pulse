var appPath = getAppRootPath();
var apiPath = getApiRootPath();
var assignedGroupsMap = {};
var assignedPlantsMap = {};
var selectedUserPlant = null;

function currentUserPayload() {
    return {
        UserId: $('#Id').val(),
        UserName: $('#UserName').val(),
        FirstName: $('#FirstName').val(),
        LastName: $('#LastName').val(),
        Email: $('#Email').val(),
        TransactionKey: $('#TransactionKey').val(),
        IsActive: $('#IsActive').val() === '1'
    };
}

function fetchPageInfo() {
    return $.ajax({
        url: apiPath + '/api/users/' + id,
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            $('#Id').val(response.userId);
            $('#UserId').val(response.userId);
            $('#UserName').val(response.userName || '');
            $('#FirstName').val(response.firstName || '');
            $('#LastName').val(response.lastName || '');
            $('#Email').val(response.email || '');
            $('#TransactionKey').val(response.transactionKey || '');

            var displayName = ((response.firstName || '') + ' ' + (response.lastName || '')).trim();
            $('#labelName').text(displayName || response.userName || response.userId);
            $('#labelDescription').text((response.userName || '') + ' | ' + (response.email || 'no email'));

            var isActive = !!response.isActive;
            $('#IsActive').val(isActive ? '1' : '0');

            if (!isActive) {
                var buttonActive = $('#buttonIsActive');
                buttonActive.html('Activate');
                buttonActive.addClass('btn-outline-success');
                buttonActive.removeClass('btn-outline-secondary');
                buttonActive.attr('data-action', 'activate');
            }
        }
    });
}

function loadAssignedGroups() {
    return $.ajax({
        url: apiPath + '/api/users/' + id + '/groups',
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            assignedGroupsMap = {};
            (response.data || []).forEach(function (row) {
                if (row.userGroupId != null) {
                    assignedGroupsMap[row.userGroupId] = row.userGroupMemberSysId || '';
                }
            });
        }
    });
}

function loadAssignedPlants() {
    return $.ajax({
        url: apiPath + '/api/users/' + id + '/plants',
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            assignedPlantsMap = {};
            (response.data || []).forEach(function (row) {
                assignedPlantsMap[row.plantCode] = row;
            });
        }
    });
}

function linkGroup(userGroupId) {
    var formData = new FormData();
    var payload = {
        userId: id,
        userGroupId: userGroupId
    };

    formData.append('usergroupmember', JSON.stringify(payload));

    return $.ajax({
        url: apiPath + '/api/users/' + id + '/group/link/' + userGroupId,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false
    });
}

function unlinkGroup(userGroupId, membershipId) {
    var formData = new FormData();
    var payload = {
        id: membershipId || '',
        userId: id,
        userGroupId: userGroupId
    };

    formData.append('usergroupmember', JSON.stringify(payload));

    return $.ajax({
        url: apiPath + '/api/users/' + id + '/group/unlink/' + userGroupId,
        type: 'DELETE',
        data: formData,
        processData: false,
        contentType: false
    });
}

function escapeHtml(value) {
    return $('<div/>').text(value == null ? '' : value).html();
}

function getSelectedUserDisplayName() {
    return $.trim($('#labelName').text()) || id;
}

function renderUserPlantAssignments(rows) {
    var body = $('#userPlantAssignmentsTableBody');
    if (!rows || !rows.length) {
        body.html('<tr><td colspan="3" class="text-center text-muted py-4">No user groups found.</td></tr>');
        return;
    }

    var markup = rows.map(function (row) {
        var isChecked = Number(row.isSelected) === 1;
        var isInactive = Number(row.isActive) !== 1;
        return '' +
            '<tr>' +
            '  <td>' +
            '    <div class="fw-semibold">' + escapeHtml(row.userGroupName) + '</div>' +
            '    <div class="text-muted small">ID: ' + escapeHtml(row.userGroupId) + '</div>' +
            '  </td>' +
            '  <td>' +
            '    <div>' + escapeHtml(row.userGroupDescription || 'No description') + '</div>' +
            (isInactive ? '<div class="text-warning small mt-1">Inactive user group</div>' : '') +
            '  </td>' +
            '  <td class="text-center">' +
            '    <div class="form-check form-switch d-inline-flex justify-content-center m-0">' +
            '      <input class="form-check-input js-user-plant-group-toggle" type="checkbox" data-user-group-id="' + escapeHtml(row.userGroupId) + '" data-assignment-id="' + escapeHtml(row.id || '') + '" ' + (isChecked ? 'checked' : '') + '>' +
            '    </div>' +
            '  </td>' +
            '</tr>';
    }).join('');

    body.html(markup);
}

function loadUserPlantAssignments(plantRow) {
    selectedUserPlant = plantRow;
    $('#userPlantAssignmentsSummary').text('Loading assignments for ' + getSelectedUserDisplayName() + ' in ' + (plantRow.plantName || plantRow.plantCode) + '...');
    $('#userPlantAssignmentsTableBody').html('<tr><td colspan="3" class="text-center text-muted py-4">Loading assignments...</td></tr>');

    return $.ajax({
        url: apiPath + '/api/plantusergroupmembers/' + encodeURIComponent(plantRow.plantCode) + '/user/' + encodeURIComponent(id),
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            $('#userPlantAssignmentsSummary').text('Assign plant user groups for ' + getSelectedUserDisplayName() + ' in ' + (plantRow.plantName || plantRow.plantCode) + '.');
            renderUserPlantAssignments(response.data || []);
        },
        error: function () {
            $('#userPlantAssignmentsSummary').text('Unable to load plant user group assignments right now.');
            $('#userPlantAssignmentsTableBody').html('<tr><td colspan="3" class="text-center text-danger py-4">Unable to load assignments.</td></tr>');
        }
    });
}

function allowPlant(plantCode) {
    return $.ajax({
        url: apiPath + '/api/users/' + id + '/plant/link/' + encodeURIComponent(plantCode),
        type: 'POST'
    });
}

function restrictPlant(plantCode) {
    return $.ajax({
        url: apiPath + '/api/users/' + id + '/plant/restrict/' + encodeURIComponent(plantCode),
        type: 'DELETE'
    });
}

$(document).ready(function () {
    var userPlantAssignmentsModalElement = document.getElementById('userPlantAssignmentsModal');
    var userPlantAssignmentsModal = userPlantAssignmentsModalElement
        ? new bootstrap.Modal(userPlantAssignmentsModalElement)
        : null;

    fetchPageInfo().then(function () {
        return $.when(loadAssignedGroups(), loadAssignedPlants());
    }).then(function () {
        $('#tableGroups').DataTable({
            processing: true,
            responsive: true,
            serverSide: false,
            ajax: {
                url: apiPath + '/api/users/groups/all',
                type: 'GET',
                contentType: 'application/json'
            },
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
            language: {
                search: '',
                searchPlaceholder: 'Search...',
                emptyTable: 'No data found.',
                processing: '<div>Loading data please wait...  </div>',
                paginate: {
                    previous: '«',
                    next: '»'
                }
            },
            columns: [
                { data: 'userGroupName' },
                { data: 'userGroupDescription' },
                {
                    data: 'userGroupId',
                    className: 'text-center',
                    render: function (value) {
                        var checked = assignedGroupsMap[value] !== undefined;
                        return '<div class="form-check form-switch d-flex justify-content-center"><input class="form-check-input dt-group-checkbox" type="checkbox" data-usergroup-id="' + value + '" ' + (checked ? 'checked' : '') + '></div>';
                    }
                }
            ]
        });

        $('#tablePlants').DataTable({
            processing: true,
            responsive: true,
            serverSide: false,
            ajax: {
                url: apiPath + '/api/users/' + id + '/plants',
                type: 'GET',
                contentType: 'application/json'
            },
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
            language: {
                search: '',
                searchPlaceholder: 'Search...',
                emptyTable: 'No data found.',
                processing: '<div>Loading data please wait...  </div>',
                paginate: {
                    previous: '«',
                    next: '»'
                }
            },
            columns: [
                {
                    data: 'plantName',
                    render: function (value, type, row) {
                        return '<div class="fw-semibold">' + escapeHtml(value || row.plantCode) + '</div><div class="text-muted small">Code: ' + escapeHtml(row.plantCode || '') + '</div>';
                    }
                },
                {
                    data: 'plantCode',
                    className: 'text-center',
                    render: function (value, type, row) {
                        var checked = Number(row.isSelected) === 1;
                        return '<div class="d-flex flex-column align-items-center gap-2">'
                            + '<label class="float-end form-check-label mb-0"><span class="status-pill ' + (checked ? 'status-pill-on' : 'status-pill-off') + ' status-pill-text">' + (checked ? 'Allowed' : 'Restricted') + '</span></label>'
                            + '<div class="form-check form-switch status-switch m-0">'
                            + '<input class="form-check-input dt-plant-checkbox" type="checkbox" data-plant-code="' + escapeHtml(value) + '" ' + (checked ? 'checked' : '') + '>'
                            + '</div></div>';
                    }
                },
                {
                    data: 'plantCode',
                    className: 'text-center',
                    render: function (value, type, row) {
                        var disabled = Number(row.isSelected) === 1 ? '' : 'disabled';
                        return '<button class="setupplantmember btn btn-sm btn-outline-dark mb-0" data-plant-code="' + escapeHtml(value) + '" ' + disabled + '>Set up</button>';
                    }
                }
            ]
        });
    });

    $('#tableGroups tbody').on('change', '.dt-group-checkbox', function () {
        var $checkbox = $(this);
        var userGroupId = parseInt($checkbox.data('usergroupId'), 10);
        var checked = $checkbox.is(':checked');
        var membershipId = assignedGroupsMap[userGroupId] || '';

        var request = checked ? linkGroup(userGroupId) : unlinkGroup(userGroupId, membershipId);

        request.done(function () {
            if (checked) {
                assignedGroupsMap[userGroupId] = membershipId;
                loadAssignedGroups();
                toastr.success('User group assignment updated.');
            } else {
                delete assignedGroupsMap[userGroupId];
                toastr.success('User group assignment updated.');
            }
        }).fail(function (xhr) {
            $checkbox.prop('checked', !checked);
            if (xhr.responseText && xhr.responseText.indexOf('ORA-00001') > -1) {
                toastr.error('User is already assigned to this group.');
            } else {
                toastr.error('Unable to update group assignment.');
            }
        });
    });

    $('#tablePlants tbody').on('change', '.dt-plant-checkbox', function () {
        var $checkbox = $(this);
        var plantCode = $checkbox.data('plantCode');
        var checked = $checkbox.is(':checked');
        var request = checked ? allowPlant(plantCode) : restrictPlant(plantCode);
        var $row = $checkbox.closest('tr');
        if ($row.hasClass('child')) {
            $row = $row.prev();
        }
        var tablePlants = $('#tablePlants').DataTable();

        request.done(function () {
            toastr.success('Plant access updated.');
            tablePlants.ajax.reload(null, false);
            loadAssignedPlants();
        }).fail(function () {
            $checkbox.prop('checked', !checked);
            toastr.error('Unable to update plant access.');
        });
    });

    $('#tablePlants tbody').on('click', '.setupplantmember', function () {
        var $button = $(this);
        var currentRow = $button.closest('tr');
        if (currentRow.hasClass('child')) {
            currentRow = currentRow.prev();
        }

        var tablePlants = $('#tablePlants').DataTable();
        var plantRow = tablePlants.row(currentRow).data();
        if (!plantRow || Number(plantRow.isSelected) !== 1 || !userPlantAssignmentsModal) {
            toastr.error('Allow the plant first before assigning plant user groups.');
            return;
        }

        loadUserPlantAssignments(plantRow);
        userPlantAssignmentsModal.show();
    });

    $('#userPlantAssignmentsTableBody').on('change', '.js-user-plant-group-toggle', function () {
        if (!selectedUserPlant) {
            return;
        }

        var checkbox = $(this);
        var isChecked = checkbox.is(':checked');
        var payload = {
            plantUserGroupMemberSysId: checkbox.data('assignmentId') || null,
            plantCode: selectedUserPlant.plantCode,
            userId: id,
            userGroupId: Number(checkbox.data('userGroupId'))
        };
        var formData = new FormData();
        formData.append('plantusergroupmember', JSON.stringify(payload));

        checkbox.prop('disabled', true);
        $.ajax({
            url: apiPath + '/api/plantusergroupmembers/' + (isChecked ? 'link' : 'unlink'),
            type: isChecked ? 'POST' : 'DELETE',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                toastr.success('Plant user group assignment updated.');
                loadUserPlantAssignments(selectedUserPlant);
            },
            error: function (xhr) {
                checkbox.prop('checked', !isChecked);
                if (xhr.status === 409) {
                    toastr.error('User group is already assigned for this plant.');
                } else if (xhr.status === 404) {
                    toastr.error('Assignment was not found.');
                } else {
                    toastr.error('Unable to update plant user group assignment.');
                }
            },
            complete: function () {
                checkbox.prop('disabled', false);
            }
        });
    });

    $('#formBasicInfo').validate({
        rules: {
            UserName: { required: true, minlength: 4 },
            FirstName: { required: true },
            LastName: { required: true },
            Email: { email: true }
        },
        messages: {
            UserName: { required: '' },
            FirstName: { required: '' },
            LastName: { required: '' }
        },
        errorElement: 'span',
        errorPlacement: function () {
            $('[id*=-error]').hide();
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function () {
            var formData = new FormData();
            var user = currentUserPayload();
            formData.append('user', JSON.stringify(user));

            $.ajax({
                url: apiPath + '/api/users/' + id,
                type: 'PUT',
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    toastr.success('User is successfully updated!');
                    fetchPageInfo();
                },
                error: function () {
                    toastr.error('Unable to update user.');
                }
            });

            return false;
        }
    });

    $('#formChangeStatus').on('submit', function (e) {
        e.preventDefault();

        var action = $('#buttonIsActive').attr('data-action');
        $('#IsActive').val(action === 'activate' ? '1' : '0');

        var formData = new FormData();
        formData.append('user', JSON.stringify(currentUserPayload()));

        $.ajax({
            url: apiPath + '/api/users/' + id,
            type: 'PUT',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                toastr.success('User status updated.');
                window.location.reload();
            },
            error: function () {
                toastr.error('Unable to update status.');
            }
        });
    });

    $('#formDelete').on('submit', function (e) {
        e.preventDefault();

        var formData = new FormData();
        formData.append('user', JSON.stringify(currentUserPayload()));

        $.ajax({
            url: apiPath + '/api/users/' + id,
            type: 'DELETE',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                toastr.success('User deleted successfully.');
                window.location.href = appPath + '/Admin/Users';
            },
            error: function () {
                toastr.error('Unable to delete user.');
            }
        });
    });
});
