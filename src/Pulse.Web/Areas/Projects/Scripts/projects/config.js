let currentColorHex = '#3b82f6';
let currentIconClass = 'bi bi-rocket-fill';
let hoveredIconClass = null;


const backdrop = document.getElementById('drawerBackdrop');
const btnOpen = document.getElementById('btnRegisterProject');
const btnClose = document.getElementById('btnCloseDrawer');
const btnCancel = document.getElementById('btnCancelDrawer');
const pickr = Pickr.create({
    el: '#btnColorPicker',
    theme: 'nano',
    default: currentColorHex,
    comparison: false,
    position: 'bottom-middle',
    swatches: [
        '#3b82f6',
        '#22c55e',
        '#f97316',
        '#ec4899',
        '#8b5cf6',
        '#eab308',
        '#f43f5e',
        '#0ea5e9'
    ],
    components: {
        preview: true,
        opacity: false,
        hue: true,
        interaction: {
            hex: true,
            input: true,
            save: true,
            clear: false
        }
    }
});



function buildCurrentProjectData() {
    

    const dataproductgroup = $('#productgroupSelect').select2('data');
    const productGroupCode = dataproductgroup[0].id;

    const dataproductdivision = $('#productdivisionSelect').select2('data');
    const productDivisionCode = dataproductdivision[0].id;
 
      
    const title = document.getElementById('projectTitle').value.trim();
    const description = document.getElementById('projectDescription').value.trim();
    const icon = $('#icon').val();
    const iconColor = $('#iconcolor').val();
    const projectNo = $('#projectNo').val();
    const transactionKey = $('#transactionKey').val();
 

    return {
        projectNo,
        transactionKey,
        title,
        description,
        icon,
        iconColor,
        productGroupCode,
        productDivisionCode
    };

}



function openIconModal() {
    iconModalBackdrop.classList.add('show');
    iconSearchInput.value = '';
    renderIconGrid(BOOTSTRAP_ICON_CLASSES);
    markSelectedIcon(currentIconClass);
    updateIconCountLabel(BOOTSTRAP_ICON_CLASSES.length);
    iconSearchInput.focus();
}

function closeIconModal() {
    iconModalBackdrop.classList.remove('show');
}

function renderIconGrid(list) {
    iconGrid.innerHTML = '';
    list.forEach(cls => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'icon-item';
        btn.dataset.iconClass = cls;
        btn.title = cls.replace('bi ', '');
        btn.innerHTML = '<i class="' + cls + '"></i>';
        btn.addEventListener('click', () => {
            hoveredIconClass = cls;
            markSelectedIcon(cls);
        });
        iconGrid.appendChild(btn);
    });
}

function markSelectedIcon(cls) {
    Array.from(iconGrid.querySelectorAll('.icon-item')).forEach(el => {
        el.classList.toggle('selected', el.dataset.iconClass === cls);
    });
}

function updateIconCountLabel(count) {
    iconCountLabel.textContent = count + ' icons';
}

function filterIcons(term) {
    term = term.trim().toLowerCase();
    if (!term) return BOOTSTRAP_ICON_CLASSES;
    return BOOTSTRAP_ICON_CLASSES.filter(c => c.toLowerCase().includes(term));
}

function setIconClass(iconClass) {
    currentIconClass = iconClass;

    document.getElementById('projectIconPreview').className = iconClass;
    document.getElementById('projectIconLabel').textContent = iconClass;
    $('#icon').val(iconClass);
    //const headerIcon = document.getElementById('headerIcon');
    //headerIcon.className = iconClass + ' text-success';

    //const previewIcon = document.getElementById('previewIcon');
    //previewIcon.className = iconClass;






}

function openDrawer(id) {

    const d = {
        draw: 1,
        start: 0,
        length: 1000,
        order: [],
        columns: []
    };

    d.nodetype = 'roadmap';
    d.search = { value: id };


    $.ajax({
        url: API_URL,
        type: 'POST',
        contentType: 'application/json',
        // xhrFields: { withCredentials: true }, //** REMOVED**
        dataType: 'json',
        data: JSON.stringify(d),
        success: function (data) {
            const projects = data.data;
            buildDrawer(id, projects);
        },
        error: function () {
            $('#' + loadingRowId).replaceWith(`
          <tr class="milestone-row ${projectKeyClass}">
            <td></td>
            <td colspan="9" class="text-danger" style="font-size:0.8rem;">
              Error loading milestones for this project.
            </td>
          </tr>
        `);
        }
    });




}

function closeDrawer() {
    var drawer = document.getElementById('drawerRegisterProject');
    drawer.classList.remove('open');
    backdrop.classList.remove('show');
    drawer.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
}

function setPrimaryColor(hex) {
    currentColorHex = hex;

    //document.getElementById('projectColorDot').style.background = hex;
    //document.getElementById('projectColorLabel').textContent = hex;

    //document.getElementById('appearanceChipColorDot').style.background = hex;

    //const headerIcon = document.getElementById('headerIcon');
    //headerIcon.style.color = hex;

    //const previewIcon = document.getElementById('previewIcon');
    //previewIcon.style.color = hex;

    //const previewBadge = document.getElementById('previewBadge');
    //previewBadge.style.backgroundColor = hex + '22';
    //previewBadge.style.borderColor = hex + '55';
    //previewBadge.style.color = '#0b1120';

    $('#iconcolor').val(hex); 
}



$(document).ready(function () {

    // ===== Icon Picker (custom modal with Bootstrap Icons) =====
    const iconModalBackdrop = document.getElementById('iconModalBackdrop');
    const iconGrid = document.getElementById('iconGrid');
    const iconSearchInput = document.getElementById('iconSearchInput');
    const iconCountLabel = document.getElementById('iconCountLabel');
    const iconModalClose = document.getElementById('iconModalClose');
    const iconModalCancel = document.getElementById('iconModalCancel');
    const iconModalApply = document.getElementById('iconModalApply');
    const btnOpenIconPicker = document.getElementById('btnOpenIconPicker');


    // ===== Color Picker (Pickr) =====
    const btnColorPicker = document.getElementById('btnColorPicker');


    $('#productgroupSelect').select2({
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
        width: '100%'
    });

    $('#productdivisionSelect').select2({
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
        placeholder: '-- Select product group --',
        width: '100%'
    });

    pickr.on('save', (color, instance) => {
        const hex = color.toHEXA().toString();
        setPrimaryColor(hex);
        instance.hide();

    });

    pickr.on('init', instance => {
        const initial = instance.getColor().toHEXA().toString();
        setPrimaryColor(initial);
    });




    // ===== Events =====

    btnOpenIconPicker.addEventListener('click', openIconModal);
    iconModalClose.addEventListener('click', closeIconModal);
    iconModalCancel.addEventListener('click', closeIconModal);

    iconModalApply.addEventListener('click', () => {
        if (hoveredIconClass) {
            setIconClass(hoveredIconClass);

        }
        closeIconModal();
    });

    iconModalBackdrop.addEventListener('click', (e) => {
        if (e.target === iconModalBackdrop) {
            closeIconModal();
        }
    });

    iconSearchInput.addEventListener('input', () => {
        const filtered = filterIcons(iconSearchInput.value);
        renderIconGrid(filtered);
        updateIconCountLabel(filtered.length);
        if (filtered.includes(currentIconClass)) {
            markSelectedIcon(currentIconClass);
        }
    });

    // ESC key closes drawer
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && drawer.classList.contains('open')) {
            closeDrawer();
        }
    });




    if (btnOpen) btnOpen.addEventListener('click', openDrawer);
    if (btnClose) btnClose.addEventListener('click', closeDrawer);
    if (btnCancel) btnCancel.addEventListener('click', closeDrawer);
    if (backdrop) backdrop.addEventListener('click', closeDrawer);




    $('#projectForm').validate({
        ignore: [], // include hidden fields if needed (e.g. Select2 hidden inputs)
        rules: {
            projectTitle: {
                required: true,
                maxlength: 200
            },
            projectDescription: {
                required: true,
                maxlength: 2000
            },
            productgroupSelect: { required: true },
            productdivisionSelect: { required: true }
        },
        messages: {
            projectTitle: {
                required: "",
                maxlength: "Title must not exceed 200 characters."
            },
            projectDescription: {
                required: "",
                maxlength: "Description must not exceed 2000 characters."
            },
            productgroupSelect: { required: "" },
            productdivisionSelect: { required: "" }
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            if (element.hasClass('select2-hidden-accessible')) {
                // place error label after the visible Select2 element
                error.insertAfter(element.next('.select2'));
            } else {
                // suppress label for standard Bootstrap inputs (we use .invalid-feedback)
                return;
            }
        },

        highlight: function (element) {
            const $el = $(element);

            $el.addClass('is-invalid').removeClass('is-valid');

            const $feedback = $el.closest('.form-floating').find('.invalid-feedback');
            if ($feedback.length) {
                $feedback.show();
            }

            if ($el.hasClass('select2-hidden-accessible')) {
                const $container = $el.next('.select2');
                $container.find('.select2-selection').addClass('is-invalid');
            }
        },

        unhighlight: function (element) {
            const $el = $(element);

            $el.removeClass('is-invalid').addClass('is-valid');

            const $feedback = $el.closest('.form-floating').find('.invalid-feedback');
            if ($feedback.length) {
                $feedback.hide();
            }

            if ($el.hasClass('select2-hidden-accessible')) {
                const $container = $el.next('.select2');
                $container.find('.select2-selection').removeClass('is-invalid');
            }
        },
    });
});



function buildDrawer(projectno, object) {
    const project = object[0];

    var drawer = document.getElementById('drawerRegisterProject');

    drawer.classList.add('open');
    backdrop.classList.add('show');
    drawer.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';



    currentIconClass = project.projectIcon;
    currentIconColorClass = project.projectIconColor;

    setIconClass(currentIconClass);
    setPrimaryColor(currentIconColorClass);
    pickr.setColor(project.projectIconColor, false);







    var owners = JSON.parse(project.jsonMembers)

    loadAndRenderAvatarGroup({
        dataSource: owners,
        container: `#projectmembers-avatar-group-container`,
        maxVisible: 10,
        avatarSize: 40,
        avatarSpacing: 20,
        label: '...',
        showLabel: false,
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
                    id: item.userid,
                    name: item.firstname + " " + item.lastname,
                    avatarUrl: "https://calwebapps.cal.st.com/ProfilePhoto/" + item.userid + ".jpg"
                };
            });
        },
    });





    // --- Simple scalar fields ---
    document.getElementById('projectNo').textContent = project.projectNo || '';
    document.getElementById('projectTitle').value = project.projectName || '';
    document.getElementById('projectDescription').value = project.projectDescription || '';
    document.getElementById('productcodes').textContent = project.productCodes || '';


    $('#projectNo').val(project.projectNo || '');
    $('#transactionKey').val(project.transactionKey || '');


    // --- Product Group (Select2) --- 
    if (project.productGroupCode) {
        const $obj = $('#productgroupSelect');
        const newOption = new Option(project.productGroup.productGroupName, project.productGroup.productGroupCode, true, true);
        $obj.append(newOption).trigger('change');
    }

    // --- Product Division (Select2) --- 
    if (project.productDivisionCode) {
        const $obj = $('#productdivisionSelect');
        const newOption = new Option(project.productDivision.productDivisionName, project.productDivision.productDivisionCode, true, true);
        $obj.append(newOption).trigger('change');
    }




    ////// focus first field
    ////setTimeout(function () {
    ////    var sel = document.getElementById('nrCategory');
    ////    if (sel) sel.focus();
    ////}, 100);


    document.getElementById('projectForm').addEventListener('submit', function (e) {
        e.preventDefault();

        var payload = buildCurrentProjectData()

        //showLoading('Updating project, please wait...');



        $.ajax({
            url: apiPath + '/api/Projects',
            type: 'PUT',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(payload),
            // xhrFields: { withCredentials: true }, //** REMOVED**
            success: function (result) {
                //hideLoading();


                refreshProjects();

                // optional redirect
                // window.location.href = appPath + '/Projects/Details/' + result.projectId;
            },
            error: function (xhr, status, error) {

                hideLoading();


                if (xhr.status == 401) {
                    window.location.href = '/auth/relogin';
                    return false;
                }

                console.error('Error saving project', xhr);
                let msg = 'Error saving project.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg += '\n' + xhr.responseJSON.message;
                }
                alert(msg);
            }
        });


        closeDrawer();
    });


}
