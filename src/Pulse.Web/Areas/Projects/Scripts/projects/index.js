
// ==========================
// CONFIG
// ==========================


const API_URL = getApiRootPath() + '/api/projects/datatables';
const CREATE_DRAFT_STORAGE_KEY = 'projectInitDraft';
const CREATE_PRODUCT_CODES_DRAFT_KEY = 'projectInitProductCodesDraft';

var table;

// ==========================
// HELPERS
// ==========================
function formatPeriod(start, end) {
    if (!start && !end) return '';
    return (start || '') + ' \u2192 ' + (end || '');
}

function mapProjectStatusToLabel(apiStatus) {
        return getPulseStatusText(apiStatus);
}

function mapMilestoneStatusToLabel(apiStatus, targetDate) {
        return getPulseStatusText(apiStatus, { targetDate: targetDate });
}

function projectStatusBadge(apiStatus, targetDate) {
        return getPulseStatusBadge(apiStatus, { targetDate: targetDate });
}

function milestoneStatusBadge(apiStatus, targetDate) {
        return getPulseStatusBadge(apiStatus, { targetDate: targetDate });
}

function progressBar(colorClass, value) {
    const v = Math.max(0, Math.min(100, Number(value) || 0));
    return `
      <div class="d-flex align-items-center gap-2">
        <div class="flex-grow-1">
          <div class="progress">
            <div class="progress-bar ${colorClass}" style="width:${v}%;"></div>
          </div>
        </div>
        <span style="font-size:0.8rem;">${v}%</span>
      </div>`;
}

function milestoneProgressBar(value) {
    const v = Math.max(0, Math.min(100, Number(value) || 0));
    return `
      <div class="d-flex align-items-center gap-2">
        <div class="flex-grow-1">
          <div class="progress">
            <div class="progress-bar bg-primary" style="width:${v}%;"></div>
          </div>
        </div>
        <span class="milestone-progress">${v}%</span>
      </div>`;
}

function projectIcon(name, color) {
    return `<i class="${name} me-1" style="color:${color}"></i>`;
}



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


function refreshProjects() {
    table.ajax.reload(null, false);
}

function getProjectRowByNo(projectNo) {
    if (!table || !projectNo) {
        return null;
    }

    const rows = table.rows().data();
    for (let index = 0; index < rows.length; index++) {
        const row = rows[index];
        if ((row.projectNo || '').toLowerCase() === String(projectNo).toLowerCase()) {
            return row;
        }
    }

    return null;
}

function buildProjectCopyDraft(draft, project) {
    const plantName = project && project.plant ? (project.plant.plantName || project.plant.name || '') : '';
    const categoryName = project && project.category ? (project.category.categoryName || '') : '';
    const categoryCode = (draft && draft.templateCategoryValue) || (project ? project.categoryCode : '') || '';
    const productGroupName = project && project.productGroup ? (project.productGroup.productGroupName || '') : '';
    const productDivisionName = project && project.productDivision ? (project.productDivision.productDivisionName || '') : '';
    const ownerText = [project && project.projectOwnerFirstName, project && project.projectOwnerLastName]
        .filter(Boolean)
        .join(' ')
        .trim();

    return Object.assign({}, draft || {}, {
        siteText: plantName || (draft && draft.siteText) || (project && project.plantCode) || '',
        productgroupText: productGroupName || (draft && draft.productgroupText) || '',
        productdivisionText: productDivisionName || (draft && draft.productdivisionText) || '',
        templateCategoryValue: categoryCode,
        templateCategory: categoryName
            ? `${categoryName}${categoryCode ? ` (${categoryCode})` : ''}`
            : ((draft && draft.templateCategory) || categoryCode || ''),
        ownerText: (draft && draft.ownerText) || ownerText,
        productCodes: []
    });
}

function copyProject(projectNo) {
    const project = getProjectRowByNo(projectNo);
    if (!project) {
        alert('Unable to prepare the selected project copy. Refresh the page and try again.');
        return;
    }

    let existingDraft = null;
    let existingProductCodesDraft = null;
    try {
        existingDraft = localStorage.getItem(CREATE_DRAFT_STORAGE_KEY);
        existingProductCodesDraft = localStorage.getItem(CREATE_PRODUCT_CODES_DRAFT_KEY);
    } catch (error) {
        existingDraft = null;
        existingProductCodesDraft = null;
    }

    const projectLabel = project.projectName || project.projectNo || 'this project';
    const confirmationMessage = existingDraft || existingProductCodesDraft
        ? `Copy ${projectLabel} into a new project draft? This will replace your current unsaved project draft.`
        : `Copy ${projectLabel} into a new project draft? Product codes will not be included.`;

    if (!window.confirm(confirmationMessage)) {
        return;
    }

    $.ajax({
        url: `${getApiRootPath()}/api/projects/${encodeURIComponent(projectNo)}/copy-draft`,
        type: 'GET',
        cache: false
    }).done(function (draft) {
        const mergedDraft = buildProjectCopyDraft(draft, project);

        try {
            localStorage.setItem(CREATE_DRAFT_STORAGE_KEY, JSON.stringify(mergedDraft));
            localStorage.removeItem(CREATE_PRODUCT_CODES_DRAFT_KEY);
        } catch (error) {
            alert('Unable to store the copied project draft in this browser.');
            return;
        }

        window.location.href = '/projects/create';
    }).fail(function (xhr) {
        const message = xhr && xhr.responseJSON && xhr.responseJSON.message
            ? xhr.responseJSON.message
            : 'Unable to copy this project right now.';
        alert(message);
    });
}

function resetAdvancedFilters() {
    const projectNo = document.getElementById('filter-project-no');
    const productCode = document.getElementById('filter-product-code');

    if (projectNo) {
        projectNo.value = "";
    }

    if (productCode) {
        productCode.value = "";
    }

    $('#filter-product-group').val(null).trigger('change');
    $('#filter-product-division').val(null).trigger('change');
    $('#filter-plant').val(null).trigger('change');
    $('#filter-category').val(null).trigger('change');
}

function getStatusScopeLabel() {
    const value = document.getElementById('isActiveFilter')?.value || '';
    if (value === 'true') return 'Active';
    if (value === 'false') return 'Inactive';
    return 'All Projects';
}

function getSortLabel() {
    const sortBy = document.getElementById('sortBy');
    const sortDirection = document.getElementById('sortDirection');

    const sortByText = sortBy ? sortBy.options[sortBy.selectedIndex].text.replace('Sort by: ', '') : 'Project Name';
    const directionText = sortDirection ? sortDirection.options[sortDirection.selectedIndex].text : 'Ascending';

    return `${sortByText} · ${directionText}`;
}

function getAdvancedFilterCount() {
    const filters = [
        document.getElementById('filter-project-no')?.value,
        document.getElementById('filter-product-code')?.value,
        $('#filter-product-group').val(),
        $('#filter-product-division').val(),
        $('#filter-plant').val(),
        $('#filter-category').val()
    ];

    return filters.reduce((count, value) => {
        if (Array.isArray(value)) {
            return count + (value.length ? 1 : 0);
        }

        return count + (value ? 1 : 0);
    }, 0);
}

function syncAdvancedFilterState() {
    const advancedCount = getAdvancedFilterCount();
    const advancedPanel = document.getElementById('advanced-filters');
    const advancedButton = document.getElementById('btn-adv-filters');
    const advancedBadge = document.getElementById('projectsAdvancedBadge');
    const summaryFilters = document.getElementById('projectsSummaryFilters');
    const summaryAdvanced = document.getElementById('projectsSummaryAdvancedCard');
    const workspaceStatus = document.getElementById('projectsWorkspaceStatus');

    if (advancedBadge) {
        advancedBadge.textContent = `${advancedCount} active`;
    }

    if (summaryAdvanced) {
        summaryAdvanced.textContent = String(advancedCount);
    }

    if (summaryFilters) {
        summaryFilters.textContent = advancedCount ? `${advancedCount} advanced filter${advancedCount === 1 ? '' : 's'} applied` : 'No advanced filters';
    }

    if (advancedButton) {
        const isOpen = advancedPanel && !advancedPanel.classList.contains('d-none');
        advancedButton.innerHTML = `<i class="bi bi-sliders"></i> ${isOpen ? 'Hide' : 'Advanced filters'}${advancedCount ? ` <span class="table-toolbar-filter-note">${advancedCount}</span>` : ''}`;
    }

    if (workspaceStatus) {
        workspaceStatus.textContent = advancedCount ? `Focused view · ${advancedCount} filter${advancedCount === 1 ? '' : 's'} active` : 'Ready to browse';
    }
}

function updateProjectsSummary() {
    if (!table) {
        return;
    }

    const pageInfo = table.page.info();
    const visibleCount = table.rows({ page: 'current' }).count();
    const filteredCount = pageInfo ? pageInfo.recordsDisplay : 0;
    const scope = getStatusScopeLabel();
    const sortLabel = getSortLabel();

    const visibleTargets = ['projectsSummaryVisible', 'projectsSummaryVisibleCard'];
    const filteredTargets = ['projectsSummaryFiltered', 'projectsSummaryFilteredCard'];

    visibleTargets.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = String(visibleCount);
        }
    });

    filteredTargets.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = String(filteredCount);
        }
    });

    const scopeTargets = ['projectsSummaryScope', 'projectsSummaryStatusCard'];
    scopeTargets.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = scope;
        }
    });

    const sortElement = document.getElementById('projectsSummarySort');
    if (sortElement) {
        sortElement.textContent = sortLabel;
    }
}

$(document).ready(function () {


    $('.table-filter-toolbar').html(
        `
<div class="table-filter-toolbar__item">
    <a href="/projects/create" type="button" class="btn btn-primary table-toolbar-btn">
            <i class="bi bi-plus-circle"></i> New Project
        </a>
</div>
<div class="table-filter-toolbar__item">
          <select id="sortDirection" class=" form-select">
              <option value="asc" selected>Ascending</option>
              <option value="desc" >Descending</option> 
            </select>
</div>
<div class="table-filter-toolbar__item">
          <select id="sortBy" class=" form-select">
              <option value="projectname" selected>Sort by: Project Name</option>
              <option value="projectno" >Sort by: Project Number</option>
              <option value="plantcode" >Sort by: Plant Code</option>
              <option value="productgroup" >Sort by: Product Group</option>
              <option value="productdivision" >Sort by: Product Division</option>
            </select>
</div>
<div class="table-filter-toolbar__item">
        <select class="form-select " id="isActiveFilter">
            <option value="">All</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
        </select>
</div>

<span class="table-filter-toolbar__item toolbar-length card-tools-projectsTable-length  "></span>
<div class="table-filter-toolbar__item">
         <button class="btn btn-outline-secondary table-toolbar-btn" id="btn-adv-filters">
                <i class="bi bi-sliders"></i> Advanced filters
            </button>
</div> 
<span class="table-filter-toolbar__item toolbar-search card-tools-projectsTable-filter  "></span>
`
    )

    syncAdvancedFilterState();
    updateProjectsSummary();

    const selectIsActiveFilter = document.getElementById('isActiveFilter');
    const params = new URLSearchParams(window.location.search);
    const status = params.get('status');

    if (status === 'active') {
        selectIsActiveFilter.value = 'true';

    }


    table = $('#projectsTable').DataTable({
        "processing": true, "responsive": true,
        "serverSide": true,
        "ajax": {
            "url": API_URL,
            "type": "POST",
            "contentType": "application/json",
            // xhrFields: { withCredentials: true }, //** REMOVED**
            "data": function (d) {
                // Add custom filter
                d.status = selectIsActiveFilter.value === "" ? "ONGOING, COMPLETED, HOLD" : (selectIsActiveFilter.value === "true" ? "ONGOING" : "COMPLETED, HOLD");

                const elementAdvanceFilter = document.querySelector('#advanced-filters');
                if (!elementAdvanceFilter.classList.contains('d-none')) {
                    d.projectNo = document.getElementById('filter-project-no').value;
                    d.productCode = document.getElementById('filter-product-code').value;
                    d.productGroupCode = $('#filter-product-group').val();
                    d.productDivisionCode = $('#filter-product-division').val();
                    d.plantCode = $('#filter-plant').val();
                    d.categoryCode = $('#filter-category').val();
                     
                }


                d.nodetype = "roadmap";
                // Extract sort info
                if (d.order && d.order.length > 0) {
                    var sortIndex = d.order[0].column; // index of sorted column
                    var sortDir = $('#sortDirection').val();      // 'asc' or 'desc'
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

            updateProjectsSummary();
            syncAdvancedFilterState();

            api.rows().every(function () {
                var data = this.data();

                var projectMembers = [];
                try {
                    projectMembers = JSON.parse(data.jsonMembers || '[]');
                } catch (error) {
                    projectMembers = [];
                }

                var ownerMembers = projectMembers.filter(function (member) {
                    var isOwner = member.isowner ?? member.isOwner ?? member.ISOWNER;
                    return isOwner === 1 || isOwner === '1' || isOwner === true;
                });

                var regularMembers = projectMembers.filter(function (member) {
                    var isOwner = member.isowner ?? member.isOwner ?? member.ISOWNER;
                    return !(isOwner === 1 || isOwner === '1' || isOwner === true);
                });



                //var node = this.node();

                loadAndRenderAvatarGroup({
                    dataSource: ownerMembers,
                    container: `#${data.projectNo}-owners-avatar-group-container`,
                    maxVisible: 10,
                    avatarSize: 40,
                    avatarSpacing: 20,
                    label: '...',
                    showLabel: false,
                    emptyText: 'No owners found',
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
                                id: item.userid,
                                name: item.firstname + " " + item.lastname,
                                //avatarUrl: "https://calwebapps.cal.st.com/ProfilePhoto/" + item.userid + ".jpg"
                                avatarUrl: `/Settings/Profile/Photo/${encodeURIComponent(item.userid)}`
                            };
                        });
                    },
                });

                loadAndRenderAvatarGroup({
                    dataSource: regularMembers,
                    container: `#${data.projectNo}-members-avatar-group-container`,
                    maxVisible: 10,
                    avatarSize: 40,
                    avatarSpacing: 20,
                    label: '...',
                    showLabel: false,
                    emptyText: 'No members found',
                    backgroundColor: '#e0f2fe',
                    fontColor: '#075985',
                    labelBackgroundColor: '#607d8b',
                    userInformationUrl: '/Settings/Profile/Index/{id}',
                    userInformationTarget: '_self',
                    labelFontColor: '#fff',
                    sort: 'initials',
                    onMoreClick: function (extraUsers, event) {
                        alert('Show more users:\n' + extraUsers.map(u => u.name).join(', '));
                    },
                    onLabelClick: function (allUsers, event) {
                        alert('All users:\n' + allUsers.map(u => u.name).join(', '));
                    },
                    transform: function (data) {
                        return data.map(function (item) {
                            return {
                                id: item.userid,
                                name: item.firstname + " " + item.lastname,
                                avatarUrl: `/Settings/Profile/Photo/${encodeURIComponent(item.userid)}`
                            };
                        });
                    },
                });
            });
        },
        "initComplete": function () {

            $('.dt-paging').appendTo('.card-tools-projectsTable-pagination');
            $('.dt-search').appendTo('.card-tools-projectsTable-filter');
            $('.dt-length').appendTo('.card-tools-projectsTable-length');

            $('#projectsTable_info').appendTo('.card-tools-projectsTable-size');

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
                "data": "projectNo",
                "orderable": false,
                "render": function (value, type, data) {

                    const project = data;

                    const projectId = project.projectNo;
                    const name = project.projectName;
                    const owner = project.projectOwnerFirstName + " " + project.projectOwnerLastName;
                    const statusRaw = project.status;      // ONGOING / COMPLETE / HOLD ...
                    const icon = project.projectIcon;
                    const iconColor = project.projectIconColor;
                    const statusLabel = mapProjectStatusToLabel(statusRaw, project.targetCompletion);
                    const productCodes = project.productCodes;
                    const projectCount = Number(project.projectCount) || 0;
                    const closedCount = (Number(project.projectCompleteCount) || 0) + (Number(project.projectCancelCount) || 0);
                    const rawPercentCompleted = projectCount > 0 ? (closedCount / projectCount) * 100 : 0;
                    const projPct = Math.max(0, Math.min(100, rawPercentCompleted));

                    const progress = Math.round(projPct);
                    const startDate = moment(project.targetStart).format("YYYY-MM-DD");
                    const endDate = moment(project.targetCompletion).format("YYYY-MM-DD");
                    const projectKeyClass = 'project-' + projectId + '-milestones';
                    const percentCompleted = projPct.toFixed(2);



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

                    var status = projectStatusBadge(data.status, data.targetCompletion);


                    const formattedCreateDate = moment(data.createdDate).format("MMM DD, YYYY HH:mm");
                    const relativeModifiedTime = moment(data.modifiedDate).fromNow();

                    return ` 
<div class="card request-card projects-project-card mb-2">
    <div class="card-body">
        <div class="projects-project-card__header">
            <div class="projects-project-card__identity">
                <div class="projects-project-card__title">
                    <a href="/projects/${projectId}/details" class="btn-list-key text-primary projects-project-card__link">${projectIcon(icon, iconColor)}${name || ''}</a>
                    ${status}
                </div>
                <div class="projects-project-card__codes">${productCodes || 'No product codes listed'}</div>
                <div class="small text-muted mt-2">Project No.: <strong>${projectId}</strong> · Created: ${formattedCreateDate}</div>
            </div>
            <div class="projects-project-card__actions">
${((function () {
    let projectMembers = [];
    try {
        projectMembers = JSON.parse(data.jsonMembers || '[]');
    } catch (error) {
        projectMembers = [];
    }

    const loggedUserId = window.user?.EmployeeId || '';
    const canManageProject = projectMembers.some(member => {
        const memberId = member.userid || member.userId || member.EmployeeId || '';
        return memberId === loggedUserId;
    }) || (data.projectOwnerId || '') === loggedUserId;

                        return `
<a title="Display" href="/projects/${projectId}/details"
                class="btn btn-outline-primary d-inline-flex align-items-center justify-content-center p-0">
    <i class="bi bi-eye"></i>
</a>
<a title="Copy Project" onclick="copyProject('${projectId}'); return false;" href="#"
                class="btn btn-outline-primary d-inline-flex align-items-center justify-content-center p-0">
    <i class="bi bi-copy"></i>
</a>
${canManageProject ? `
<a title="Project Review" href="/projects/${projectId}/review"
        class="btn btn-outline-primary d-inline-flex align-items-center justify-content-center p-0">
  <i class="bi bi-ui-checks"></i>
</a> 
<a title="Quick Edit" onclick="openDrawer('${projectId}'); return false;" href="#"
        class="btn btn-outline-primary d-inline-flex align-items-center justify-content-center p-0">
  <i class="bi bi-pen-fill"></i>
</a> 
<a title="Configure"  href="/projects/${projectId}/configure"
        class="btn btn-outline-primary d-inline-flex align-items-center justify-content-center p-0">
  <i class="bi bi-gear"></i>
</a>` : ''}`;
})())}
            </div>
        </div>
        <div class="projects-project-card__meta-grid">
            <div class="projects-project-card__meta">
                <span class="projects-project-card__label">Project Period</span>
                <div class="projects-project-card__value"><i class="far fa-calendar me-1"></i>${startDate} → ${endDate}</div>
            </div>
            <div class="projects-project-card__meta">
                <span class="projects-project-card__label">Completion</span>
                <div class="projects-project-card__value">${progressBar('bg-success', percentCompleted)}</div>
            </div>
            <div class="projects-project-card__meta">
                <span class="projects-project-card__label">Portfolio Metadata</span>
                <div class="projects-project-card__value projects-project-card__value--muted">Plant: ${data.plant.plantName} - ${data.plantCode}<br>Category: ${data.category.categoryName}<br>Product Group: ${data.productGroup.productGroupName}<br>Product Division: ${data.productDivision.productDivisionName}</div>
            </div>
        </div>
        <div class="projects-project-card__foot">
            <div class="d-flex flex-wrap align-items-start gap-4">
                <div>
                    <div class="small text-muted mb-1">Project Owners</div>
                    <div id="${projectId}-owners-avatar-group-container"></div>
                </div>
                <div>
                    <div class="small text-muted mb-1">Project Members</div>
                    <div id="${projectId}-members-avatar-group-container"></div>
                </div>
            </div>
            <small class="text-muted">⏱ Updated ${relativeModifiedTime}</small>
        </div>
    </div>
</div>
`







                }
            },


        ]
    });


    $('#filter-product-group').select2({
        ajax: {
            url: getApiRootPath() + '/api/productgroups',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                data.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.productGroupName.localeCompare(b.productGroupName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: data.map(item => ({
                        id: item.productGroupCode,
                        text: item.productGroupName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select product group --',
        allowClear: true,
        width: '100%'
    });

    $('#filter-product-division').select2({
        ajax: {
            url: getApiRootPath() + '/api/productdivisions',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                data.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.productDivisionName.localeCompare(b.productDivisionName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: data.map(item => ({
                        id: item.productDivisionCode,
                        text: item.productDivisionName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select product division --',
        allowClear: true,
        width: '100%'
    });

    $('#filter-plant').select2({
        ajax: {
            url: getApiRootPath() + '/api/plants/Allowed',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                data.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.name.localeCompare(b.name, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: data.map(item => ({
                        id: item.plantCode,
                        text: item.plantName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select site --',
        allowClear: true,
        width: '100%'
    });


    $('#filter-category').select2({
        ajax: {
            url: getApiRootPath() + '/api/categories',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                data.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.categoryName.localeCompare(b.productDivisionName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: data.map(item => ({
                        id: item.categoryCode,
                        text: item.categoryName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select category --',
        allowClear: true,
        width: '100%'
    });

    // ======= Event wiring =======
    document.getElementById('btn-adv-filters').addEventListener('click', () => {
        const panel = document.getElementById('advanced-filters');
        panel.classList.toggle('d-none');
        syncAdvancedFilterState();
    });

    document.getElementById('btn-filters-reset').addEventListener('click', () => {
        resetAdvancedFilters();
        syncAdvancedFilterState();
    });

    document.getElementById('btn-filters-apply').addEventListener('click', () => {
        syncAdvancedFilterState();
        table.ajax.reload();
    });

    $('#sortBy').change(function () {
        updateProjectsSummary();
        table.ajax.reload();
    });

    $('#sortDirection').change(function () {
        updateProjectsSummary();
        table.ajax.reload();
    });

    // Redraw table when filter changes
    $('#isActiveFilter').change(function () {
        updateProjectsSummary();
        table.ajax.reload();
    });

    $('#filter-product-group, #filter-product-division, #filter-plant, #filter-category').on('change', syncAdvancedFilterState);
    $('#filter-project-no, #filter-product-code').on('input', syncAdvancedFilterState);
});

 