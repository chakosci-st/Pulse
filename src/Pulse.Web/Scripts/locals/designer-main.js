// scripts/designer-main.js

window.formFields = [];
window.referenceFieldsCatalog = [];


// --- URL validation helper ---
function isValidUrl(url) {
    if (!url) return false;
    let testUrl = url.replace('{key}', 'testvalue');
    try {
        let u = new URL(testUrl);
        return u.protocol === 'http:' || u.protocol === 'https:';
    } catch {
        return false;
    }
}

function toBooleanFlag(value) {
    if (value === true || value === 1) return true;
    if (typeof value === 'string') {
        const v = value.toLowerCase();
        return v === 'true' || v === '1';
    }

    return false;
}

function extractChildFieldsFromJson(json) {
    if (!Array.isArray(json)) throw new Error('Child fields JSON must be an array');
    return json.map(f => ({
        title: f.title,
        type: f.type,
        name: f.name,
        ...(f.placeholder && { placeholder: f.placeholder }),
        ...(f.options && { options: f.options }),
        ...(f.datasource && { datasource: f.datasource }),
        ...(f.datasourceParamField && { datasourceParamField: f.datasourceParamField }),
        ...(f.minLength && { minLength: f.minLength }),
        ...(f.maxLength && { maxLength: f.maxLength }),
        ...(f.caseOption && { caseOption: f.caseOption }),
        ...(f.fileTypes && { fileTypes: f.fileTypes }),
        ...(f.fileMaxSize && { fileMaxSize: f.fileMaxSize }),
        ...(f.isrequired && { isrequired: f.isrequired }),
        ...(f.rules && { rules: f.rules }),
        ...(f.validate && { validate: f.validate }),
        ...(f.readAccess && { readAccess: f.readAccess }),
        ...(f.writeAccess && { writeAccess: f.writeAccess }),
        ...(f.tooltip && { tooltip: f.tooltip }),
        ...(f.urlIsParam && { urlIsParam: f.urlIsParam }),
        ...(f.urlDefaultPattern && { urlDefaultPattern: f.urlDefaultPattern }),
        ...(f.defaultValue && { defaultValue: f.defaultValue }),
        ...(f.defaultClobValue && { defaultClobValue: f.defaultClobValue }),
        ...(f.childFields && { childFields: extractChildFieldsFromJson(f.childFields) }),
    }));
}

function updateFieldTargetOptions() {
    const $sel = $('#fieldTarget').empty();
    $sel.append('<option value="main">Main Form</option>');
    window.formFields.forEach((f, idx) => {
        if (f.type === 'children') {
            $sel.append(`<option value="children-${idx}">${f.title} (Detail Table)</option>`);
        }
    });
}

function mapReferenceField(rawField) {
    const options = (rawField.options || rawField.Options || []).map(o => ({
        id: o.id || o.FieldOptionSysId || '',
        value: o.value || o.optionValue || o.OptionValue || '',
        label: o.label || o.optionLabel || o.OptionLabel || o.value || o.optionValue || o.OptionValue || '',
        orderIndex: o.orderIndex || o.OrderIndex || 0
    }));

    const rules = (rawField.rules || rawField.Rules || []).map(r => ({
        id: r.id || r.FieldRuleSysId || '',
        field: r.field || r.RuleField || '',
        operator: r.operator || r.RuleOperator || 'eq',
        value: r.value || r.RuleValue || '',
        action: r.action || r.RuleAction || 'visible',
        actionValue: r.actionValue || r.RuleActionValue || ''
    }));

    return {
        id: '',
        fieldSysId: rawField.id || rawField.FieldSysId || rawField.fieldSysId || '',
        title: rawField.title || rawField.FieldTitle || 'Untitled',
        name: rawField.name || rawField.FieldName || 'field',
        type: rawField.type || rawField.FieldType || 'text',
        placeholder: rawField.placeholder || rawField.Placeholder || '',
        tooltip: rawField.tooltip || rawField.Tooltip || '',
        isrequired: rawField.isrequired === true || rawField.IsRequired === true,
        readAccess: rawField.readAccess || rawField.ReadAccess || '*',
        writeAccess: rawField.writeAccess || rawField.WriteAccess || '*',
        minLength: rawField.minLength || rawField.MinLength || undefined,
        maxLength: rawField.maxLength || rawField.MaxLength || undefined,
        caseOption: rawField.caseOption || rawField.CaseOption || undefined,
        datasource: rawField.datasource || rawField.DataSource || undefined,
        datasourceParamField: rawField.datasourceParamField || rawField.DataSourceParamField || undefined,
        validate: rawField.validate || rawField.FieldValidate || undefined,
        fileTypes: rawField.fileTypes || rawField.FileType || undefined,
        fileMaxSize: rawField.fileMaxSize || rawField.FileMaxSize || undefined,
        urlIsParam: rawField.urlIsParam === true || rawField.urlIsParameter === true || rawField.UrlIsParam === true || rawField.UrlIsParameter === true,
        urlDefaultPattern: rawField.urlDefaultPattern || rawField.UrlDefaultPattern || undefined,
        defaultPattern: rawField.defaultPattern || rawField.DefaultPattern || undefined,
        defaultValue: rawField.defaultValue || rawField.DefaultValue || undefined,
        defaultClobValue: rawField.defaultClobValue || rawField.DefaultClobValue || undefined,
        options: options,
        rules: rules,
        useFieldDefaults: true,
        createAsReference: false
    };
}

function initializeReferenceFieldPicker() {
    const $select = $('#fieldReferenceSelect');
    if (!$select.length) return;

    const loadOptions = function (selectedId) {
        $select.empty();
        $select.append('<option value="">Search field...</option>');
        window.referenceFieldsCatalog.forEach(function (field) {
            const text = (field.FieldTitle || field.title || 'Untitled') + ' (' + (field.FieldType || field.type || 'text') + ')';
            const value = field.FieldSysId || field.id || '';
            $select.append(new Option(text, value, false, selectedId && selectedId === value));
        });
    };

    const loadReferenceFields = function () {
        return $.ajax({
            url: getApiRootPath() + '/api/fields',
            type: 'GET',
            contentType: 'application/json'
        }).done(function (response) {
            window.referenceFieldsCatalog = Array.isArray(response) ? response : [];
            loadOptions();
            if ($select.data('select2')) {
                $select.trigger('change.select2');
            }
        });
    };

    loadOptions();

    if ($.fn.select2) {
        $select.select2({
            placeholder: 'Search field...',
            allowClear: true,
            width: '100%'
        });
    }

    $select.data('reloadReferenceFields', loadReferenceFields);
    loadReferenceFields();
}

function renderFieldsList() {
    const $list = $('#fieldsList').empty();
    function renderFieldItem(field, idx, parentIdx) {
        let badges = '';
        if (field.isrequired) badges += `<span class="badge bg-danger ms-1">Required</span>`;
        if (field.placeholder) badges += `<span class="badge bg-secondary ms-1">Placeholder</span>`;
        if (field.tooltip) badges += `<span class="tooltip-icon" title="${field.tooltip}"><i class="bi bi-info-circle"></i></span>`;
        if (field.readAccess) badges += `<span class="badge bg-info text-dark access-badge ms-1">R:${field.readAccess}</span>`;
        if (field.writeAccess) badges += `<span class="badge bg-warning text-dark access-badge ms-1">W:${field.writeAccess}</span>`;
        if (['selection', 'radio', 'checkboxcollection'].includes(field.type)) {
            if (field.options && field.options.length) {
                const optionText = (field.options || []).map(function (opt) {
                    if (typeof opt === 'string') return opt;
                    return opt.label || opt.value || '';
                }).filter(Boolean).join(', ');
                badges += `<span class="badge bg-light text-dark ms-1">Options: <code>${optionText}</code></span>`;
            }
            if (field.datasource)
                badges += `<span class="badge bg-light text-dark ms-1">Datasource</span>`;
            if (field.datasourceParamField)
                badges += `<span class="badge bg-light text-dark ms-1">Param: ${field.datasourceParamField}</span>`;
        }
        if (field.type === 'file') {
            if (field.fileTypes) badges += `<span class="badge bg-light text-dark ms-1">Types: ${field.fileTypes}</span>`;
            if (field.fileMaxSize) badges += `<span class="badge bg-light text-dark ms-1">Max: ${field.fileMaxSize}MB</span>`;
        }
        if (field.type === 'children') {
            badges += `<span class="badge bg-light text-dark ms-1">Child Fields</span>`;
        }
        if (field.visibleIf) {
            badges += `<span class="badge bg-light text-dark ms-1">Visible if <strong>${field.visibleIf.field}</strong> = <strong>${field.visibleIf.value}</strong></span>`;
        }
        if (field.rules && field.rules.length) {
            badges += `<span class="badge bg-light text-dark ms-1">Rules</span>`;
        }
        if (field.validate) {
            badges += `<span class="badge bg-light text-dark ms-1">Validation</span>`;
        }
        if (field.minLength) {
            badges += `<span class="badge bg-light text-dark ms-1">Min: ${field.minLength}</span>`;
        }
        if (field.maxLength) {
            badges += `<span class="badge bg-light text-dark ms-1">Max: ${field.maxLength}</span>`;
        }
        if (field.caseOption) {
            badges += `<span class="badge bg-light text-dark ms-1">${field.caseOption === 'upper' ? 'UPPER' : 'lower'}</span>`;
        }
        if (field.type === 'url' && field.urlIsParam) {
            badges += `<span class="badge bg-primary ms-1">Parameterized URL</span>`;
        }
                if (field.wasReactivated === true) {
                        badges += `<span class="badge bg-success-subtle text-success-emphasis ms-1">Reactivated</span>`;
                }
                if (field.fieldSysId) {
                    badges += `<span class="badge bg-primary-subtle text-primary-emphasis ms-1">Ref</span>`;
                    badges += toBooleanFlag(field.useFieldDefaults)
                    ? `<span class="badge bg-light text-dark ms-1">Using defaults</span>`
                    : `<span class="badge bg-warning text-dark ms-1">Form override</span>`;
                }
        let $item = $(`
          <li class="list-group-item d-flex align-items-center field-config" data-field-idx="${idx}" ${parentIdx !== undefined ? `data-parent-idx="${parentIdx}"` : ''}>
            <i class="bi bi-arrows-move me-2 move-handle"></i>
            <span class="flex-grow-1">
              <strong>${field.title}</strong>
              <span class="badge bg-info text-dark">${field.type}</span>
              ${badges}
            </span>
            <button type="button" class="btn btn-sm btn-info ms-2 edit-btn" title="Edit"><i class="bi bi-pencil"></i></button>
            <button type="button" class="btn btn-sm btn-danger ms-1 remove-btn" title="Remove"><i class="bi bi-trash"></i></button>
          </li>
        `);
        $list.append($item);

        // Render child fields recursively
        if (field.type === 'children' && Array.isArray(field.childFields)) {
            let $childList = $('<ul class="list-group ms-4"></ul>');
            field.childFields.forEach((childField, cidx) => {
                renderFieldItem(childField, cidx, idx);
            });
            $list.append($childList);
        }
    }
    window.formFields.forEach((field, idx) => renderFieldItem(field, idx));
}

function renderPreviewForm() {
    $('#previewForm').empty().dynamicField({ fields: window.formFields, userCode: window.currentUserCode });
}

function renderFormStructure() {
    $('#formStructure').text(JSON.stringify(window.formFields, null, 2));
}

function updateConditionalFieldOptions() {
    const $select = $('#conditionalField').empty();
    $select.append('<option value="">-- Select Field --</option>');
    function addOptions(fields) {
        fields.forEach(f => {
            $select.append(`<option value="${f.name || f.title}">${f.title}</option>`);
            if (f.type === 'children' && Array.isArray(f.childFields)) {
                addOptions(f.childFields);
            }
        });
    }
    addOptions(window.formFields);

    // Also update datasource param field options
    const $paramSelect = $('#datasourceParamField').empty();
    $paramSelect.append('<option value="">-- None --</option>');
    addOptions(window.formFields);
}

$(document).ready(function () {
    window.currentUserCode = "*";

    initializeReferenceFieldPicker();

    $('#fieldType').on('change', function () {
        const type = $(this).val();
        $('#optionsGroup').toggle(['selection', 'radio', 'checkboxcollection'].includes(type));
        $('#datasourceGroup').toggle(['selection', 'radio', 'checkboxcollection'].includes(type));
        $('#datasourceParamGroup').toggle(['selection', 'radio', 'checkboxcollection'].includes(type));
        $('#childrenGroup').toggle(type === 'children');
        $('#validationGroup').toggle(['text', 'number', 'url'].includes(type));
        $('#conditionalGroup').toggle(window.formFields.length > 0);
        $('#minLengthGroup, #maxLengthGroup').toggle(['text', 'textarea', 'url'].includes(type));
        $('#caseOptionGroup').toggle(['text', 'textarea', 'url'].includes(type));
        $('#rulesGroup').toggle(window.formFields.length > 0);
        $('#fileTypesGroup, #fileMaxSizeGroup').toggle(type === 'file');
        $('#urlParamGroup').toggle(type === 'url');

        $('#fieldDefaultValueGroup').toggle(type !== 'textarea' && type !== 'richtext');
        $('#fieldDefaultClobValueGroup').toggle(type === 'textarea' || type === 'richtext');

        updateConditionalFieldOptions();
        if (window.RulesBuilder) window.RulesBuilder.initRulesUI(window.formFields);
    });

    $('#addReferenceFieldBtn').on('click', function () {
        const selectedId = $('#fieldReferenceSelect').val();
        if (!selectedId) {
            alert('Select an existing field first.');
            return;
        }

        $.ajax({
            url: getApiRootPath() + '/api/fields/' + selectedId,
            type: 'GET',
            contentType: 'application/json'
        }).done(function (response) {
            const mapped = mapReferenceField(response || {});
            if (!mapped.fieldSysId) {
                mapped.fieldSysId = selectedId;
            }

            window.formFields.push(mapped);
            renderFieldsList();
            renderPreviewForm();
            renderFormStructure();
            updateFieldTargetOptions();
        }).fail(function () {
            toastr.error('Unable to load selected field details.');
        });
    });

    // Add field
    $('#addFieldForm').on('submit', function (e) {
        e.preventDefault();
        const type = $('#fieldType').val();
        let field = {
            title: $('#fieldLabel').val(),
            type: type,
            name: $('#fieldLabel').val().replace(/\s+/g, '_').toLowerCase(),
            isrequired: $('#fieldRequired').is(':checked'),
            placeholder: $('#fieldPlaceholder').val(),
            tooltip: $('#fieldTooltip').val(),
            readAccess: $('#fieldReadAccess').val(),
            writeAccess: $('#fieldWriteAccess').val(),
            defaultValue: $('#fieldDefaultValue').val() || undefined,
            fieldSysId: '',
            useFieldDefaults: true,
            createAsReference: true
        };
        if (['selection', 'radio', 'checkboxcollection'].includes(type)) {
            field.options = $('#fieldOptions').val().split(',').map(s => s.trim()).filter(Boolean);
            field.datasource = $('#fieldDatasource').val();
            field.datasourceParamField = $('#datasourceParamField').val() || undefined;
        }
        if (type === 'children') {
            field.childFields = [];
        }

        if (type === 'url' && $('#urlIsParam').is(':checked')) {
            const pattern = $('#urlDefaultPattern').val();
            if (!pattern || !pattern.includes('{key}') || !isValidUrl(pattern)) {
                alert('Default pattern must be a valid URL and include {key}');
                return;
            }
        }

        if (type === 'url') {
            field.urlIsParam = $('#urlIsParam').is(':checked');
            field.urlDefaultPattern = $('#urlDefaultPattern').val() || undefined;
        }
        if ($('#fieldValidation').val()) {
            field.validate = $('#fieldValidation').val();
        }
        if ($('#conditionalField').val()) {
            field.visibleIf = {
                field: $('#conditionalField').val(),
                value: $('#conditionalValue').val()
            };
        }
        if (['text', 'textarea', 'url'].includes(type)) {
            field.minLength = parseInt($('#fieldMinLength').val()) || undefined;
            field.maxLength = parseInt($('#fieldMaxLength').val()) || undefined;
            field.caseOption = $('#fieldCaseOption').val() || undefined;
        }

        if (['richtext', 'textarea'].includes(type)) {
            field.defaultClobValue = $('#fieldDefaultClobValue').val() || undefined
        }

        if (type === 'file') {
            field.fileTypes = $('#fieldFileTypes').val() || undefined;
            field.fileMaxSize = parseFloat($('#fieldFileMaxSize').val()) || undefined;
        }
        if (window.RulesBuilder) {
            field.rules = window.RulesBuilder.collectRules();
        }
        // Add to main or child container
        const target = $('#fieldTarget').val();
        if (target === 'main') {
            window.formFields.push(field);
        } else if (target.startsWith('children-')) {
            const idx = parseInt(target.split('-')[1], 10);
            if (!window.formFields[idx].childFields) window.formFields[idx].childFields = [];
            window.formFields[idx].childFields.push(field);
        }
        renderFieldsList();
        renderPreviewForm();
        renderFormStructure();
        this.reset();
        $('#fieldType').trigger('change');
        updateFieldTargetOptions();
        if (window.RulesBuilder) $('#rulesList').empty();
    });

    // Update field
    $('#updateFieldBtn').on('click', function () {
        const idx = $('#editFieldIdx').val();
        const parentIdx = $('#editFieldParentIdx').val();
        if (idx === '') return;
        const type = $('#fieldType').val();
        const id = $('#editFieldId').val();
        let existingField;
        if (parentIdx !== undefined && parentIdx !== '' && !isNaN(parentIdx)) {
            existingField = window.formFields[parentIdx].childFields[idx];
        } else {
            existingField = window.formFields[idx];
        }
        let field = {
            id: id,
            title: $('#fieldLabel').val(),
            type: type,
            name: $('#fieldLabel').val().replace(/\s+/g, '_').toLowerCase(),
            isrequired: $('#fieldRequired').is(':checked'),
            placeholder: $('#fieldPlaceholder').val(),
            tooltip: $('#fieldTooltip').val(),
            readAccess: $('#fieldReadAccess').val(),
            writeAccess: $('#fieldWriteAccess').val(),
            defaultValue: $('#fieldDefaultValue').val() || undefined,
            wasReactivated: existingField && existingField.wasReactivated === true,
            fieldSysId: (existingField && existingField.fieldSysId) || $('#editFieldSysId').val() || '',
            useFieldDefaults: false,
            createAsReference: false
        };
        if (['selection', 'radio', 'checkboxcollection'].includes(type)) {
            field.options = $('#fieldOptions').val().split(',').map(s => s.trim()).filter(Boolean);
            field.datasource = $('#fieldDatasource').val();
            field.datasourceParamField = $('#datasourceParamField').val() || undefined;
        }
        if (type === 'children') {
            field.childFields = [];
        }


        if (type === 'url' && $('#urlIsParam').is(':checked')) {
            const pattern = $('#urlDefaultPattern').val();
            if (!pattern || !pattern.includes('{key}') || !isValidUrl(pattern)) {
                alert('Default pattern must be a valid URL and include {key}');
                return;
            }
        }

        if (type === 'url') {
            field.urlIsParam = $('#urlIsParam').is(':checked');
            field.urlDefaultPattern = $('#urlDefaultPattern').val() || undefined;
        }


        if ($('#fieldValidation').val()) {
            field.validate = $('#fieldValidation').val();
        }
        if ($('#conditionalField').val()) {
            field.visibleIf = {
                field: $('#conditionalField').val(),
                value: $('#conditionalValue').val()
            };
        }
        if (['text', 'textarea', 'url'].includes(type)) {
            field.minLength = parseInt($('#fieldMinLength').val()) || undefined;
            field.maxLength = parseInt($('#fieldMaxLength').val()) || undefined;
            field.caseOption = $('#fieldCaseOption').val() || undefined;
        }

        if (['richtext', 'textarea'].includes(type)) {
            field.defaultClobValue = $('#fieldDefaultClobValue').val() || undefined;
        }

        if (type === 'file') {
            field.fileTypes = $('#fieldFileTypes').val() || undefined;
            field.fileMaxSize = parseFloat($('#fieldFileMaxSize').val()) || undefined;
        }
        if (window.RulesBuilder) {
            field.rules = window.RulesBuilder.collectRules();
        }
        if (parentIdx !== undefined && parentIdx !== '' && !isNaN(parentIdx)) {
            window.formFields[parentIdx].childFields[idx] = field;
        } else {
            window.formFields[idx] = field;
        }


        renderFieldsList();
        renderPreviewForm();
        renderFormStructure();
        $('#addFieldForm')[0].reset();
        $('#fieldType').trigger('change');
        $('#editFieldIdx').val('');
        $('#editFieldParentIdx').val('');
        $('#editFieldSysId').val('');
        $('#editFieldUseDefaults').val('');
        $('#addFieldBtn').show();
        $('#updateFieldBtn, #cancelEditBtn').hide();
        if (window.RulesBuilder) $('#rulesList').empty();
    });

    // Cancel edit
    $('#cancelEditBtn').on('click', function () {
        $('#addFieldForm')[0].reset();
        $('#fieldType').trigger('change');
        $('#editFieldIdx').val('');
        $('#editFieldParentIdx').val('');
        $('#editFieldSysId').val('');
        $('#editFieldUseDefaults').val('');
        $('#addFieldBtn').show();
        $('#updateFieldBtn, #cancelEditBtn').hide();
        if (window.RulesBuilder) $('#rulesList').empty();
    });

    // Get form values
    $('#getFormValuesBtn').on('click', function (e) {
        e.preventDefault();
        const values = $('#previewForm').dynamicFieldGetValues();
        $('#formOutput').text(JSON.stringify(values, null, 2));
    });

    // Clear All logic
    $('#clearAllBtn').on('click', function () {
        if (confirm('Are you sure you want to remove all fields?')) {
            window.formFields = [];
            renderFieldsList();
            renderPreviewForm();
            renderFormStructure();
            $('#addFieldForm')[0].reset();
            $('#fieldType').trigger('change');
            $('#editFieldIdx').val('');
            $('#editFieldParentIdx').val('');
            $('#addFieldBtn').show();
            $('#updateFieldBtn, #cancelEditBtn').hide();
            if (window.RulesBuilder) $('#rulesList').empty();
        }
    });

    // Edit/remove logic for main and child fields
    $('#fieldsList').on('click', '.edit-btn', function () {
        const $li = $(this).closest('.field-config');
        const idx = $li.data('field-idx');
        const parentIdx = $li.data('parent-idx');
        let field;
        if (parentIdx !== undefined) {
            field = window.formFields[parentIdx].childFields[idx];
        } else {
            field = window.formFields[idx];
        }

        const isrequired = field.isrequired === true || String(field.isrequired).toLowerCase() === 'true';
        const urlIsParam = field.urlIsParam === true || String(field.urlIsParam).toLowerCase() === 'true';

        $('#editFieldId').val(field.id);
        $('#editFieldIdx').val(idx);
        $('#editFieldParentIdx').val(parentIdx !== undefined ? parentIdx : '');
        $('#editFieldSysId').val(field.fieldSysId || '');
        $('#editFieldUseDefaults').val(toBooleanFlag(field.useFieldDefaults) ? 'true' : 'false');
        $('#fieldLabel').val(field.title);
        $('#fieldType').val(field.type).trigger('change');
        $('#fieldRequired').prop('checked', isrequired);
        $('#fieldPlaceholder').val(field.placeholder || '');
        $('#fieldTooltip').val(field.tooltip || '');
        $('#fieldReadAccess').val(field.readAccess || '');
        $('#fieldWriteAccess').val(field.writeAccess || '');
        $('#fieldOptions').val((field.options || []).map(function (opt) {
            if (typeof opt === 'string') return opt;
            return opt.label || opt.value || '';
        }).filter(Boolean).join(', '));
        $('#fieldDatasource').val(field.datasource || '');
        $('#datasourceParamField').val(field.datasourceParamField || '');
        $('#fieldValidation').val(field.validate || '');
        $('#conditionalField').val(field.visibleIf ? field.visibleIf.field : '');
        $('#conditionalValue').val(field.visibleIf ? field.visibleIf.value : '');
        $('#fieldMinLength').val(field.minLength || '');
        $('#fieldMaxLength').val(field.maxLength || '');
        $('#fieldCaseOption').val(field.caseOption || '');
        $('#fieldFileTypes').val(field.fileTypes || '');
        $('#fieldFileMaxSize').val(field.fileMaxSize || '');
        $('#urlIsParam').prop('checked', urlIsParam);
        $('#urlDefaultPattern').val(field.urlDefaultPattern || '');
        if (window.RulesBuilder && field.rules) {
            window.RulesBuilder.initRulesUI(window.formFields).loadRules(field.rules);
        }
        $('#fieldDefaultValue').val(field.defaultValue || '');
        $('#fieldDefaultClobValue').val(field.defaultClobValue || '');
        $('#addFieldBtn').hide();
        $('#updateFieldBtn, #cancelEditBtn').show();
    });

    $('#fieldsList').on('click', '.remove-btn', function () {
        const $li = $(this).closest('.field-config');
        const idx = $li.data('field-idx');
        const parentIdx = $li.data('parent-idx');
        if (confirm('Remove this field?')) {
            if (parentIdx !== undefined) {
                window.formFields[parentIdx].childFields.splice(idx, 1);
            } else {
                window.formFields.splice(idx, 1);
            }
            renderFieldsList();
            renderPreviewForm();
            renderFormStructure();
            updateFieldTargetOptions();
        }
    });

    // Initialize SortableJS for main and child fields
    Sortable.create(document.getElementById('fieldsList'), {
        animation: 150,
        handle: '.move-handle',
        onEnd: function () {
            let newOrder = [];
            $('#fieldsList > .field-config').each(function () {
                let idx = $(this).data('field-idx');
                let parentIdx = $(this).data('parent-idx');
                if (parentIdx !== undefined) {
                    // For child fields, handle reordering inside the parent
                    let parent = window.formFields[parentIdx];
                    let childOrder = [];
                    $(this).parent().find('.field-config').each(function () {
                        let cidx = $(this).data('field-idx');
                        childOrder.push(parent.childFields[cidx]);
                    });
                    parent.childFields = childOrder;
                } else {
                    newOrder.push(window.formFields[idx]);
                }
            });
            if (newOrder.length) {
                window.formFields = newOrder;
            }
            renderFieldsList();
            renderPreviewForm();
            renderFormStructure();
            updateFieldTargetOptions();
        }
    });

    // Initial render
    renderFieldsList();
    renderPreviewForm();
    renderFormStructure();
    $('#fieldType').trigger('change');
    updateFieldTargetOptions();
});