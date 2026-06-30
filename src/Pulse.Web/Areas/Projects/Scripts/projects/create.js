


 


$('#chips').html(
    `
<span class="project-chip">
    <i class="fas fa-route"></i> Select roadmap template
</span>
<span class="project-chip">
    <i class="far fa-user"></i> Set owner & members
</span>
<span class="project-chip">
    <i class="fas fa-user-check"></i> Assign responsibilities
</span>
`
);

$('#subcontent').html(
    `
<div class="small text-gray-300">Next step</div>
<div class="fw-semibold">
    <i class="fas fa-diagram-project me-1 text-primary"></i>
    Monitoring dashboard
</div>
`
);


var appPath = getAppRootPath();
var apiPath = getApiRootPath();
var firstEmptySelect = true;
var __Template = null;

function normalizeSelect2SearchTerm(value) {
    return (value || '').toString().trim().toLowerCase();
}

function filterSelect2Results(items, params, fieldsResolver) {
    const term = normalizeSelect2SearchTerm(params && params.term);

    if (!term) {
        return items.slice();
    }

    return items.filter(item => {
        const fields = fieldsResolver(item) || [];
        return fields.some(field => normalizeSelect2SearchTerm(field).includes(term));
    });
}

// ------- PROJECT DATA ------- //
const DRAFT_STORAGE_KEY = 'projectInitDraft';

function saveDraft(projectData) {
    try {
        localStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(projectData));
    } catch (e) {
        console.warn('Unable to save draft to localStorage', e);
    }
}

function loadDraft() {
    try {
        const raw = localStorage.getItem(DRAFT_STORAGE_KEY);
        if (!raw) return null;
        return JSON.parse(raw);
    } catch (e) {
        console.warn('Unable to load draft from localStorage', e);
        return null;
    }
}

function clearDraft() {
    try {
        localStorage.removeItem(DRAFT_STORAGE_KEY);
    } catch (e) {
        console.warn('Unable to clear draft from localStorage', e);
    }
}




document.addEventListener('DOMContentLoaded', function () {
    // Appearance data
    //const projectIconClass = currentIconClass;   // from icon picker
    //const projectColor = currentColorHex;        // from Pickr

    // ===== Icon Picker (custom modal with Bootstrap Icons) =====
    const iconModalBackdrop = document.getElementById('iconModalBackdrop');
    const iconGrid = document.getElementById('iconGrid');
    const iconSearchInput = document.getElementById('iconSearchInput');
    const iconCountLabel = document.getElementById('iconCountLabel');
    const iconModalClose = document.getElementById('iconModalClose');
    const iconModalCancel = document.getElementById('iconModalCancel');
    const iconModalApply = document.getElementById('iconModalApply');
    const btnOpenIconPicker = document.getElementById('btnOpenIconPicker');

    let currentIconClass = 'bi bi-rocket-fill';
    let hoveredIconClass = null;

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

    btnOpenIconPicker.addEventListener('click', openIconModal);
    iconModalClose.addEventListener('click', closeIconModal);
    iconModalCancel.addEventListener('click', closeIconModal);

    iconModalApply.addEventListener('click', () => {
        if (hoveredIconClass) {
            setIconClass(hoveredIconClass);
            scheduleDraftSave();
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

    // Initialize icon preview
    setIconClass(currentIconClass);

    // ===== Color Picker (Pickr) =====
    const btnColorPicker = document.getElementById('btnColorPicker');
    let currentColorHex = '#3b82f6';

    function setPrimaryColor(hex) {
        currentColorHex = hex;

        document.getElementById('projectColorDot').style.background = hex;
        document.getElementById('projectColorLabel').textContent = hex;

        //document.getElementById('appearanceChipColorDot').style.background = hex;

        //const headerIcon = document.getElementById('headerIcon');
        //headerIcon.style.color = hex;

        //const previewIcon = document.getElementById('previewIcon');
        //previewIcon.style.color = hex;

        const previewBadge = document.getElementById('previewBadge');
        //previewBadge.style.backgroundColor = hex + '22';
        //previewBadge.style.borderColor = hex + '55';
        //previewBadge.style.color = '#0b1120';

        $('#iconcolor').val(hex);
        
    }



    const pickr = Pickr.create({
        el: '#btnColorPickerMount',
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

    btnColorPicker.addEventListener('click', function () {
        pickr.show();
    });

    pickr.on('save', (color, instance) => {
        const hex = color.toHEXA().toString();
        setPrimaryColor(hex);
        instance.hide();
        scheduleDraftSave();
    });

    pickr.on('init', instance => {
        const initial = instance.getColor().toHEXA().toString();
        setPrimaryColor(initial);
    });


    // --------------------------------------------------------------------
    // Initialize Controls
    // --------------------------------------------------------------------
    const form = document.getElementById('projectInitForm');

    //flatpickr("yearpicker", {
    //    dateFormat: "Y"
    //});

    //flatpickr("#projectendDatePicker", {
    //    dateFormat: "Y"
    //});
    $('.yearpicker').yearpicker({ autoHide: true, });



    $('#projectownerSelect').select2({
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
                    obj.id = obj.userName;
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
            ////if (data.firstName != undefined) {
            ////    $(data.element).attr('data-attr-username', data.userName);
            ////    $(data.element).attr('data-attr-userid', data.id);
            ////    $(data.element).attr('data-attr-firstname', data.firstName);
            ////    $(data.element).attr('data-attr-lastname', data.lastName);
            ////    $(data.element).attr('data-attr-email', data.email);
            ////}
            ////else {
            ////    return "";
            ////}


            if (!data.id) return data.text || '';
            if (data.firstName == undefined) return data.text;
            return data.firstName + " " + data.lastName;
        },
        matcher: matchCustom
    });

    $('#projectmemberSelect').select2({
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
                    obj.id = obj.userName;
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

            if (data.id) {
                $(data.element).attr('data-attr-username', data.userName);
                $(data.element).attr('data-attr-userid', data.id);
                $(data.element).attr('data-attr-firstname', data.firstName);
                $(data.element).attr('data-attr-lastname', data.lastName);
                $(data.element).attr('data-attr-email', data.email);
            }
            else {
                return "";
            }
            if (data.firstName == undefined) return data.text;

            return data.firstName + " " + data.lastName;
        },
        matcher: matchCustom
    });

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
            processResults: function (data, params) {
                const filteredData = filterSelect2Results(data || [], params, function (item) {
                    return [
                        item.productGroupCode,
                        item.productGroupName
                    ];
                });

                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                filteredData.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.productGroupName.localeCompare(b.productGroupName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: filteredData.map(item => ({
                        id: item.productGroupCode,
                        text: item.productGroupName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select product division --',
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
            processResults: function (data, params) {
                const filteredData = filterSelect2Results(data || [], params, function (item) {
                    return [
                        item.productDivisionCode,
                        item.productDivisionName
                    ];
                });

                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                filteredData.sort(function (a, b) {
                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return a.productDivisionName.localeCompare(b.productDivisionName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: filteredData.map(item => ({
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

    $('#siteSelect').select2({
        ajax: {
            url: getApiRootPath() + '/api/plants/Allowed',
            dataType: 'json',
            // xhrFields: { withCredentials: true }, //** REMOVED**
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data, params) {
                const filteredData = filterSelect2Results(data || [], params, function (item) {
                    return [
                        item.plantCode,
                        item.plantName,
                        item.name
                    ];
                });

                // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                filteredData.sort(function (a, b) {
                    const aPlantName = a.plantName || a.name || '';
                    const bPlantName = b.plantName || b.name || '';

                    // Sort by isActive descending (1 first)
                    if (a.isActive !== b.isActive) {
                        return b.isActive - a.isActive;
                    }
                    // If isActive is the same, sort by name (case-insensitive)
                    return aPlantName.localeCompare(bPlantName, undefined, { sensitivity: 'base' });
                });


                // Transform the API response to Select2 format
                return {
                    results: filteredData.map(item => ({
                        id: item.plantCode,
                        text: item.plantName + (item.isActive == 0 ? " (In Active)" : ""),
                        disabled: item.isActive == 0
                    }))
                };
            },
            cache: true
        },
        placeholder: '-- Select site --',
        width: '100%'
    });

    function resetTemplateSelectionState() {
        $('#templateDescription').html('Select a template to see its structure.');
        $('#templateCategory').html('Select a template to see its category.');
        $('#templateCategoryValue').val('');
        $('#templatePlantRoadmapLinkSysId').val('');
        $('#templateJson').val('');
    }

    function showInlineLoadingStatus(message) {
        const $status = $('#projectCreateLoadingStatus');
        if (!$status.length) {
            return;
        }

        $status.removeClass('d-none');
        $status.find('[data-loading-text]').text(message || 'Loading...');
    }

    function hideInlineLoadingStatus() {
        const $status = $('#projectCreateLoadingStatus');
        if (!$status.length) {
            return;
        }

        $status.addClass('d-none');
        $status.find('[data-loading-text]').text('Loading...');
    }

    function populateTemplates(plantCode) {
        const $templateSelect = $('#templateSelect');

        if ($templateSelect.hasClass('select2-hidden-accessible')) {
            $templateSelect.select2('destroy');
        }

        $templateSelect.html('<option></option>').val(null);
        resetTemplateSelectionState();

        if (!plantCode) {
            $templateSelect.prop('disabled', true).select2({
                placeholder: '-- Select roadmap template --',
                width: '100%'
            });
            return;
        }

        $templateSelect.prop('disabled', false);

        $templateSelect.select2({
            ajax: {
                url: getApiRootPath() + `/api/plantroadmaplinks/${plantCode}`,
                dataType: 'json',
                transport: function (params, success, failure) {
                    showInlineLoadingStatus('Loading template list...');
                    const request = $.ajax(params);
                    request.then(success);
                    request.fail(failure);
                    request.always(function () {
                        hideInlineLoadingStatus();
                    });
                    return request;
                },
                data: function (params) {
                    return {
                        q: params.term
                    };
                },
                processResults: function (data, params) {
                    // Sort: isActive first (1 before 0), then by categoryName (case-insensitive)
                    var _data = filterSelect2Results((data && data.data) || [], params, function (item) {
                        return [
                            item.plantCode,
                            item.roadmap && item.roadmap.roadmapName,
                            item.roadmap && item.roadmap.categoryCode,
                            item.roadmap && item.roadmap.roadmapDescription,
                            item.roadmap && item.roadmap.category && item.roadmap.category.categoryName
                        ];
                    });

                    _data.sort(function (a, b) {
                        // Sort by isActive descending (1 first)
                        if (a.isActive !== b.isActive) {
                            return b.isActive - a.isActive;
                        }
                        // If isActive is the same, sort by name (case-insensitive)
                        return a.roadmap.roadmapName.localeCompare(b.roadmap.roadmapName, undefined, { sensitivity: 'base' });
                    });

                    //if (data.plantRoadmapLinkSysId) {
                    //    $(data.element).attr('data-attr-roadmapsysid', item.roadmapSysId);
                    //    $(data.element).attr('data-attr-plantroadmaplinksysid', item.plantRoadmapLinkSysId);
                    //    $(data.element).attr('data-attr-categorycode', item.roadmap.categoryCode);
                    //    $(data.element).attr('data-attr-categoryname', item.roadmap.categoryName);
                    //    $(data.element).attr('data-attr-roadmapdescription', data.roadmapDescription); 
                    //}
                    //else {
                    //    return "";
                    //}


                    // Transform the API response to Select2 format
                    return {
                        results: _data.map(item => ({
                            id: item.roadmapSysId,
                            text: item.roadmap.roadmapName + " - " + item.roadmap.categoryCode + (item.isActive == 0 ? " (In Active)" : ""),
                            plantRoadmapLinkSysId: item.plantRoadmapLinkSysId,
                            roadmapSysId: item.roadmapSysId,
                            roadmapDescription: item.roadmap.roadmapDescription,
                            categoryCode: item.roadmap.categoryCode,
                            categoryName: item.roadmap.category.categoryName,
                            plantCode: item.plantCode,
                            disabled: item.isActive == 0
                        }))
                    };
                },
                cache: true
            },
            placeholder: '-- Select roadmap template --',
            width: '100%'
        });


    }


    //GET JSON STRUCTURE
    async function getJsonStructure(plantCode, roadmapsysid) {
        showInlineLoadingStatus('Loading template structure and populating milestones/tasks...');
        try {
            const jsonString = await getDataAsync(getApiRootPath() + `/api/plantroadmaplinks/${plantCode}/roadmaps/${roadmapsysid}/treemap`); // getDataAsync returns a Promise
            if (jsonString) {
                $('#templateJson').val(jsonString);
                //RENDER
                renderTemplate(jsonString)
                scheduleDraftSave();
            }
        } catch (err) {
            console.warn('getJsonStructure: unable to load roadmap treemap.', err);
            if (window.toastr) {
                window.toastr.error('Unable to load template structure for this roadmap.');
            }
        } finally {
            hideInlineLoadingStatus();
        }
    }

    $('#siteSelect').on('select2:select', function (e) {
        var selectedData = e.params.data;
        scheduleDraftSave();
        populateTemplates(selectedData.id);

    });

    $('#siteSelect').on('select2:clear', function () {
        scheduleDraftSave();
        populateTemplates('');
    });

    populateTemplates($('#siteSelect').val());

    $('#templateSelect').on('select2:select', function (e) {
        var selectedData = e.params.data;
        var categoryCode = selectedData.categoryCode || (selectedData.roadmap && selectedData.roadmap.categoryCode) || '';
        var categoryName = selectedData.categoryName || (selectedData.roadmap && selectedData.roadmap.category && selectedData.roadmap.category.categoryName) || '';
        $('#templateDescription').html(selectedData.roadmapDescription);
        $('#templateCategory').html(categoryName + (categoryCode ? " (" + categoryCode + ")" : ""));
        $('#templateCategoryValue').val(categoryCode);

        if (selectedData.plantRoadmapLinkSysId)
            $('#templatePlantRoadmapLinkSysId').val(selectedData.plantRoadmapLinkSysId);


        getJsonStructure(selectedData.plantCode, selectedData.roadmapSysId);
    });



    function buildCurrentProjectData() {
        const datasite = $('#siteSelect').select2('data');
        var siteValue = "";
        var siteText = "";
        var ownerData;
        if (datasite.length) {
            siteValue = datasite[0].id;
            siteText = datasite[0].text;
        }

        const datatemplate = $('#templateSelect').select2('data');
        var templateValue = "";
        var templateText = "";
        if (datatemplate.length) {
            templateValue = datatemplate[0].id;
            templateText = datatemplate[0].text;
        }

        const dataprojectowner = $('#projectownerSelect').select2('data');
        var ownerValue = "";
        var ownerText = "";
        if (dataprojectowner.length) {
            ownerValue = dataprojectowner[0].id;
            ownerText = dataprojectowner[0].text;
            ownerData = dataprojectowner[0];
        }


        const dataproductgroup = $('#productgroupSelect').select2('data');
        var productgroupValue = "";
        var productgroupText = "";
        if (dataproductgroup.length) {
            productgroupValue = dataproductgroup[0].id;
            productgroupText = dataproductgroup[0].text;
        }

        const dataproductdivision = $('#productdivisionSelect').select2('data');
        var productdivisionValue = "";
        var productdivisionText = "";
        if (dataproductdivision.length) {
            productdivisionValue = dataproductdivision[0].id;
            productdivisionText = dataproductdivision[0].text;
        }


        const title = document.getElementById('projectTitle').value.trim();
        const description = document.getElementById('projectDescription').value.trim();
        const icon = $('#icon').val();
        const iconcolor = $('#iconcolor').val();
        const projectstartYear = projectstartDatePicker.value.trim();
        const projectstartWorkWeek = targetstartworkweekSelect.value.trim();
        const projectendYear = projectendDatePicker.value.trim();
        const projectendWorkWeek = targetendworkweekSelect.value.trim();
        const memberNames = getAllAssignableMembers();
        const selectedTemplateData = $('#templateSelect').select2('data');
        const selectedTemplate = selectedTemplateData.length ? selectedTemplateData[0] : {};
        const templateCategory = $('#templateCategory').html();
        const templateCategoryValue = $('#templateCategoryValue').val()
            || selectedTemplate.categoryCode
            || (selectedTemplate.roadmap && selectedTemplate.roadmap.categoryCode)
            || '';
        const templateDescription = $('#templateDescription').html();
        const templateJson = $('#templateJson').val();
        const templatePlantRoadmapLinkSysId = $('#templatePlantRoadmapLinkSysId').val();
        let resultMilestones = [];

        if (__Template) {
            resultMilestones = __Template.milestones.map((ms, msIndex) => {
                const msWidget = milestonesContainer.querySelector(
                    `.ms-owner-widget [data-owner-widget="true"][data-milestone-index="${msIndex}"]`
                );
                let msOwners = [];
                if (msWidget) {
                    const hidden = msWidget.querySelector('.owners-hidden');
                    msOwners = hidden ? JSON.parse(hidden.value || '[]') : [];
                }

                const msStartYearInput = milestonesContainer.querySelector(
                    `input[data-type="ms-startyy"][data-milestone-index="${msIndex}"]`
                );
                const msEndYearInput = milestonesContainer.querySelector(
                    `input[data-type="ms-endyy"][data-milestone-index="${msIndex}"]`
                );
                const msStartWwSelect = milestonesContainer.querySelector(
                    `select[data-type="ms-startww"][data-milestone-index="${msIndex}"]`
                );
                const msEndWwSelect = milestonesContainer.querySelector(
                    `select[data-type="ms-endww"][data-milestone-index="${msIndex}"]`
                );

                const tasks = ms.tasks.map((t, tIndex) => {
                    const tWidget = milestonesContainer.querySelector(
                        `.task-owner-widget [data-owner-widget="true"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    let tOwners = [];
                    if (tWidget) {
                        const hidden = tWidget.querySelector('.owners-hidden');
                        tOwners = hidden ? JSON.parse(hidden.value || '[]') : [];
                    }

                    const tStartYearInput = milestonesContainer.querySelector(
                        `input[data-type="task-startyy"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    const tEndYearInput = milestonesContainer.querySelector(
                        `input[data-type="task-endyy"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    const tStartWwSelect = milestonesContainer.querySelector(
                        `select[data-type="task-startww"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    const tEndWwSelect = milestonesContainer.querySelector(
                        `select[data-type="task-endww"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );

                    return {
                        name: t.name,
                        owners: tOwners,
                        startDate: tStartYearInput ? tStartYearInput.value : '',
                        endDate: tEndYearInput ? tEndYearInput.value : '',
                        startWeek: tStartWwSelect ? tStartWwSelect.value : '',
                        endWeek: tEndWwSelect ? tEndWwSelect.value : '',
                        meta: t.meta || null
                    };
                });

                return {
                    name: ms.name,
                    owners: msOwners,
                    startDate: msStartYearInput ? msStartYearInput.value : '',
                    endDate: msEndYearInput ? msEndYearInput.value : '',
                    startWeek: msStartWwSelect ? msStartWwSelect.value : '',
                    endWeek: msEndWwSelect ? msEndWwSelect.value : '',
                    tasks,
                    meta: ms.meta || null
                };
            });
        }


        return {
            siteValue,
            siteText,
            templateValue,
            templateText,
            templateDescription,
            templateCategory,
            templateCategoryValue,
            categoryCode: templateCategoryValue,
            templateJson: templateJson,
            title,
            description,
            icon,
            iconcolor,
            ownerValue,
            ownerText,
            ownerData,
            productgroupValue,
            productgroupText,
            productdivisionValue,
            productdivisionText,
            projectstartYear,
            projectstartWorkWeek,
            projectendYear,
            projectendWorkWeek,
            templatePlantRoadmapLinkSysId,
            members: memberNames.map(n => ({ name: n })),
            milestones: resultMilestones
        };

    }


    function populateWorkWeek(obj) {
        for (let y = 1; y <= 52; y++) {
            const val = y.toString().padStart(2, "0");

            const opt = document.createElement("option");
            opt.value = val;
            opt.textContent = val;

            obj.appendChild(opt);
        }
    }

    document.querySelectorAll(".workweekSelect-root").forEach(select => {
        populateWorkWeek(select)
    });

    // --------------------------------------------------------------------
    // MEMBERS & OWNER-CHIPS WIDGET (unchanged from your original)
    // --------------------------------------------------------------------
    const templateSelect = document.getElementById('templateSelect');
    const milestonesContainer = document.getElementById('milestonesContainer');
    const membersListContainer = document.getElementById('membersList');
    const memberNameInput = document.getElementById('projectmemberSelect');
    const btnAddMember = document.getElementById('btnAddMember');
    const projectOwnerInput = document.getElementById('projectownerSelect');
    const btnExpandAllTaskDetails = document.getElementById('btnExpandAllTaskDetails');
    const btnCollapseAllTaskDetails = document.getElementById('btnCollapseAllTaskDetails');

    // ------- MEMBERS ------- //

    let members = []; // {name}

    function getAllAssignableMembers() {
        const data = $('#projectownerSelect').select2('data')[0] || {};
        const unique = new Set();

        if (data.id) {
            const owner = data.firstName
                ? `${data.firstName} ${data.lastName} (${data.userName})`
                : [data.text || '', data.id ? `(${data.id})` : ''].join(' ').trim();

            if (owner) {
                unique.add(owner);
            }
        }

        members.forEach(m => unique.add(m.name));

        return Array.from(unique);
    }

    function getOwnerDisplayMeta(name) {
        const fullLabel = (name || '').trim();
        const baseLabel = fullLabel.split('(')[0].trim();
        const parts = baseLabel.split(/\s+/).filter(Boolean);
        const initials = parts.slice(0, 2).map(part => part.charAt(0).toUpperCase()).join('') || '?';
        const shortLabel = parts.length > 1
            ? `${parts[0]} ${parts[parts.length - 1].charAt(0).toUpperCase()}.`
            : (parts[0] || fullLabel || 'Unknown owner');

        return {
            fullLabel,
            shortLabel,
            initials
        };
    }

    function parseOwnerSelections(hiddenField) {
        if (!hiddenField) {
            return [];
        }

        try {
            const parsed = JSON.parse(hiddenField.value || '[]');
            return Array.isArray(parsed) ? parsed : [];
        } catch (e) {
            return [];
        }
    }

    function renderOwnerChips(inputArea, selected, onRemove) {
        inputArea.innerHTML = '';

        if (!selected.length) {
            const placeholder = document.createElement('span');
            placeholder.className = 'owners-placeholder';
            placeholder.textContent = 'Select owner(s)...';
            inputArea.appendChild(placeholder);
            return;
        }

        selected.slice(0, 2).forEach(name => {
            const meta = getOwnerDisplayMeta(name);
            const chip = document.createElement('span');
            chip.className = 'owners-chip';

            const avatar = document.createElement('span');
            avatar.className = 'owners-chip__avatar';
            avatar.textContent = meta.initials;

            const label = document.createElement('span');
            label.className = 'owners-chip__label';
            label.textContent = meta.shortLabel;
            label.title = meta.fullLabel;

            const button = document.createElement('button');
            button.type = 'button';
            button.title = `Remove ${meta.fullLabel}`;
            button.innerHTML = '<i class="fas fa-times"></i>';
            button.addEventListener('click', ev => {
                ev.stopPropagation();
                onRemove(name);
            });

            chip.appendChild(avatar);
            chip.appendChild(label);
            chip.appendChild(button);
            inputArea.appendChild(chip);
        });

        if (selected.length > 2) {
            const overflowChip = document.createElement('span');
            overflowChip.className = 'owners-chip owners-chip--overflow';
            overflowChip.textContent = `+${selected.length - 2} more`;
            inputArea.appendChild(overflowChip);
        }
    }

    function renderOwnerDropdownItems(dropdown, selected, onToggle) {
        const allMembers = getAllAssignableMembers();
        dropdown.innerHTML = '';

        if (!allMembers.length) {
            const empty = document.createElement('div');
            empty.className = 'owners-item';
            empty.textContent = 'No members available';
            dropdown.appendChild(empty);
            return;
        }

        allMembers.forEach(name => {
            const meta = getOwnerDisplayMeta(name);
            const isSelected = selected.has(name);
            const item = document.createElement('div');
            item.className = 'owners-item' + (isSelected ? ' active-owner' : '');

            const nameWrap = document.createElement('div');
            nameWrap.className = 'owners-item-name';

            const avatar = document.createElement('span');
            avatar.className = 'owners-item-avatar';
            avatar.textContent = meta.initials;

            const textWrap = document.createElement('span');
            textWrap.className = 'owners-item-text';

            const primary = document.createElement('strong');
            primary.textContent = meta.shortLabel;
            primary.title = meta.fullLabel;

            const secondary = document.createElement('small');
            secondary.textContent = meta.fullLabel;

            textWrap.appendChild(primary);
            textWrap.appendChild(secondary);
            nameWrap.appendChild(avatar);
            nameWrap.appendChild(textWrap);

            const check = document.createElement('div');
            check.className = 'owners-item-check';
            if (isSelected) {
                check.innerHTML = '<i class="fas fa-check"></i>';
            }

            item.appendChild(nameWrap);
            item.appendChild(check);
            item.addEventListener('click', ev => {
                ev.stopPropagation();
                onToggle(name);
            });

            dropdown.appendChild(item);
        });
    }

    function syncOwnerWidgetDisplay(widget) {
        const hidden = widget.querySelector('.owners-hidden');
        const inputArea = widget.querySelector('.owners-input');
        const dropdown = widget.querySelector('.owners-dropdown');

        if (!hidden || !inputArea || !dropdown) {
            return;
        }

        const selected = parseOwnerSelections(hidden);
        renderOwnerChips(inputArea, selected, name => {
            const nextSelected = parseOwnerSelections(hidden).filter(item => item !== name);
            hidden.value = JSON.stringify(nextSelected);
            syncOwnerWidgetDisplay(widget);
            refreshMilestoneSummaries();
            scheduleDraftSave();
        });

        renderOwnerDropdownItems(dropdown, new Set(selected), name => {
            const nextSelected = new Set(parseOwnerSelections(hidden));
            if (nextSelected.has(name)) {
                nextSelected.delete(name);
            } else {
                nextSelected.add(name);
            }

            hidden.value = JSON.stringify(Array.from(nextSelected));
            syncOwnerWidgetDisplay(widget);
            refreshMilestoneSummaries();
            scheduleDraftSave();
        });
    }

    function renderMembers() {
        membersListContainer.innerHTML = '';
        members.forEach((m, idx) => {
            const pill = document.createElement('span');
            pill.className = 'member-pill';
            pill.innerHTML = `
          <i class="far fa-user"></i>${m.name}
          <button type="button" data-index="${idx}" title="Remove">
            <i class="fas fa-times"></i>
          </button>
        `;
            membersListContainer.appendChild(pill);
        });

        membersListContainer.querySelectorAll('button[data-index]').forEach(btn => {
            btn.addEventListener('click', function () {
                const i = parseInt(this.dataset.index, 10);
                members.splice(i, 1);
                renderMembers();
                refreshAllOwnerWidgets();
            });
        });

        refreshAllOwnerWidgets();
    }

    btnAddMember.addEventListener('click', async function () {
        const data = $('#projectmemberSelect').select2('data')[0] || {};
        const memberLabel = data.firstName
            ? `${data.firstName} ${data.lastName}`
            : (data.text || '').trim();
        const memberUserName = data.userName || data.id || '';
        const name = memberUserName ? `${memberLabel} (${memberUserName})` : memberLabel;

        if (!name) return;
        if (members.some(m => m.name.toLowerCase() === name.toLowerCase())) {
            memberNameInput.value = '';
            return;
        }

        showInlineLoadingStatus('Adding member...');
        try {
            members.push({ name });
            memberNameInput.value = '';
            $('#projectmemberSelect').val(null).trigger('change');
            renderMembers();
            scheduleDraftSave();
        } finally {
            hideInlineLoadingStatus();
        }

    });



    // ------- OWNER CHIP SELECTOR WIDGET ------- //
    function createOwnerWidget(initialSelected = []) {
        const wrap = document.createElement('div');
        wrap.className = 'owners-select-wrapper';
        wrap.dataset.ownerWidget = 'true';

        wrap.innerHTML = `
            <div class="owners-input">
              <span class="owners-placeholder">Select owner(s)...</span>
            </div>
            <span class="owners-caret"><i class="fas fa-caret-down"></i></span>
            <div class="owners-dropdown d-none"></div>
            <input type="hidden" class="owners-hidden" value="${JSON.stringify(initialSelected)}">
            <div class="owners-helper">Click to select; click again to remove.</div>
          `;

        const inputArea = wrap.querySelector('.owners-input');
        const dropdown = wrap.querySelector('.owners-dropdown');
        const hidden = wrap.querySelector('.owners-hidden');

        function toggleDropdown(show) {
            if (show) {
                dropdown.classList.remove('d-none');
                inputArea.classList.add('focused');
                syncOwnerWidgetDisplay(wrap);

                // Flip up if not enough space below
                const rect = wrap.getBoundingClientRect();
                const dropdownHeight = Math.min(dropdown.scrollHeight, 220); // max-height
                const spaceBelow = window.innerHeight - rect.bottom;
                const spaceAbove = rect.top;

                if (spaceBelow < dropdownHeight && spaceAbove > dropdownHeight) {
                    // open upwards
                    dropdown.style.top = 'auto';
                    dropdown.style.bottom = '60%';
                    dropdown.style.marginTop = '0';
                    dropdown.style.marginBottom = '3px';
                } else {
                    // default: open downwards
                    dropdown.style.top = '60%';
                    dropdown.style.bottom = 'auto';
                    dropdown.style.marginTop = '3px';
                    dropdown.style.marginBottom = '0';
                }

            } else {
                dropdown.classList.add('d-none');
                inputArea.classList.remove('focused');
            }
        }

        inputArea.addEventListener('click', (ev) => {
            ev.stopPropagation();
            const visible = !dropdown.classList.contains('d-none');
            toggleDropdown(!visible);
        });

        dropdown.addEventListener('click', ev => {
            ev.stopPropagation();
        });

        document.addEventListener('click', () => {
            toggleDropdown(false);
        });

        // Initial render
        hidden.value = JSON.stringify(Array.isArray(initialSelected) ? initialSelected : []);
        syncOwnerWidgetDisplay(wrap);

        return wrap;
    }

    function refreshAllOwnerWidgets() {
        const widgets = milestonesContainer.querySelectorAll('[data-owner-widget="true"]');
        widgets.forEach(syncOwnerWidgetDisplay);
        refreshMilestoneSummaries();
    }

    function formatOwnerSummary(count) {
        if (!count) {
            return 'No owners';
        }

        return count === 1 ? '1 owner' : `${count} owners`;
    }

    function formatYearWeekSummary(year, workWeek) {
        const parts = [];
        if (year) {
            parts.push(year);
        }
        if (workWeek) {
            parts.push(`WW${workWeek}`);
        }

        return parts.join(' ');
    }

    function formatDateRangeSummary(startYear, startWeek, endYear, endWeek) {
        const startLabel = formatYearWeekSummary(startYear, startWeek);
        const endLabel = formatYearWeekSummary(endYear, endWeek);

        if (startLabel && endLabel) {
            return `${startLabel} -> ${endLabel}`;
        }
        if (startLabel) {
            return `Starts ${startLabel}`;
        }
        if (endLabel) {
            return `Ends ${endLabel}`;
        }

        return 'Dates pending';
    }

    function setTaskSummaryTone(chip, tone) {
        if (!chip) {
            return;
        }

        chip.classList.remove('task-summary-chip--ok', 'task-summary-chip--warning');
        if (tone === 'ok') {
            chip.classList.add('task-summary-chip--ok');
        } else if (tone === 'warning') {
            chip.classList.add('task-summary-chip--warning');
        }
    }

    function setMilestoneSummaryTone(chip, tone) {
        if (!chip) {
            return;
        }

        chip.classList.remove('milestone-summary-chip--ok', 'milestone-summary-chip--warning', 'milestone-summary-chip--neutral');
        chip.classList.add(`milestone-summary-chip--${tone}`);
    }

    function refreshMilestoneSummaries() {
        const milestoneRows = milestonesContainer.querySelectorAll('.milestone-row');

        milestoneRows.forEach(row => {
            const taskRows = row.querySelectorAll('.task-row');
            let assignedTaskCount = 0;
            let tasksMissingDates = 0;
            let requiredUnowned = 0;

            taskRows.forEach(taskRow => {
                const selectedOwners = parseOwnerSelections(taskRow.querySelector('.task-owner-widget .owners-hidden'));
                const ownerCount = selectedOwners.length;
                const ownerChip = taskRow.querySelector('[data-role="task-owner-chip"]');
                const ownerText = taskRow.querySelector('[data-role="task-owner-text"]');

                if (ownerText) {
                    ownerText.textContent = formatOwnerSummary(ownerCount);
                }
                setTaskSummaryTone(ownerChip, ownerCount > 0 ? 'ok' : 'warning');

                if (ownerCount > 0) {
                    assignedTaskCount += 1;
                }

                const startYear = taskRow.querySelector('input[data-type="task-startyy"]')?.value.trim() || '';
                const startWeek = taskRow.querySelector('select[data-type="task-startww"]')?.value.trim() || '';
                const endYear = taskRow.querySelector('input[data-type="task-endyy"]')?.value.trim() || '';
                const endWeek = taskRow.querySelector('select[data-type="task-endww"]')?.value.trim() || '';
                const hasCompleteDates = !!startYear && !!startWeek && !!endYear && !!endWeek;
                const dateChip = taskRow.querySelector('[data-role="task-date-chip"]');
                const dateText = taskRow.querySelector('[data-role="task-date-text"]');

                if (dateText) {
                    dateText.textContent = formatDateRangeSummary(startYear, startWeek, endYear, endWeek);
                }
                setTaskSummaryTone(dateChip, hasCompleteDates ? 'ok' : 'warning');

                if (!hasCompleteDates) {
                    tasksMissingDates += 1;
                }

                if (taskRow.dataset.required === 'true' && ownerCount === 0) {
                    requiredUnowned += 1;
                }
            });

            const taskCount = taskRows.length;
            const assignmentChip = row.querySelector('[data-role="milestone-assignment-chip"]');
            const assignmentText = row.querySelector('[data-role="milestone-assignment-text"]');
            const dateChip = row.querySelector('[data-role="milestone-date-chip"]');
            const dateText = row.querySelector('[data-role="milestone-date-text"]');
            const requiredChip = row.querySelector('[data-role="milestone-required-chip"]');
            const requiredText = row.querySelector('[data-role="milestone-required-text"]');

            if (assignmentText) {
                assignmentText.textContent = taskCount === 0 ? 'No tasks' : `${assignedTaskCount}/${taskCount} assigned`;
            }
            setMilestoneSummaryTone(
                assignmentChip,
                taskCount === 0 ? 'neutral' : (assignedTaskCount === taskCount ? 'ok' : (assignedTaskCount > 0 ? 'neutral' : 'warning'))
            );

            if (dateText) {
                dateText.textContent = taskCount === 0
                    ? 'No task dates'
                    : (tasksMissingDates === 0 ? 'Dates set' : `${tasksMissingDates} missing dates`);
            }
            setMilestoneSummaryTone(dateChip, taskCount > 0 && tasksMissingDates === 0 ? 'ok' : (taskCount === 0 ? 'neutral' : 'warning'));

            if (requiredText) {
                requiredText.textContent = requiredUnowned === 0 ? 'Required covered' : `${requiredUnowned} required unowned`;
            }
            setMilestoneSummaryTone(requiredChip, requiredUnowned === 0 ? 'ok' : 'warning');
        });
    }

    function toggleAllTaskDetails(expand) {
        milestonesContainer.querySelectorAll('.task-summary__toggle').forEach(button => {
            const isExpanded = button.getAttribute('aria-expanded') === 'true';
            if (isExpanded !== expand) {
                button.click();
            }
        });
    }


    // ================================================================
    // RENDER TEMPLATE (STATIC OR BACKEND-MAPPED)
    // ================================================================
    function getAllRootMilestones() {
        if (!window.__Template || !Array.isArray(__Template.milestones)) {
            return [];
        }

        return __Template.milestones.filter(ms =>
            ms.key !== '__ROOT_ACTIVITIES__' &&
            ms.name !== 'Root activities'
        );
    }

    function renderTemplate(raw) {
        let parsedRaw = raw;

        if (typeof parsedRaw === 'string') {
            const trimmed = parsedRaw.trim();
            if (!trimmed) {
                return;
            }

            try {
                parsedRaw = JSON.parse(trimmed);
            } catch (err) {
                console.warn('renderTemplate: invalid template JSON payload', err);
                return;
            }
        }

        if (!parsedRaw || typeof parsedRaw !== 'object') {
            return;
        }

        let uiTemplate;
        if (Array.isArray(parsedRaw.milestones)) {
            // Draft restore path: already in UI-template shape.
            uiTemplate = {
                name: parsedRaw.name || 'Draft Template',
                description: parsedRaw.description || '',
                milestones: parsedRaw.milestones
            };
        } else {
            const camelized = keysToCamel(parsedRaw);
            uiTemplate = mapBackendTemplateToUiTemplate(
                camelized,
                'Backend Template',
                'Template from backend'
            );
        }

        if (!uiTemplate || !Array.isArray(uiTemplate.milestones)) {
            return;
        }

        __Template = uiTemplate;

        milestonesContainer.innerHTML = '';

        uiTemplate.milestones.forEach((ms, msIndex) => {
            const msDiv = document.createElement('div');
            const isRootActivities = ms.name === 'Root activities';
            const milestoneTitle = isRootActivities ? 'General activities' : ms.name;
            const milestoneIconClass = isRootActivities ? 'fas fa-stream text-info' : 'fas fa-flag text-primary';
            const msId = `ms_${msIndex}`;
            const msOwnerWidget = isRootActivities ? null : createOwnerWidget([]);
            const milestoneStartYear = ms.startDate || '';
            const milestoneStartWeek = ms.startWeek || '';
            const milestoneEndYear = ms.endDate || '';
            const milestoneEndWeek = ms.endWeek || '';
            const maturityLabel = ms.meta && ms.meta.maturity ? `
          <span class="badge-maturity"><i class="fas fa-layer-group me-1"></i>${ms.meta.maturity}</span>
        ` : '';

            msDiv.className = 'milestone-row' + (isRootActivities ? ' milestone-row--root' : '');

            const milestoneHTML = !isRootActivities ? `
          <div class="milestone-detail-grid">
            <div class="milestone-detail-card">
              <span class="milestone-detail-card__label">Milestone owners</span>
              <div class="ms-owner-widget"></div>
            </div>
            <div class="milestone-detail-card">
              <span class="milestone-detail-card__label">Target window</span>
              <div class="milestone-date-grid">
                <div class="milestone-field">
                  <label for="projectstartDatePicker${msIndex}">Target Start Year</label>
                  <input type="text"
                         class="form-control yearpicker"
                         id="projectstartDatePicker${msIndex}"
                         name="projectstartDatePicker${msIndex}"
                         value="${milestoneStartYear}"
                         data-type="ms-startyy"
                         data-milestone-index="${msIndex}" />
                </div>
                <div class="milestone-field">
                  <label for="targetstartworkweekSelect${msIndex}">Target Start (WW)</label>
                  <select class="form-select workweekSelect"
                          id="targetstartworkweekSelect${msIndex}"
                          data-type="ms-startww"
                          data-milestone-index="${msIndex}"
                          data-initial-value="${milestoneStartWeek}"><option></option></select>
                </div>
                <div class="milestone-field">
                  <label for="projectendDatePicker${msIndex}">Target End Year</label>
                  <input type="text"
                         class="form-control yearpicker"
                         id="projectendDatePicker${msIndex}"
                         name="projectendDatePicker${msIndex}"
                         value="${milestoneEndYear}"
                         data-type="ms-endyy"
                         data-milestone-index="${msIndex}" />
                </div>
                <div class="milestone-field">
                  <label for="targetendworkweekSelect${msIndex}">Target End (WW)</label>
                  <select class="form-select workweekSelect"
                          id="targetendworkweekSelect${msIndex}"
                          data-type="ms-endww"
                          data-milestone-index="${msIndex}"
                          data-initial-value="${milestoneEndWeek}"><option></option></select>
                </div>
              </div>
            </div>
          </div>` : `
          <div class="milestone-root-note">
            <i class="fas fa-sitemap mt-1"></i>
            <span>These activities sit outside a formal milestone gate. Assign owners and target dates directly on each task card.</span>
          </div>`;

            msDiv.innerHTML = `
          <div class="milestone-header" data-bs-toggle="collapse" data-bs-target="#${msId}_body"
               aria-expanded="true" aria-controls="${msId}_body">
            <div class="d-flex flex-column flex-md-row flex-md-wrap gap-1 flex-grow-1">
              <div class="milestone-title">
                <i class="${milestoneIconClass}"></i>${milestoneTitle}
              </div>
              <div class="milestone-meta">
                ${maturityLabel}
                <span class="milestone-summary-chip milestone-summary-chip--neutral"><i class="far fa-list-alt"></i><span>${ms.tasks.length} task(s)</span></span>
                <span class="milestone-summary-chip milestone-summary-chip--warning" data-role="milestone-assignment-chip"><i class="far fa-user"></i><span data-role="milestone-assignment-text">0/${ms.tasks.length} assigned</span></span>
                <span class="milestone-summary-chip milestone-summary-chip--warning" data-role="milestone-date-chip"><i class="far fa-calendar-alt"></i><span data-role="milestone-date-text">Dates pending</span></span>
                <span class="milestone-summary-chip milestone-summary-chip--warning" data-role="milestone-required-chip"><i class="fas fa-triangle-exclamation"></i><span data-role="milestone-required-text">Required pending</span></span>
              </div>
            </div>
            <div>
              <i class="fas fa-chevron-right milestone-toggle-icon"></i>
            </div>
          </div>
          <div class="collapse show milestone-body" id="${msId}_body">
            ${milestoneHTML}
            <div class="tasks-container"></div>
          </div>
        `;

            if (msOwnerWidget) {
                msDiv.querySelector('.ms-owner-widget').appendChild(msOwnerWidget);
                msOwnerWidget.dataset.type = 'milestone';
                msOwnerWidget.dataset.milestoneIndex = msIndex;
            }

            const tasksContainer = msDiv.querySelector('.tasks-container');

            ms.tasks.forEach((t, tIndex) => {
                const taskRow = document.createElement('div');
                const taskOwnerWidget = createOwnerWidget([]);
                const isRequired = t.meta && t.meta.isRequired === true;
                const requiredBadge = isRequired
                    ? '<span class="badge-required">Required</span>'
                    : '<span class="badge-optional">Optional</span>';
                const prereqCount = t.meta && t.meta.prerequisites ? t.meta.prerequisites.length : 0;
                const prereqLabel = prereqCount > 0
                    ? `<span class="ms-1"><i class="fas fa-link me-1"></i>${prereqCount} prerequisite(s)</span>`
                    : '';
                const taskId = `${msId}_task_${tIndex}`;
                const taskStartYear = t.startDate || '';
                const taskStartWeek = t.startWeek || '';
                const taskEndYear = t.endDate || '';
                const taskEndWeek = t.endWeek || '';
                const hasCompleteDates = !!taskStartYear && !!taskStartWeek && !!taskEndYear && !!taskEndWeek;

                taskRow.className = 'task-row';
                taskRow.dataset.required = isRequired ? 'true' : 'false';
                taskRow.innerHTML = `
    <div class="task-card${isRequired ? ' task-card--required' : ''}">
        <div class="task-summary">
            <div class="task-summary__main">
                <div class="task-name">
                    <i class="fas fa-check-circle"></i>${t.name}
                </div>
                <div class="task-meta">
                    ${requiredBadge}${prereqLabel}
                </div>
            </div>
            <div class="task-summary__aside">
                <span class="task-summary-chip task-summary-chip--warning" data-role="task-owner-chip"><i class="far fa-user"></i><span data-role="task-owner-text">No owners</span></span>
                <span class="task-summary-chip ${hasCompleteDates ? 'task-summary-chip--ok' : 'task-summary-chip--warning'}" data-role="task-date-chip"><i class="far fa-calendar-alt"></i><span data-role="task-date-text">${formatDateRangeSummary(taskStartYear, taskStartWeek, taskEndYear, taskEndWeek)}</span></span>
                <button type="button"
                        class="task-summary__toggle"
                        data-bs-toggle="collapse"
                        data-bs-target="#${taskId}_body"
                        aria-expanded="false"
                        aria-controls="${taskId}_body">
                    <span>Details</span>
                    <i class="fas fa-chevron-right"></i>
                </button>
            </div>
        </div>
        <div class="collapse task-details" id="${taskId}_body">
            <div class="task-details__grid">
                <div class="task-detail-card">
                    <span class="task-detail-card__label">Activity owners</span>
                    <div class="task-owner-widget"></div>
                </div>
                <div class="task-detail-card">
                    <span class="task-detail-card__label">Target window</span>
                    <div class="task-dates-grid">
                        <div class="task-field">
                            <label for="taskStartYear_${msIndex}_${tIndex}">Target Start Year</label>
                            <input type="text"
                                   class="form-control yearpicker"
                                   id="taskStartYear_${msIndex}_${tIndex}"
                                   name="taskStartYear_${msIndex}_${tIndex}"
                                   value="${taskStartYear}"
                                   data-type="task-startyy"
                                   data-milestone-index="${msIndex}"
                                   data-task-index="${tIndex}" />
                        </div>
                        <div class="task-field">
                            <label for="taskStartWw_${msIndex}_${tIndex}">Target Start (WW)</label>
                            <select class="form-select workweekSelect"
                                    id="taskStartWw_${msIndex}_${tIndex}"
                                    data-type="task-startww"
                                    data-milestone-index="${msIndex}"
                                    data-task-index="${tIndex}"
                                    data-initial-value="${taskStartWeek}">
                                <option></option>
                            </select>
                        </div>
                        <div class="task-field">
                            <label for="taskEndYear_${msIndex}_${tIndex}">Target End Year</label>
                            <input type="text"
                                   class="form-control yearpicker"
                                   id="taskEndYear_${msIndex}_${tIndex}"
                                   name="taskEndYear_${msIndex}_${tIndex}"
                                   value="${taskEndYear}"
                                   data-type="task-endyy"
                                   data-milestone-index="${msIndex}"
                                   data-task-index="${tIndex}" />
                        </div>
                        <div class="task-field">
                            <label for="taskEndWw_${msIndex}_${tIndex}">Target End (WW)</label>
                            <select class="form-select workweekSelect"
                                    id="taskEndWw_${msIndex}_${tIndex}"
                                    data-type="task-endww"
                                    data-milestone-index="${msIndex}"
                                    data-task-index="${tIndex}"
                                    data-initial-value="${taskEndWeek}">
                                <option></option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
`;

                taskRow.querySelector('.task-owner-widget').appendChild(taskOwnerWidget);
                taskOwnerWidget.dataset.type = 'task';
                taskOwnerWidget.dataset.milestoneIndex = msIndex;
                taskOwnerWidget.dataset.taskIndex = tIndex;

                tasksContainer.appendChild(taskRow);
            });

            milestonesContainer.appendChild(msDiv);
        });

        refreshAllOwnerWidgets();
        $('.yearpicker').yearpicker({ autoHide: true, });

        document.querySelectorAll('.workweekSelect').forEach(select => {
            populateWorkWeek(select);
            if (select.dataset.initialValue) {
                select.value = select.dataset.initialValue;
            }
        });

        refreshMilestoneSummaries();
    }

    function mapBackendTemplateToUiTemplate(backendTemplateJson, uiTemplateName, uiTemplateDescription) {

        let source = backendTemplateJson;

        if (typeof source === 'string') {
            try {
                source = JSON.parse(source);
            } catch (err) {
                console.error('mapBackendTemplateToUiTemplate: invalid JSON string', err);
                source = {};
            }
        }

        if (typeof source !== 'object' || source === null) {
            console.error('mapBackendTemplateToUiTemplate: backendTemplateJson is not an object', source);
            source = {};
        }

        const treeData = Array.isArray(source.treeData) ? source.treeData : [];



        const uiTemplate = {
            name: uiTemplateName || 'Imported Template',
            description: uiTemplateDescription || '',
            milestones: []
        };

        const milestoneMap = new Map(); // id -> milestone object

        function getOrCreateMilestone(node) {
            let ms = milestoneMap.get(node.id);
            if (!ms) {
                ms = {
                    name: node.data ?.name || 'Untitled milestone',
                    meta: {
                        id: node.id,
                        maturity: node.data ?.maturity || null,
                        mandays: node.data ?.mandays || null,
                        isRequired: node.data ?.isRequired ?? null,
                        desc: node.data ?.desc || ''
            },
                    tasks: []
                };
                milestoneMap.set(node.id, ms);
                uiTemplate.milestones.push(ms);
            }
            return ms;
        }

        function addActivityTask(node, currentMilestone) {
            const data = node.data || {};
            const task = {
                name: data.name || 'Untitled activity',
                meta: {
                    id: node.id,
                    desc: data.desc || '',
                    maturity: data.maturity || null,
                    mandays: data.mandays || null,
                    isRequired: data.isRequired ?? null,
                    prerequisites: node.prerequisites || [],
                    forms: node.forms || [],
                    collapsed: node.collapsed ?? false
          }
            };
            currentMilestone.tasks.push(task);
        }

        function traverse(node, currentMilestone) {
            const { type, children = [] } = node;
            const data = node.data || {};

            let milestoneForChildren = currentMilestone;

            if (type === 'milestone') {
                milestoneForChildren = getOrCreateMilestone(node);
            } else if (type === 'activity') {
                if (currentMilestone) {
                    addActivityTask(node, currentMilestone);
                } else {
                    // root-level activity -> attach to special "Root activities" milestone
                    let rootMs = milestoneMap.get('__ROOT_ACTIVITIES__');
                    if (!rootMs) {
                        rootMs = {
                            name: 'Root activities',
                            meta: { id: '__ROOT_ACTIVITIES__', desc: 'Activities not under a milestone' },
                            tasks: []
                        };
                        milestoneMap.set('__ROOT_ACTIVITIES__', rootMs);
                        uiTemplate.milestones.push(rootMs);
                    }
                    addActivityTask(node, rootMs);
                    milestoneForChildren = rootMs;
                }
            }

            children.forEach(child => traverse(child, milestoneForChildren));
        }

        treeData.forEach(rootNode => traverse(rootNode, null));

        return uiTemplate;
    }


    // ================================================================
    // DRAFT: apply loaded draft to UI
    // ================================================================
    function applyDraftToUi(draft) {
        if (!draft) return;

        // --- Simple scalar fields ---
        document.getElementById('projectTitle').value = draft.title || '';
        document.getElementById('projectDescription').value = draft.description || '';
        document.getElementById('projectstartDatePicker').value = draft.projectstartYear || '';
        document.getElementById('targetstartworkweekSelect').value = draft.projectstartWorkWeek || '';
        document.getElementById('projectendDatePicker').value = draft.projectendYear || '';
        document.getElementById('targetendworkweekSelect').value = draft.projectstartWorkWeek || '';
        $('#templateDescription').html(draft.templateDescription || '');
        $('#templateCategory').html(draft.templateCategory || '');
        $('#templateJson').val(draft.templateJson || '');
        $('#templateCategoryValue').val(draft.templateCategoryValue || '');
        $('#templatePlantRoadmapLinkSysId').val(draft.templatePlantRoadmapLinkSysId || '');

        //const headerIcon = document.getElementById('headerIcon');
        //headerIcon.style.color = draft.iconcolor || '';
        $('#iconcolor').val(draft.iconcolor || '');

        document.getElementById('projectIconPreview').className = draft.icon || '';
        document.getElementById('projectIconLabel').textContent = draft.icon || '';
        $('#icon').val(draft.icon || '');


        // --- Site (Select2) --- 
        if (draft.siteValue) {
            // Set underlying select value "dummy-owner" and text
            const $obj = $('#siteSelect');
            const newOption = new Option(draft.siteText, draft.siteValue, true, true);

            //populateSites();

            $obj.append(newOption).trigger('change');

            populateTemplates(draft.siteValue);
        }



        // --- Owner (Select2) --- 
        if (draft.ownerValue) {
            ////const $obj = $('#projectownerSelect');
            ////const newOption = new Option(draft.ownerText, draft.ownerValue, true, true);
            ////$obj.append(newOption).trigger('change');


            var $option = $('<option selected>')
                .val(draft.ownerData.id)
                .text(draft.ownerData.text)
                .attr('data-email', draft.ownerData.email)
                .attr('data-firstName', draft.ownerData.firstName)
                .attr('data-lastName', draft.ownerData.lastName)
                .attr('data-userId', draft.ownerData.userId)
                .attr('data-userName', draft.ownerData.userName)
                .prop('selected', true);

            $('#projectownerSelect').append($option).trigger('change');

        }

        // --- Product Group (Select2) --- 
        if (draft.productgroupValue) {
            const $obj = $('#productgroupSelect');
            const newOption = new Option(draft.productgroupText, draft.productgroupValue, true, true);
            $obj.append(newOption).trigger('change');
        }

        // --- Product Division (Select2) --- 
        if (draft.productdivisionValue) {
            const $obj = $('#productdivisionSelect');
            const newOption = new Option(draft.productdivisionText, draft.productdivisionValue, true, true);
            $obj.append(newOption).trigger('change');
        }

        function getMembersOnly() {

            var _members = Array.isArray(draft.members) ? draft.members.map(m => ({ name: m.name, username: m.name.replace('(', '|').replace(')', '').split('|')[1] })) : []


            return _members.filter(mem =>
                mem.username !== draft.ownerData.id
            );
        }


        // --- Members list ---
        members = getMembersOnly();
        renderMembers();

        // --- Template & Milestones & Tasks --- 
        if (draft.templateValue) {
            // select the template in select2 (value = plantRoadmapLinkSysId)
            const $tpl = $('#templateSelect');
            const existingOption = $tpl.find(`option[value="${draft.templateValue}"]`);
            if (existingOption.length === 0) {
                // create a dummy option so select2 shows something
                const opt = new Option(draft.templateText, draft.templateValue, true, true);
                $tpl.append(opt);
            } else {
                existingOption.prop('selected', true);
            }
            $tpl.trigger('change');

            const templateSource = (typeof draft.templateJson === 'string' && draft.templateJson.trim())
                ? draft.templateJson
                : {
                    name: draft.templateText || 'Draft Template',
                    description: draft.templateDescription || '',
                    milestones: Array.isArray(draft.milestones) ? draft.milestones : []
                };

            renderTemplate(templateSource);

            setTimeout(function () {
                applyDraftMilestones(draft);
            }, 500);
        }
    }

    function applyDraftMilestones(draft) {
        if (!draft || !Array.isArray(draft.milestones)) return;
        draft.milestones.forEach((msDraft, msIndex) => {
            // Milestone owners
            const msOwnerWidget = milestonesContainer.querySelector(
                `.ms-owner-widget [data-owner-widget="true"][data-milestone-index="${msIndex}"]`
            );
            if (msOwnerWidget) {
                const hidden = msOwnerWidget.querySelector('.owners-hidden');
                if (hidden) hidden.value = JSON.stringify(msDraft.owners || []);
            }

            // Milestone dates (year + WW)
            const msStartYearInput = milestonesContainer.querySelector(
                `input[data-type="ms-startyy"][data-milestone-index="${msIndex}"]`
            );
            const msEndYearInput = milestonesContainer.querySelector(
                `input[data-type="ms-endyy"][data-milestone-index="${msIndex}"]`
            );
            const msStartWwSelect = milestonesContainer.querySelector(
                `select[data-type="ms-startww"][data-milestone-index="${msIndex}"]`
            );
            const msEndWwSelect = milestonesContainer.querySelector(
                `select[data-type="ms-endww"][data-milestone-index="${msIndex}"]`
            );

            if (msStartYearInput && msDraft.startDate) {
                msStartYearInput.value = msDraft.startDate;
            }
            if (msEndYearInput && msDraft.endDate) {
                msEndYearInput.value = msDraft.endDate;
            }

            if (msStartWwSelect && msDraft.startWeek) {
                msStartWwSelect.value = msDraft.startWeek;
            }
            if (msEndWwSelect && msDraft.endWeek) {
                msEndWwSelect.value = msDraft.endWeek;
            }

            // Tasks
            if (Array.isArray(msDraft.tasks)) {
                msDraft.tasks.forEach((tDraft, tIndex) => {
                    const tOwnerWidget = milestonesContainer.querySelector(
                        `.task-owner-widget [data-owner-widget="true"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    if (tOwnerWidget) {
                        const hidden = tOwnerWidget.querySelector('.owners-hidden');
                        if (hidden) hidden.value = JSON.stringify(tDraft.owners || []);
                    }

                    const tStartYearInput = milestonesContainer.querySelector(
                        `input[data-type="task-startyy"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    const tEndYearInput = milestonesContainer.querySelector(
                        `input[data-type="task-endyy"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    const tStartWwSelect = milestonesContainer.querySelector(
                        `select[data-type="task-startww"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );
                    const tEndWwSelect = milestonesContainer.querySelector(
                        `select[data-type="task-endww"][data-milestone-index="${msIndex}"][data-task-index="${tIndex}"]`
                    );

                    if (tStartYearInput && tDraft.startDate) {
                        tStartYearInput.value = tDraft.startDate;
                    }
                    if (tEndYearInput && tDraft.endDate) {
                        tEndYearInput.value = tDraft.endDate;
                    }
                    if (tStartWwSelect && tDraft.startWeek) {
                        tStartWwSelect.value = tDraft.startWeek;
                    }
                    if (tEndWwSelect && tDraft.endWeek) {
                        tEndWwSelect.value = tDraft.endWeek;
                    }
                });
            }
        });

        // Re-render owner widgets (chips + dropdown)
        refreshAllOwnerWidgets();
    }




    // --------------------------------------------------------------------
    // PRODUCT LIST
    // --------------------------------------------------------------------

    var productcodesList = $('#productcodesList').DataTable({
        paging: false,
        "initComplete": function () {
            $('.dt-search').appendTo('.card-tools-productcodesList-filter');
        },
        columns: [
            {   // Product Code
                data: 'productCode',
                title: 'Product Code'
            },
            {   // Plant Type (you can also show description via render)
                data: 'plantType',
                title: 'Plant Type',
                render: function (data, type, row) {
                    // Example: show "FE - Front End"
                    return row.plantType + (row.plantTypeDesc ? ' - ' + row.plantTypeDesc : '');
                }
            },
            {   // Product Family
                data: 'productFamily',
                title: 'Product Family',
                render: function (data, type, row) {
                    return row.productFamily + (row.productFamilyDesc ? ' - ' + row.productFamilyDesc : '');
                }
            },
            {   // Macro Package
                data: 'macroPackage',
                title: 'Macro Package',
                render: function (data, type, row) {
                    return row.macroPackage + (row.macroPackageDesc ? ' - ' + row.macroPackageDesc : '');
                }
            },
            {   // Package
                data: 'pack',
                title: 'Package',
                render: function (data, type, row) {
                    return row.pack + (row.packDesc ? ' - ' + row.packDesc : '');
                }
            },
            {   // PLine
                data: 'pLine',
                title: 'Product Line',
                render: function (data, type, row) {
                    return row.pLine + (row.pLineDesc ? ' - ' + row.pLineDesc : '');
                }
            },
            {   // Maturity (simple)
                data: 'maturity',
                title: 'Maturity'
            },
            {   // Action column
                data: null,
                className: 'productcodes-action-cell',
                defaultContent:
                    '<button type="button" class="btn btn-sm btn-outline-danger btnDelete">Delete</button>',
                orderable: false,
                searchable: false
            }
        ],
        language: {
            emptyTable: "No product code selected"
        },
        // DOM: we will move the filter into #productcodeslistTools
        dom: 'lrtip',
        paging: false,
        searching: true,
        info: false
    });

    $('#productcodesList tbody').on('click', '.btnDelete', function () {
        productcodesList.row($(this).closest('tr')).remove().draw(false);
        saveProductCodesDraft();
    });

    $('#btnAddProductCode').on('click', async function () {
        var _productCode = $('#productcodeInput').val().trim();
        var _plantType = '';
        var _plantTypeDesc = '';
        var _productFamily = '';
        var _productFamilyDesc = '';
        var _macroPackage = '';
        var _macroPackageDesc = '';
        var _pack = '';
        var _packDesc = '';
        var _pLine = '';
        var _pLineDesc = '';
        var _maturity = '';

        // Required + uniqueness check
        if (!_productCode) {
            alert('Product Code is required');
            return;
        }



        async function getDetails(productcode, plantcode) {
            const encodedProductcode = encodeURIComponent(productcode).replace(/\*/g, "%2A");

            const data = await getDataAsync(getApiRootPath() + `/api/products?productcode=${encodedProductcode}&plantcode=${plantcode}`); // getDataAsync returns a Promise


            var newRow = {
                productCode: data.productCode,
                plantType: data.plantType,
                plantTypeDesc: data.plantTypeDescription,
                productFamily: data.productFamilyCode,
                productFamilyDesc: data.productFamilyDescription,
                macroPackage: data.macroPackageCode,
                macroPackageDesc: data.macroPackageDescription,
                pack: data.packageCode,
                packDesc: data.packageDescription,
                pLine: data.productCode,
                pLineDesc: data.productLine,
                maturity: data.maturityCode
            };

            if (!data.projectNo) {
                productcodesList.row.add(newRow).draw(false);
                saveProductCodesDraft();
            }
            else {
                alert(`Product Code ${_productCode} already linked to project no: ${data.projectNo}.`);
                return;
            }
        }

        const productCodes = _productCode
            .split(/[;,|\t ]+/)
            .map(value => (value || '').trim())
            .filter(Boolean);

        if (!productCodes.length) {
            alert('Product Code is required');
            return;
        }

        showInlineLoadingStatus('Retrieving product details...');
        try {
            const total = productCodes.length;
            for (let i = 0; i < total; i++) {
                const productcode = productCodes[i];
                showInlineLoadingStatus(`Retrieving product ${i + 1} of ${total}: ${productcode}...`);

                if (productCodeExists(productcode)) {
                    alert('Product Code "' + productcode + '" already exists.');
                    continue;
                }

                await getDetails(productcode, $('#siteSelect').val());
            }
        } catch (err) {
            console.warn('Unable to retrieve product details', err);
            if (window.toastr) {
                window.toastr.error('Unable to retrieve one or more product codes.');
            } else {
                alert('Unable to retrieve one or more product codes.');
            }
        } finally {
            hideInlineLoadingStatus();
            $('#productcodeInput').val('');
        }

    });

    function productCodeExists(code) {
        var exists = false;
        var normalized = code.toLowerCase();

        productcodesList.column(0).data().each(function (value, index) {
            if (String(value).toLowerCase() === normalized) {
                exists = true;
                return false; // break
            }
        });

        return exists;
    }

    // ================================================================
    // DRAFT: DataTables (productcodesList) helpers
    // ================================================================
    const PRODUCT_CODES_DRAFT_KEY = 'projectInitProductCodesDraft';

    function getProductCodesDraftData() {
        const rows = [];
        productcodesList.rows().every(function () {
            const data = this.data();
            rows.push(data);
        });
        return rows;
    }

    function saveProductCodesDraft() {
        try {
            const rows = getProductCodesDraftData();
            localStorage.setItem(PRODUCT_CODES_DRAFT_KEY, JSON.stringify(rows));
        } catch (e) {
            console.warn('Unable to save productcodesList draft', e);
        }
    }

    function loadProductCodesDraft() {
        try {
            const raw = localStorage.getItem(PRODUCT_CODES_DRAFT_KEY);
            if (!raw) return null;
            return JSON.parse(raw);
        } catch (e) {
            console.warn('Unable to load productcodesList draft', e);
            return null;
        }
    }

    function clearProductCodesDraft() {
        try {
            localStorage.removeItem(PRODUCT_CODES_DRAFT_KEY);
        } catch (e) {
            console.warn('Unable to clear productcodesList draft', e);
        }
    }

    function applyProductCodesDraft(rows) {
        if (!Array.isArray(rows) || rows.length === 0) return;
        productcodesList.clear();
        rows.forEach(row => {
            productcodesList.row.add(row);
        });
        productcodesList.draw(false);
    }

    // ================================================================
    // DRAFT: load DataTable on startup
    // ================================================================
    const existingProductCodesDraft = loadProductCodesDraft();
    if (existingProductCodesDraft && existingProductCodesDraft.length > 0) {
        applyProductCodesDraft(existingProductCodesDraft);
    }



    // --------------------------------------------------------------------
    // Validate configuration
    // --------------------------------------------------------------------
    $('#projectInitForm').validate({
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
            siteSelect: { required: true },
            templateSelect: { required: true },
            projectownerSelect: { required: true },
            productgroupSelect: { required: true },
            productdivisionSelect: { required: true },
            projectstartDatePicker: {
                digits: true,
                minlength: 4,
                maxlength: 4
            },
            projectendDatePicker: {
                digits: true,
                minlength: 4,
                maxlength: 4
            }
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
            siteSelect: { required: "" },
            templateSelect: { required: "" },
            projectownerSelect: { required: "" },
            productgroupSelect: { required: "" },
            productdivisionSelect: { required: "" },
            projectstartDatePicker: {
                digits: "Start year must be numeric.",
                minlength: "Enter a 4-digit year.",
                maxlength: "Enter a 4-digit year."
            },
            projectendDatePicker: {
                digits: "End year must be numeric.",
                minlength: "Enter a 4-digit year.",
                maxlength: "Enter a 4-digit year."
            }
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



    // ================================================================
    // DRAFT: auto-save on change
    // ================================================================
    let draftSaveTimeout = null;

    function scheduleDraftSave() {
        if (!form) return;
        clearTimeout(draftSaveTimeout);
        draftSaveTimeout = setTimeout(() => {
            const data = buildCurrentProjectData();
            saveDraft(data);
            // console.log('Draft saved', data);
        }, 500); // 0.5s debounce
    }

    // Listen to general changes
    form.addEventListener('input', scheduleDraftSave);
    form.addEventListener('change', scheduleDraftSave);
    milestonesContainer.addEventListener('input', refreshMilestoneSummaries);
    milestonesContainer.addEventListener('change', refreshMilestoneSummaries);

    // Because some widgets update via custom clicks, ensure they also trigger save
    milestonesContainer.addEventListener('click', function (e) {
        if (e.target.closest('.owners-chip') || e.target.closest('.owners-item')) {
            refreshMilestoneSummaries();
            scheduleDraftSave();
        }
    });

    membersListContainer.addEventListener('click', function (e) {
        if (e.target.closest('button[data-index]')) {
            scheduleDraftSave();
        }
    });

    $('#projectownerSelect').on('select2:select select2:clear', function () {
        refreshAllOwnerWidgets();
        refreshMilestoneSummaries();
        scheduleDraftSave();
    });
    //$('#projectmemberSelect').on('select2:select select2:clear', scheduleDraftSave);
    $('#templateSelect').on('select2:select select2:clear', scheduleDraftSave);

    if (btnExpandAllTaskDetails) {
        btnExpandAllTaskDetails.addEventListener('click', function () {
            toggleAllTaskDetails(true);
        });
    }

    if (btnCollapseAllTaskDetails) {
        btnCollapseAllTaskDetails.addEventListener('click', function () {
            toggleAllTaskDetails(false);
        });
    }

    // ================================================================
    // DRAFT: load on startup
    // ================================================================
    const existingDraft = loadDraft();
    if (existingDraft) {
        // optional confirmation:
        //if (confirm('A draft project exists. Restore it?')) { applyDraftToUi(existingDraft); }

        applyDraftToUi(existingDraft);

    }

    // --------------------------------------------------------------------
    // FORM SUBMIT / PREVIEW (unchanged logic, works for all templates)
    // --------------------------------------------------------------------

    function clearForm() {
        setTimeout(() => {
            milestonesContainer.innerHTML = '';
            members = [];
            renderMembers();
            $('#templateSelect').val(null).trigger('change');
            $('#siteSelect').val(null).trigger('change');
            templateDescription.textContent = 'Select a template to see its structure.';
            templateCategory.textContent = 'Select a template to see its category.';
            $('#projectownerSelect').val(null).trigger('change');
            $('#productgroupSelect').val(null).trigger('change');
            $('#productdivisionSelect').val(null).trigger('change');

            $('#projectstartDatePicker').val('');
            $('#targetstartworkweekSelect').val('');
            $('#projectendDatePicker').val('');
            $('#targetendworkweekSelect').val('');


            document.getElementById('previewCard').classList.add('d-none');
            clearDraft();

            // Clear DataTable & its draft
            productcodesList.clear().draw(false);
            clearProductCodesDraft();
        }, 0);
    }


    // Reset behavior
    form.addEventListener('reset', function () {
        clearForm();
    });

    // --------------------------------------------------------------------
    // FORM SUBMIT via AJAX + jQuery Validate
    // --------------------------------------------------------------------


    form.addEventListener('submit', function (e) {
        e.preventDefault(); // prevent normal form post

        // Run jQuery Validate
        var $form = $('#projectInitForm');
        if (!$form.valid()) {
            // Optional: focus first invalid element
            $form.find('.is-invalid:first').focus();
            return;
        }

        function createProjectFlow() {

            scheduleDraftSave();

            const rootMs = getAllRootMilestones();
            var statusOptionsHtml = rootMs.map(function (opt) {
                return (
                    `<option data-maturitycode=${opt.meta.maturity} value="${opt.meta.id}">${opt.name}</option>`
                );
            })
                .join("");

            var dialogHtml =
                '<div class="container-fluid px-0">' +
                '  <div class="card border-0 shadow-sm">' +
                '    <div class="card-body">' +
                '      <p class="text-muted mb-3">' +
                '        Confirm project creation and optionally auto-start (set its initial maturity/milestone and start date) the project.' +
                '      </p>' +

                '      <div class="mb-3">' +
                '        <label class="form-label fw-semibold mb-1">Status options</label>' +
                '        <div class="border rounded p-3 bg-light">' +
                '          <div class="form-check">' +
                '            <input class="form-check-input" type="checkbox" id="chkSetStatusStarted">' +
                '            <label class="form-check-label" for="chkSetStatusStarted">' +
                '              Set status to <span class="fw-semibold">Started</span> after creation' +
                '            </label>' +
                '          </div>' +
                '          <small class="text-muted d-block mt-1">' +
                '            If checked, choose the current maturity/milestone and the actual start date.' +
                '          </small>' +
                '        </div>' +
                '      </div>' +

                '      <div id="statusTransitionContainer" class="mb-2" style="display:none;">' +
                '        <div class="mb-3">' +
                '          <label for="selFromStatus" class="form-label fw-semibold mb-1">Current Maturity/Milestone</label>' +
                '          <select id="selFromStatus" class="form-select">' +
                '            <option value="">Select current maturity/milestone…</option>' +
                statusOptionsHtml +
                '          </select>' +
                '          <div id="statusError" class="text-danger small mt-1" style="display:none;">' +
                '            Please select a maturity/milestone.' +
                '          </div>' +
                '        </div>' +

                '        <div class="mb-1">' +
                '          <label for="txtActualStartDate" class="form-label fw-semibold mb-1">Actual start date</label>' +
                '          <input type="text" id="txtActualStartDate" class="form-control" placeholder="Select start date" />' +
                '          <div id="dateError" class="text-danger small mt-1" style="display:none;">' +
                '            Please select an actual start date.' +
                '          </div>' +
                '        </div>' +
                '      </div>' +

                '      <div class="small text-muted mt-2">' +
                '        You can change the project maturity/milestone and dates later from the project details page.' +
                '      </div>' +
                '    </div>' +
                '  </div>' +
                '</div>';

            var dialog = bootbox.dialog({
                title: "New project",
                message: dialogHtml,
                size: "medium",
                centerVertical: true,
                buttons: {
                    cancel: {
                        label: "Cancel",
                        className: "btn-outline-secondary"
                    },
                    confirm: {
                        label: "Create project",
                        className: "btn-primary",
                        callback: function () {
                            var setStatusToStarted = $("#chkSetStatusStarted").is(":checked");
                            var fromMilestone = null;
                            var fromMaturityCode = null;
                            var actualStartDate = null;

                            if (setStatusToStarted) {
                                const $ms = $("#selFromStatus");
                                const $selectedoption = $ms.find('option:selected')
                                fromMilestone = $ms.val();
                                fromMaturityCode = $selectedoption.data('maturitycode')
                                actualStartDate = $("#txtActualStartDate").val();

                                var hasError = false;

                                if (!fromMilestone) {
                                    $("#statusError").show();
                                    $("#selFromStatus").addClass("is-invalid");
                                    hasError = true;
                                }
                                if (!actualStartDate) {
                                    $("#dateError").show();
                                    $("#txtActualStartDate").addClass("is-invalid");
                                    hasError = true;
                                }

                                if (hasError) {
                                    return false; // keep dialog open
                                }
                            }

                            finalizeProjectCreation({
                                confirmed: true,
                                setStatusToStarted: setStatusToStarted,
                                fromMaturityCode: fromMaturityCode,
                                fromMilestone: fromMilestone,
                                actualStartDate: actualStartDate
                            });
                        }
                    }
                }
            });

            dialog.on("shown.bs.modal", function () {
                // Toggle section visibility
                $("#chkSetStatusStarted").on("change", function () {
                    var checked = $(this).is(":checked");
                    if (checked) {
                        $("#statusTransitionContainer").slideDown(150);
                    } else {
                        $("#statusTransitionContainer").slideUp(150);
                        $("#selFromStatus").removeClass("is-invalid");
                        $("#statusError").hide();
                        $("#txtActualStartDate").removeClass("is-invalid");
                        $("#dateError").hide();
                    }
                });

                // Clear validation on change
                $("#selFromStatus").on("change", function () {
                    if ($(this).val()) {
                        $(this).removeClass("is-invalid");
                        $("#statusError").hide();
                    }
                });

                // Init flatpickr for actual start date
                flatpickr("#txtActualStartDate", {
                    dateFormat: "Y-m-d",
                    maxDate: "today",
                    defaultDate: "today",
                    allowInput: true
                });

                $("#txtActualStartDate").on("change", function () {
                    if ($(this).val()) {
                        $(this).removeClass("is-invalid");
                        $("#dateError").hide();
                    }
                });
            });
        }

        function showProjectCreatedDialog(projectName, projectNumber) {
            const normalizedProjectNumber = String(projectNumber || '').trim();
            const detailsUrl = `${appPath}/projects/${encodeURIComponent(normalizedProjectNumber)}/details`;

            bootbox.dialog({
                backdrop: true,
                closeButton: false,
                size: "small",
                centerVertical: true,
                message:
                    '<div class="text-center py-2">' +
                    '  <div class="mb-3">' +
                    '    <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-3 py-2" ' +
                    '          style="font-size: 0.8rem;">' +
                    '      <i class="bi bi-check-circle-fill me-1"></i>' +
                    "      Project Created" +
                    "    </span>" +
                    "  </div>" +
                    '  <h5 class="mb-1" style="font-weight: 600;">' +
                    "    " +
                    escapeHtml(projectName) +
                    "</h5>" +
                    '  <p class="text-muted mb-3" style="font-size: 0.9rem;">' +
                    "    Your project has been successfully created." +
                    "  </p>" +
                    '  <div class="mb-3">' +
                    '    <span class="text-uppercase text-muted" style="font-size: 0.75rem; letter-spacing: 0.08em;">' +
                    "      Project #" +
                    "    </span>" +
                    '    <div class="fs-5 fw-bold text-primary mt-1">' +
                    "      " +
                    escapeHtml(projectNumber) +
                    "    </div>" +
                    "  </div>" +
                    '  <p class="text-muted mb-0" style="font-size: 0.8rem;">' +
                    "    You can now configure details, assign team members and set milestones." +
                    "  </p>" +
                    "</div>",
                buttons: {
                    details: {
                        label: "View Project",
                        className:
                            "btn btn-primary btn-sm px-3 rounded-pill fw-semibold",
                        callback: function () {
                            clearForm();

                            if (!normalizedProjectNumber) {
                                return true;
                            }

                            window.location.href = detailsUrl;
                            return true;
                        },
                    },
                    close: {
                        label: "Close",
                        className:
                            "btn btn-outline-secondary btn-sm px-3 rounded-pill",
                        callback: function () {

                            try {
                                clearForm();

                            } catch (error) {
                                console.error('An error occurred:', error.message);
                            }
                            

                            window.location.href = "/Projects/Index"
                            return true;
                        },
                    },
                },
            });
        }

        // Simple HTML escaping to avoid XSS if project name comes from user input
        function escapeHtml(str) {
            return String(str)
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }


        // Final handler
        function finalizeProjectCreation(data) {
            // data = { confirmed, setStatusToStarted, fromStatus } 
            console.log("Finalized project creation:", data);


            // <<< SHOW LOADING HERE >>>
            showLoading('Creating project, please wait...');
            // Build the payload from current UI
            const payload = buildCurrentProjectData();
            payload.autoStart = data.setStatusToStarted;
            payload.projectMaturityCode = data.fromMaturityCode;
            payload.currentMilestoneSysId = data.fromMilestone;
            payload.actualStartDate = data.actualStartDate;
            payload.productCodes = getProductCodesDraftData();

            var formData = new FormData();
 
            formData.append("project", JSON.stringify(payload));



            //JSON.stringify(payload)
           


            $.ajax({
                url: apiPath + '/api/Projects',
                type: 'POST',
                processData: false,
                contentType: false, 
                data: formData,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function (result) {
                    hideLoading();

                    //alert('Project successfully created with ID: ' + result.projectId);
                    showProjectCreatedDialog(result.title, result.projectId);
 
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
        }




        createProjectFlow();





    });
});


//////$(document).ready(function () {

//////    ////$('#modalForms').on('select2:select', function (e) {
//////    ////    // e.params.data contains the selected option's data
//////    ////    var selectedData = e.params.data;

//////    ////    $('#modalDescription').val(selectedData.description)

//////    ////    async function updatePageContent(id) {
//////    ////        const data = await getDataAsync(getApiRootPath() + `/api/forms/${id}`); // getDataAsync returns a Promise

//////    ////        let formObj;
//////    ////        try {
//////    ////            formObj = JSON.parse(data.formJson);
//////    ////        } catch (e) {
//////    ////            bootbox.alert("Invalid form JSON!");
//////    ////            return;
//////    ////        }
//////    ////        const fields = formObj.fields || [];
//////    ////        const $formContainer = $('<form class="bootbox-form-container"></form>');
//////    ////        $formContainer.dynamicField({ fields: fields, userCode: "*" });


//////    ////        $('#containerPreview').html($formContainer);
//////    ////    }


//////    ////    updatePageContent(selectedData.id)


//////    ////});


//////    ////$('#categories').on('change', function () {
//////    ////    $(this).valid();
//////    ////});





//////    $('#projectInitForm').validate({
//////        // Add your validation rules if needed
//////        rules: {
//////            projectTitle: { required: true },
//////            siteSelect: { required: true },
//////            templateSelect: { required: true },
//////            projectownerSelect: { required: true },
//////            productgroupSelect: { required: true },
//////            productdivisionSelect: { required: true } 

//////        },
//////        messages: {
//////            projectTitle: { required: "test" },
//////            siteSelect: { required: "" },
//////            templateSelect: { required: "" },
//////            projectownerSelect: { required: "" },
//////            productgroupSelect: { required: "" },
//////            productdivisionSelect: { required: "" } 
//////        },
//////        errorElement: 'span', 
//////        errorPlacement: function (error, element) {
//////            if (element.hasClass('select2-hidden-accessible')) {
//////                // place error label after the visible Select2 element
//////                error.insertAfter(element.next('.select2'));
//////            } else {
//////                // suppress label for standard Bootstrap inputs (we use .invalid-feedback)
//////                return;
//////            }
//////        },

//////        highlight: function (element) {
//////            const $el = $(element);

//////            $el.addClass('is-invalid').removeClass('is-valid');

//////            const $feedback = $el.closest('.form-floating').find('.invalid-feedback');
//////            if ($feedback.length) {
//////                $feedback.show();
//////            }

//////            if ($el.hasClass('select2-hidden-accessible')) {
//////                const $container = $el.next('.select2');
//////                $container.find('.select2-selection').addClass('is-invalid');
//////            }
//////        },

//////        unhighlight: function (element) {
//////            const $el = $(element);

//////            $el.removeClass('is-invalid').addClass('is-valid');

//////            const $feedback = $el.closest('.form-floating').find('.invalid-feedback');
//////            if ($feedback.length) {
//////                $feedback.hide();
//////            }

//////            if ($el.hasClass('select2-hidden-accessible')) {
//////                const $container = $el.next('.select2');
//////                $container.find('.select2-selection').removeClass('is-invalid');
//////            }
//////        },
//////        submitHandler: function (form) {

//////            const projectData = buildCurrentProjectData(); 

//////            clearDraft();

//////            //////// This function runs only if the form is valid

//////            //////// Prepare FormData
//////            //////var formData = new FormData();
//////            //////var fileInput = $('#siteImage')[0];
//////            //////if (fileInput.files.length > 0) {
//////            //////    formData.append("file", fileInput.files[0]);
//////            //////}
//////            //////// Collect data 
//////            //////var plant = {
//////            //////    PlantCode: $('#Code').val(),
//////            //////    PlantName: $('#Name').val(),
//////            //////    TransactionKey: $('#TransactionKey').val(),
//////            //////    IsActive: $('#isActive').val()
//////            //////};
//////            //////formData.append("plant", JSON.stringify(plant));

//////            //////// AJAX call
//////            //////$.ajax({
//////            //////    url: getApiRootPath() + '/api/plants/' + plant.PlantCode,
//////            //////    type: 'PUT',
//////            //////    data: formData,
//////            //////    processData: false,
//////            //////    contentType: false,
//////            //////    // xhrFields: { withCredentials: true }, //** REMOVED**
//////            //////    success: function () {
//////            //////        toastr.success('Plant is successfully updated!');
//////            //////        fetchPageInfo();
//////            //////    },
//////            //////    error: function (xhr) {
//////            //////        if (xhr.status === 404) {
//////            //////            toastr.error('Plant code does not exist!');

//////            //////        } else {
//////            //////            toastr.error('Error: ' + xhr.responseText);
//////            //////        }
//////            //////    }
//////            //////});

//////            //////// Prevent default form submission
//////            //////return false;
//////        }
//////    });
//////});