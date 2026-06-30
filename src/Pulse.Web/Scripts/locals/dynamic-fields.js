// scripts/dynamic-fields.js

(function ($) {
    const {
        generateGUID,
        safeJsonParse,
        replaceParam
    } = window.FormUtils;

    // --- Helper: Floating label support ---
    function supportsFloatingLabel(field) {
        if (['text', 'number', 'date', 'email', 'url', 'password'].includes(field.type)) return true;
        if (field.type === 'textarea') return true;
        if (field.type === 'selection' && !field.multiple) return true;
        return false;
    }

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

    // --- Access Control ---
    function hasAccess(field, type, userCode) {
        let access = (type === 'read' ? field.readAccess : field.writeAccess);
        if (typeof access !== 'string') access = '';
        access = access.trim();
        if (access === '*') return true;
        if (!access) return false;
        if (!userCode) return false;
        return access.split(',').map(s => s.trim()).includes(userCode);
    }

    // --- Helper: attach dynamic data-* attributes based on field data + options callback ---
    function applyFieldDataAttributesDynamic($element, field, options) {
        if (!options || typeof options.buildFieldDataAttributes !== 'function') return;

        // user callback: receives field + options, returns plain object { key: value }
        const attrs = options.buildFieldDataAttributes(field, options) || {};
        if (typeof attrs !== 'object') return;

        Object.keys(attrs).forEach(function (key) {
            let val = attrs[key];

            if (val === undefined || val === null) return;

            // Serialize non-primitive values
            if (typeof val === 'object') {
                try {
                    val = JSON.stringify(val);
                } catch (e) {
                    return;
                }
            }

            // Normalized data attribute name: data-{key}
            // Allow caller to pass either "foo-bar" or "fooBar"
            const attrName = 'data-' + String(key)
                .replace(/[A-Z]/g, m => '-' + m.toLowerCase());

            $element.attr(attrName, val);
        });
    }

    // --- Case Enforcement ---
    function enforceCase($input, caseOption) {
        if (!caseOption) return;
        $input.on('input', function () {
            let val = $input.val();
            if (caseOption === 'upper') {
                $input.val(val.toUpperCase());
            } else if (caseOption === 'lower') {
                $input.val(val.toLowerCase());
            }
        });
    }

    // --- Validation Logic ---
    function attachValidation($input, field) {
        if (['text', 'textarea', 'url', 'number'].includes(field.type)) {
            if (field.minLength) $input.attr('minlength', field.minLength);
            if (field.maxLength) $input.attr('maxlength', field.maxLength);
            $input.on('input', function () {
                let val = $input.val();
                let valid = true;
                let msg = '';
                if (field.minLength && val.length < field.minLength) {
                    valid = false; msg = `Minimum length is ${field.minLength}`;
                }
                if (field.maxLength && val.length > field.maxLength) {
                    valid = false; msg = `Maximum length is ${field.maxLength}`;
                }
                if (!valid) {
                    $input.addClass('is-invalid');
                    if ($input.siblings('.invalid-feedback').length === 0) {
                        $input.after(`<div class="invalid-feedback">${msg}</div>`);
                    } else {
                        $input.siblings('.invalid-feedback').text(msg);
                    }
                } else {
                    $input.removeClass('is-invalid');
                    $input.siblings('.invalid-feedback').remove();
                }
            });
        }
        if (field.type === 'url' && !field.urlIsParameter) {
            $input.on('blur', function () {
                let val = $input.val();
                let valid = true;
                let msg = '';
                if (field.isParam && field.defaultPattern) {
                    if (!isValidUrl(field.defaultPattern)) {
                        valid = false;
                        msg = 'Default pattern is not a valid URL (must include http(s):// and {key})';
                    }
                } else {
                    if (val && !isValidUrl(val)) {
                        valid = false;
                        msg = 'Please enter a valid URL (starting with http:// or https://)';
                    }
                }
                if (!valid) {
                    $input.addClass('is-invalid');
                    if ($input.siblings('.invalid-feedback').length === 0) {
                        $input.after(`<div class="invalid-feedback">${msg}</div>`);
                    } else {
                        $input.siblings('.invalid-feedback').text(msg);
                    }
                } else {
                    $input.removeClass('is-invalid');
                    $input.siblings('.invalid-feedback').remove();
                }
            });
        }
        if (field.validate && (field.type === 'text' || field.type === 'number' || field.type === 'url')) {
            $input.on('blur', function () {
                let val = $input.val();
                let isValid = true;
                let errorMsg = '';
                try {
                    isValid = eval(field.validate);
                } catch {
                    isValid = true;
                }
                if (!isValid) {
                    errorMsg = 'Validation failed';
                    $input.addClass('is-invalid');
                    if ($input.siblings('.invalid-feedback').length === 0) {
                        $input.after(`<div class="invalid-feedback">${errorMsg}</div>`);
                    }
                } else {
                    $input.removeClass('is-invalid');
                    $input.siblings('.invalid-feedback').remove();
                }
            });
        }
    }

    // --- Datasource/Options Logic ---
    function getOptions(field, callback, currentValues) {
        if (field.datasource) {
            if (/^https?:\/\//.test(field.datasource)) {
                let url = field.datasource;
                if (field.datasourceParamField && currentValues) {
                    const paramValue = currentValues[field.datasourceParamField] || '';
                    url = replaceParam(url, 'param', paramValue);
                }
                $.getJSON(url, function (data) {
                    callback(data);
                });
            } else {
                let arr = safeJsonParse(field.datasource);
                if (arr) {
                    callback(arr);
                } else {
                    callback(field.options || []);
                }
            }
        } else {
            callback(field.options || []);
        }
    }

    // --- Rule Evaluation ---
    function evaluateRule(rule, currentValues) {
        const fieldVal = currentValues[rule.field];
        switch (rule.operator) {
            case 'eq': return fieldVal == rule.value;
            case 'neq': return fieldVal != rule.value;
            case 'in': return rule.value.split(',').map(v => v.trim()).includes(fieldVal);
            case 'notin': return !rule.value.split(',').map(v => v.trim()).includes(fieldVal);
            default: return false;
        }
    }

    function applyRules(field, $input, $wrapper, currentValues) {
        if (!field.rules || !field.rules.length) return;
        field.rules.forEach(rule => {
            if (evaluateRule(rule, currentValues)) {
                if (rule.action === 'visible') {
                    $wrapper.show();
                }
                if (rule.action === 'enabled') {
                    $input.prop('disabled', false);
                }
                if (rule.action === 'options' && (field.type === 'selection' || field.type === 'radio' || field.type === 'checkboxcollection')) {
                    let opts = safeJsonParse(rule.actionValue);
                    if (opts) {
                        $input.empty();
                        if (field.type === 'selection') $input.append($('<option>', { value: '', text: '' }));
                        opts.forEach(opt => {
                            if (typeof opt === 'object') {
                                $input.append($('<option>', { value: opt.value, text: opt.label }));
                            } else {
                                $input.append($('<option>', { value: opt, text: opt }));
                            }
                        });
                    }
                }
            } else {
                if (rule.action === 'visible') {
                    $wrapper.hide();
                }
                if (rule.action === 'enabled') {
                    $input.prop('disabled', true);
                }
            }
        });
    }

    // --- Floating label field renderers ---
    function renderFloatingField(field, value, canWrite, isDisabled, typeOverride) {
        let type = typeOverride || field.type;
        let $wrapper = $('<div class="form-floating"></div>');
        let $input;
        // Use defaultValue if value is undefined/null/empty string
        let effectiveValue = (value !== undefined && value !== null && value !== '')
            ? value
            : (field.defaultValue !== undefined ? field.defaultValue : field.defaultClobValue !== undefined ? field.defaultClobValue : field.value);

        if (type === 'textarea') {
            $input = $('<textarea>', {
                id: field.guid,
                name: field.name,
                class: 'form-control',
                required: !!field.isrequired,
                placeholder: field.placeholder || field.title || ' ',
                readonly: !canWrite,
                disabled: isDisabled,
                style: 'height: 100px'
            }).val(effectiveValue || '');

            if (field.maxLength) {
                $input.attr('maxlength', field.maxLength);
            }
        } else if (type === 'selection') {
            $input = $('<select>', {
                id: field.guid,
                name: field.name,
                class: 'form-select',
                required: !!field.isrequired,
                disabled: isDisabled,
                placeholder: field.placeholder || field.title || ' '
            });
            $input.append($('<option>', { value: '', text: '' }));
        } else {
            $input = $('<input>', {
                type: (type === 'url' && field.urlIsParameter) ? "text" : type,
                id: field.guid,
                name: field.name,
                class: 'form-control',
                value: effectiveValue || '',
                required: !!field.isrequired,
                placeholder: field.placeholder || field.title || ' ',
                readonly: !canWrite,
                disabled: isDisabled
            });
        }

        // Add help text below the floating box if placeholder/help is provided
        let $help = null;
        let _helper = "";
        if (field.placeholder) {
            _helper = ` <span class="text-muted">(${field.placeholder})</span>`;
            $help = $(`<div class="form-text text-placeholder text-muted ps-3">(${field.placeholder})</div>`);
        }

        let $label = $('<label>', {
            for: field.guid,
            html: field.title + _helper + (field.isrequired ? ' <span class="text-danger">*</span><span class="required-indicator sr-only"> (required)</span> ' : '')
        });
        $wrapper.append($input, $label);

        if (type === 'selection') {
            getOptions(field, function (optionsArr) {
                optionsArr.forEach(opt => {
                    if (typeof opt === 'object') {
                        $input.append($('<option>', { value: opt.value, text: opt.label }));
                    } else {
                        $input.append($('<option>', { value: opt, text: opt }));
                    }
                });
                if (effectiveValue) $input.val(effectiveValue);
            });
        }
        attachValidation($input, field);
        enforceCase($input, field.caseOption);

        return $wrapper;
    }

    // --- Non-floating field renderers (with help text) ---
    function renderRadioField(field, value, canWrite, isDisabled, currentValues, options) {
        let $input = $('<div class=""></div>');
        let effectiveValue = (value !== undefined && value !== null && value !== '')
            ? value
            : (field.defaultValue !== undefined ? field.defaultValue : field.value);
        getOptions(field, function (optionsArr) {
            optionsArr.forEach(opt => {
                let val = typeof opt === 'object' ? opt.value : opt;
                let label = typeof opt === 'object' ? opt.label : opt;
                let $radio = $('<input>', {
                    type: 'radio',
                    name: field.guid,
                    value: val,
                    class: 'form-check-input',
                    checked: effectiveValue === val,
                    disabled: isDisabled
                });
                let $radioLabel = $('<label>', { class: 'form-check-label ms-1 me-3' }).append($radio).append(' ' + label);
                $input.append($radioLabel);
            });
        }, currentValues);
        if (field.placeholder) {
            $input.append(`<div class="form-text text-placeholder text-muted ps-3">${field.placeholder}</div>`);
        }
        return $input;
    }

    function renderCheckboxCollectionField(field, value, canWrite, isDisabled, currentValues, options) {
        let $input = $('<div class=""></div>');
        let effectiveValue = (value !== undefined && value !== null && value !== '')
            ? value
            : (field.defaultValue !== undefined ? field.defaultValue : field.value);
        getOptions(field, function (optionsArr) {
            optionsArr.forEach(opt => {
                let val = typeof opt === 'object' ? opt.value : opt;
                let label = typeof opt === 'object' ? opt.label : opt;
                let $checkbox = $('<input>', {
                    type: 'checkbox',
                    name: field.guid,
                    value: val,
                    class: 'form-check-input',
                    checked: Array.isArray(effectiveValue) && effectiveValue.includes(val),
                    disabled: isDisabled
                });
                let $checkboxLabel = $('<label>', { class: 'form-check-label ms-1 me-3' }).append($checkbox).append(' ' + label);
                $input.append($checkboxLabel);
            });
        }, currentValues);
        if (field.placeholder) {
            $input.append(`<div class="form-text text-placeholder text-muted ps-3">${field.placeholder}</div>`);
        }
        return $input;
    }

    function renderCheckboxField(field, value, canWrite, isDisabled) {
        let $wrapper = $('<div class="form-check"></div>');
        let effectiveValue = (value !== undefined && value !== null && value !== '')
            ? value
            : (field.defaultValue !== undefined ? field.defaultValue : field.value);
        let $input = $('<input>', {
            type: 'checkbox',
            id: field.guid,
            name: field.name,
            class: 'form-check-input',
            checked: !!effectiveValue,
            disabled: isDisabled
        });
        let $label = $('<label>', {
            for: field.guid,
            class: 'form-check-label',
            html: field.title + (field.isrequired ? ' <span class="text-danger">*</span>' : '')
        });
        $wrapper.append($input, $label);
        if (field.placeholder) {
            $wrapper.append(`<div class="form-text text-placeholder text-muted ps-3">${field.placeholder}</div>`);
        }
        return $wrapper;
    }

    function renderRichTextField(field, value, canWrite, isDisabled) {
        let $wrapper = $('<div class="mb-3"></div>');
        let $textarea = $('<textarea>', {
            id: field.guid,
            name: field.name,
            class: 'form-control',
            required: !!field.isrequired,
            placeholder: field.placeholder || field.title || ' ',
            readonly: !canWrite,
            disabled: isDisabled,
            placeholder: field.placeholder,
        }).val(value || field.value || field.defaultClobValue || '');

        let $label = $('<label>', {
            for: field.guid,
            html: field.title + (field.isrequired ? ' <span class="text-danger">*</span><span class="required-indicator sr-only"> (required)</span>' : '')
        });
        $objectContainer = $(`<div class="mb-1"></div>`);
        $objectContainer.append($label);
        $wrapper.append($objectContainer);
        $wrapper.append($textarea);

        // Initialize Summernote after appending to DOM
        setTimeout(function () {
            $textarea.summernote({
                height: 200,
                placeholder: field.placeholder,
                toolbar: [
                    ['style', ['bold', 'italic', 'underline', 'clear']],
                    ['font', ['strikethrough', 'superscript', 'subscript']],
                    ['fontsize', ['fontsize']],
                    ['color', ['color']],
                    ['para', ['ul', 'ol', 'paragraph']],
                    ['insert', ['link', 'picture', 'video']],
                    ['view', ['fullscreen', 'codeview', 'help']]
                ],
                codemirror: { // codemirror options
                    theme: 'monokai'
                },
                disable: isDisabled || !canWrite,
                callbacks: {
                    onInit: function () {
                        if (isDisabled || !canWrite) {
                            $textarea.summernote('disable');
                        }
                    }
                }
            });
        }, 0);

        return $wrapper;
    }

    function renderFileField(field, canWrite, isDisabled) {
        let dzId = field.guid + '-dz';
        let $dzContainer = $('<div>', { class: 'dropzone', id: dzId });
        let $hiddenInput = $('<input>', {
            type: 'hidden',
            id: field.guid,
            name: field.name
        });

        setTimeout(function () {
            let dz = new Dropzone('#' + dzId, {
                url: "/fake-upload",
                autoProcessQueue: false,
                maxFiles: 1,
                clickable: true,
                acceptedFiles: field.fileTypes || null,
                maxFilesize: field.fileMaxSize || null,
                addRemoveLinks: true,
                dictDefaultMessage: "Drop file here or click to upload",
                init: function () {
                    this.on("addedfile", function (file) {
                        if (field.fileTypes) {
                            let valid = false;
                            let types = field.fileTypes.split(',').map(s => s.trim().toLowerCase());
                            valid = types.some(type => {
                                if (type.startsWith('.')) return file.name.toLowerCase().endsWith(type);
                                if (type.endsWith('/*')) return file.type.startsWith(type.replace('/*', ''));
                                return file.type === type;
                            });
                            if (!valid) {
                                this.removeFile(file);
                                alert('Invalid file type!');
                                return;
                            }
                        }
                        if (field.fileMaxSize && file.size > field.fileMaxSize * 1024 * 1024) {
                            this.removeFile(file);
                            alert('File is too large!');
                            return;
                        }
                        $hiddenInput.val(file.name);
                    });
                    this.on("removedfile", function () {
                        $hiddenInput.val('');
                    });
                }
            });
            if (isDisabled) {
                dz.disable();
            }
        }, 0);

        let $wrapper = $('<div class="mb-3"></div>').append($dzContainer, $hiddenInput);
        if (field.placeholder) {
            $wrapper.append(`<div class="form-text text-placeholder text-muted ps-3">${field.placeholder}</div>`);
        }
        return $wrapper;
    }

    function renderChildrenField(field, value, canWrite, isDisabled, currentValues, options) {
        let $input = $('<div class="children-container"></div>');
        let $addChildBtn = $('<button type="button" class="btn btn-sm btn-success mb-2"><i class="bi bi-plus"></i> Add</button>');
        $input.append($addChildBtn);

        function addChildRow(childData = {}) {
            let $row = $('<div class="child-row border p-2 mb-2"></div>');
            (field.childFields || []).forEach(childField => {
                let childFieldCopy = $.extend(true, {}, childField);
                renderField(childFieldCopy, $row, null, childData[childField.name], currentValues, options);
            });
            let $removeBtn = $('<button type="button" class="btn btn-sm btn-danger ms-2"><i class="bi bi-trash"></i> Remove</button>');
            $removeBtn.on('click', function () { $row.remove(); });
            $row.append($removeBtn);
            $input.append($row);
        }

        if (Array.isArray(field.value)) {
            field.value.forEach(childData => addChildRow(childData));
        }
        $addChildBtn.on('click', function () { addChildRow(); });
        if (field.placeholder) {
            $input.append(`<div class="form-text text-placeholder text-muted ps-3">${field.placeholder}</div>`);
        }
        return $input;
    }

    function renderReadOnlyDisplay(field, value) {
        let effectiveValue = (value !== undefined && value !== null && value !== '')
            ? value
            : (field.defaultValue !== undefined
                ? field.defaultValue
                : field.defaultClobValue !== undefined
                    ? field.defaultClobValue
                    : field.value);

        let displayValue = effectiveValue;

        if (field.type === 'checkbox') {
            displayValue = effectiveValue ? 'Yes' : 'No';
        } else if (field.type === 'checkboxcollection') {
            if (Array.isArray(effectiveValue)) {
                displayValue = effectiveValue.join(', ');
            }
        } else if (field.type === 'radio' || field.type === 'selection') {
            if (field.options && effectiveValue != null && effectiveValue !== '') {
                let opts = field.options;
                let found = opts.find(o => {
                    if (typeof o === 'object') return o.value == effectiveValue;
                    return o == effectiveValue;
                });
                if (found) {
                    displayValue = typeof found === 'object' ? found.label : found;
                }
            }
        } else if (field.type === 'file') {
            if (!displayValue) displayValue = '';
        }

        if (displayValue === null || displayValue === undefined) {
            displayValue = '';
        }

        let $wrapper = $('<div class="mb-3" data-field-name="' + (field.name || '') + '"></div>');
        let $labelRow = $('<div class="d-flex justify-content-between align-items-center mb-1"></div>');
        let $title = $('<div class="fw-semibold text-muted small"></div>').text(field.title || field.name || '');
        $labelRow.append($title);

        let $valueContainer = $('<div class="border rounded form-control-sm  bg-light px-3 py-2"></div>');

        if (field.type === 'url') {
            if (displayValue) {
                let href = displayValue;
                // Ensure it has a protocol for href
                if (!/^https?:\/\//i.test(href)) {
                    href = 'https://' + href;
                }
                let $link = $('<a>', {
                    href: href,
                    text: displayValue,
                    target: '_blank',
                    rel: 'noopener noreferrer',
                    class: 'text-decoration-none'
                });
                $valueContainer.empty().append($link);
            } else {
                $valueContainer.append('<span class="text-muted fst-italic">Not specified</span>');
            }
        } else if (field.type === 'richtext') {
            // Will be handled by a dedicated function; this branch is only for safety.
            $valueContainer.html(displayValue || '<span class="text-muted fst-italic">No content</span>');
        } else if (field.type === 'file') {
            if (displayValue) {
                let $fileIcon = $('<i class="bi bi-paperclip me-2"></i>');
                let $fileText = $('<span></span>').text(displayValue);
                $valueContainer.append($fileIcon, $fileText);
            } else {
                $valueContainer.append('<span class="text-muted fst-italic">No file</span>');
            }
        } else if (displayValue === '') {
            $valueContainer.append('<span class="text-muted fst-italic">Not specified</span>');
        } else {
            let withBr = String(displayValue)
                .replace(/\r\n/g, '\n')    // normalize
                .replace(/\r/g, '\n')
                .replace(/\n/g, '<br>');
            $valueContainer.html(withBr);
        }

        $wrapper.append($labelRow, $valueContainer);

        if (field.placeholder) {
            $wrapper.append(`<div class="form-text form-control-sm text-placeholder text-muted ps-1">${field.placeholder}</div>`);
        }

        return $wrapper;
    }

    function renderUrlReadOnlyField(field, value) {
        // 1. Determine "raw" URL value: value → defaultValue → defaultClobValue → ""
        let urlvalue =
            (value !== undefined && value !== null && value !== '')
                ? value
                : (field.defaultValue !== undefined && field.defaultValue !== null && field.defaultValue !== '')
                    ? field.defaultValue
                    : (field.defaultClobValue !== undefined && field.defaultClobValue !== null && field.defaultClobValue !== '')
                        ? field.defaultClobValue
                        : "";

        // 2. Split into multiple values using allowed delimiters
        //    Customize this list as needed: comma, semicolon, newline, whitespace, pipe, etc.
        const delimiterRegex = /[,\n;\|]+/; // split on ',', ';', '\n', '|'
        let rawParts = [];

        if (urlvalue && typeof urlvalue === 'string') {
            rawParts = urlvalue
                .split(delimiterRegex)
                .map(p => p.trim())
                .filter(p => p.length > 0);
        }

        // 3. Map parts to "effectiveValue" (for urlIsParameter / pattern), then render
        let effectiveValues = rawParts.map(part => {
            var key = part;
            var value = part;
            if (field.urlIsParameter && field.urlDefaultPattern) {
                value =  field.urlDefaultPattern.replace("{key}", part);
            }
 

            return {
                key: key,
                value: value
            };
        });

        let $wrapper = $('<div class="mb-3" data-field-name="' + (field.name || '') + '"></div>');

        // Label
        let $label = $('<div class="fw-semibold text-muted small mb-1"></div>')
            .text(field.title || field.name || '');

        let $valueContainer = $('<div></div>');

        // No URLs at all
        if (!effectiveValues.length) {
            $valueContainer.append('<span class="text-muted fst-italic">Not specified</span>');
        } else {
            // Create a pill for each URL
            effectiveValues.forEach(function (effectiveValue) {
                if (!effectiveValue) return;

                // Normalize href
                let href = effectiveValue.value;
                if (!/^https?:\/\//i.test(href)) {
                    href = 'https://' + href;
                }

                // Derive a short readable text
                let labelText = effectiveValue.value;
                try {
                    let u = new URL(href);
                    // show hostname + short path
                    let path = u.pathname === '/' ? '' : u.pathname;
                    labelText = u.hostname + path;
                } catch (e) {
                    labelText = effectiveValue.value;
                }

                // Truncate to avoid breaking layout
                const maxLength = 45;
                let truncated = labelText.length > maxLength
                    ? labelText.slice(0, maxLength - 3) + '...'
                    : labelText;

                // Pill-style link
                let $pill = $('<a>', {
                    href: href,
                    target: '_blank',
                    rel: 'noopener noreferrer',
                    title: effectiveValue.value,
                    class: 'd-inline-flex align-items-center gap-2 px-3 py-2 rounded-pill bg-white border text-decoration-none shadow-sm me-2 mb-2'
                });

                let $icon = $(
                    '<span class="d-inline-flex align-items-center justify-content-center rounded-circle bg-primary-subtle text-primary" style="width:24px;height:24px;">' +
                    '<i class="bi bi-link-45deg"></i>' +
                    '</span>'
                );

                let $textGroup = $('<div class="d-flex flex-column text-start"></div>');
                // Main label — you may want to make this dynamic; using 'Open ATF' as in your original
                let $mainLabel = $('<span class="fw-semibold small"></span>').text(effectiveValue.key);
                let $subLabel = $('<span class="text-muted small text-truncate" style="max-width: 220px;"></span>')
                    .text(truncated);

                $textGroup.append($mainLabel, $subLabel);
                $pill.append($icon, $textGroup);
                $valueContainer.append($pill);
            });
        }

        $wrapper.append($label, $valueContainer);

        if (field.placeholder && !(urlvalue !== undefined && urlvalue !== null && urlvalue !== '')) {
            $wrapper.append(
                '<div class="form-text text-placeholder text-muted ps-1">' +
                field.placeholder +
                '</div>'
            );
        }

        return $wrapper;
    }

    function renderUrlReadOnlyField1(field, value) {

        function splitIfDelimited(str, delimiters = [',', ';', '|']) {
            
            const delimiter = delimiters.find(d => str.includes(d));
             
            //if (!delimiter) {
            //    return str; 
            //}
             
            return str.split(delimiter);
        }

        let urlvalue = ((value !== undefined && value !== null && value !== '') ? value :
            (field.defaultValue !== undefined && field.defaultValue !== null && field.defaultValue !== '') ? field.defaultValue : 
                (field.defaultClobValue !== undefined && field.defaultClobValue !== null && field.defaultClobValue !== '') ? field.defaultClobValue : "")

        let $wrapper = $('<div class="mb-3" data-field-name="' + (field.name || '') + '"></div>');
        

        function renderElement(_field, effectiveValue) {
            let $wrapperContainer = $('<div class="mb-3" data-field-name="' + (field.name || '') + '"></div>');
            let $valueContainer = $('<div></div>');
            // Label
            let $label = $('<div class="fw-semibold text-muted small mb-1"></div>')
                .text(_field.title || _field.name || '');



            if (!effectiveValue) {
                $valueContainer.append('<span class="text-muted fst-italic">Not specified</span>');
            } else {
                // Normalize href
                let href = effectiveValue;
                if (!/^https?:\/\//i.test(href)) {
                    href = 'https://' + href;
                }

                // Derive a short readable text
                let labelText = effectiveValue;
                try {
                    let u = new URL(href);
                    // show hostname + short path
                    let path = u.pathname === '/' ? '' : u.pathname;
                    labelText = u.hostname + path;
                } catch (e) {
                    labelText = effectiveValue;
                }

                // Truncate to avoid breaking layout
                const maxLength = 45;
                let truncated = labelText.length > maxLength
                    ? labelText.slice(0, maxLength - 3) + '...'
                    : labelText;

                // Modern pill-style button/link (aligned with your cards)
                let $pill = $('<a>', {
                    href: href,
                    target: '_blank',
                    rel: 'noopener noreferrer',
                    title: effectiveValue, // full URL on hover
                    // adjust colors to match your theme if needed
                    class: 'd-inline-flex align-items-center gap-2 px-3 py-2 rounded-pill bg-white border text-decoration-none shadow-sm'
                });

                let $icon = $('<span class="d-inline-flex align-items-center justify-content-center rounded-circle bg-primary-subtle text-primary" style="width:24px;height:24px;">' +
                    '<i class="bi bi-link-45deg"></i>' +
                    '</span>');

                let $textGroup = $('<div class="d-flex flex-column text-start"></div>');
                // Main label: “Open ATF” (or any business label)
                let $mainLabel = $('<span class="fw-semibold small"></span>').text('Open ATF');
                // Secondary line: truncated URL
                let $subLabel = $('<span class="text-muted small text-truncate" style="max-width: 220px;"></span>')
                    .text(truncated);

                $textGroup.append($mainLabel, $subLabel);

                $pill.append($icon, $textGroup);
                $valueContainer.append($pill);
            }

            $wrapperContainer.append($label, $valueContainer);


            return $wrapperContainer;
        }

        let _values = splitIfDelimited(urlvalue);

        _values.forEach(v => {
            renderElement(field, v);
        });


        //let effectiveValue = (value !== undefined && value !== null && value !== '')
        //    ? (field.urlIsParameter ? field.urlDefaultPattern.replace("{key}", value) : value)
        //    : (field.defaultValue !== undefined
        //        ? (field.urlIsParameter ? field.urlDefaultPattern.replace("{key}", field.defaultValue) : field.defaultValue)
        //        : field.defaultClobValue !== undefined
        //            ? (field.urlIsParameter ? field.urlDefaultPattern.replace("{key}", field.defaultClobValue) : field.defaultClobValue)
        //            : (field.urlIsParameter ? field.urlDefaultPattern.replace("{key}", field.value) : field.value));

        

        if (field.placeholder && ((value === undefined || value === null || value === ''))) {
            $wrapper.append(`<div class="form-text text-placeholder text-muted ps-1">${field.placeholder}</div>`);
        }

        return $wrapper;
    }

    function renderRichTextReadOnlyField(field, value) {
        // Resolve effective value (prefer explicit value, then defaults)
        let effectiveValue =
            (value !== undefined && value !== null && value !== '')
                ? value
                : (field.defaultValue !== undefined && field.defaultValue !== null && field.defaultValue !== ''
                    ? field.defaultValue
                    : (field.defaultClobValue !== undefined && field.defaultClobValue !== null && field.defaultClobValue !== ''
                        ? field.defaultClobValue
                        : field.value));

        if (typeof effectiveValue === 'string' && /&lt;\/?[a-z][\s\S]*&gt;/i.test(effectiveValue)) {
            effectiveValue = $('<textarea/>').html(effectiveValue).text();
        }

        let $wrapper = $('<div class="mb-3" data-field-name="' + (field.name || '') + '"></div>');

        // Optional global cssClass hook (same pattern as for other fields)
        if (field.cssClass) {
            $wrapper.addClass(field.cssClass);
        }

        let $label = $('<label>', {
            for: field.guid,
            class: 'fw-semibold text-muted small mb-1 d-block',
            html: field.title + (field.isrequired
                ? ' <span class="text-danger">*</span><span class="required-indicator sr-only"> (required)</span>'
                : '')
        });

        // Note: Summernote will replace this textarea with its own editor.
        // We feed the *HTML* content into it.
        let $textarea = $('<textarea>', {
            id: field.guid,
            name: field.name,
            class: 'form-control',
            placeholder: field.placeholder || field.title || ' '
        })
            .val(effectiveValue || '');

        $wrapper.append($label, $textarea);

        if (field.placeholder && !(effectiveValue !== undefined && effectiveValue !== null && effectiveValue !== '')) {
            $wrapper.append(
                `<div class="form-text text-placeholder text-muted ps-1">${field.placeholder}</div>`
            );
        }

         

        // Initialize Summernote in read-only mode
        setTimeout(function () {
            $textarea.summernote({
                height: 200,
                toolbar: [
                    ['view', ['fullscreen', 'codeview', 'help']]
                ],
                codemirror: {
                    theme: 'monokai'
                },
                callbacks: {
                    onInit: function () {
                        $textarea.summernote('code', effectiveValue || '');
                        // Disable editing so it's truly read-only
                        $textarea.summernote('disable');
                    }
                }
            });
        }, 0);

        return $wrapper;
    }

    // --- Main Field Renderer (Floating label for supported types) ---
    function renderField(field, $container, parentName, value, currentValues, options) {
        field.guid = field.guid || generateGUID();
        let fieldName = parentName ? `${parentName}[${field.name}]` : field.name || field.guid;

        options = options || {};
        let canRead = hasAccess(field, 'read', options.userCode);
        let canWrite = hasAccess(field, 'write', options.userCode);
        let isDisabled = !canWrite;

        if (!canRead) return;

        let $input;
        let showField = true;
        if (field.visibleIf && typeof field.visibleIf === 'object') {
            let refVal = currentValues ? currentValues[field.visibleIf.field] : undefined;
            showField = refVal === field.visibleIf.value;
        }

        // READONLY MODE
        if (options.mode === 'READONLY') {
            if (field.type === 'richtext') {
                $input = renderRichTextReadOnlyField(field, value);
            }
            else if (field.type === 'url') {
                $input = renderUrlReadOnlyField(field, value);
            }
            else {
                $input = renderReadOnlyDisplay(field, value);
            }
        } else {
            // WRITE / INTERACTIVE MODE
            if (field.type === 'richtext') {
                $input = renderRichTextField(field, value, canWrite, isDisabled);
            }
            else if (supportsFloatingLabel(field)) {
                if (field.type === 'selection') {
                    $input = renderFloatingField(field, value, canWrite, isDisabled, 'selection');
                } else if (field.type === 'textarea') {
                    $input = renderFloatingField(field, value, canWrite, isDisabled, 'textarea');
                } else if (field.type === 'url') {
                    if (field.isParam && field.defaultPattern) {
                        $input = renderFloatingField({ ...field, type: 'text' }, value, canWrite, isDisabled, 'text');
                    } else {
                        $input = renderFloatingField(field, value, canWrite, isDisabled, 'url');
                    }
                } else {
                    $input = renderFloatingField(field, value, canWrite, isDisabled, field.type);
                }
            } else {
                switch (field.type) {
                    case 'radio':
                        $input = renderRadioField(field, value, canWrite, isDisabled, currentValues, options);
                        break;
                    case 'checkbox':
                        $input = renderCheckboxField(field, value, canWrite, isDisabled);
                        break;
                    case 'checkboxcollection':
                        $input = renderCheckboxCollectionField(field, value, canWrite, isDisabled, currentValues, options);
                        break;
                    case 'file':
                        $input = renderFileField(field, canWrite, isDisabled);
                        break;
                    case 'children':
                        $input = renderChildrenField(field, value, canWrite, isDisabled, currentValues, options);
                        break;
                    default:
                        $input = renderFloatingField(field, value, canWrite, isDisabled, field.type);
                }
            }
        }

        let $inputContainer = $('<div class="mb-3"></div>');

        // In READONLY mode we skip value-change handlers and rule-triggered re-render
        if (options.mode !== 'READONLY') {
            $input.find('input, select, textarea').addBack('input, select, textarea').on('change input', function () {
                let val;
                if (field.type === 'checkbox') {
                    val = $input.find('input[type=checkbox]').is(':checked');
                } else if (field.type === 'radio') {
                    val = $input.find('input[type=radio]:checked').val();
                } else if (field.type === 'checkboxcollection') {
                    val = [];
                    $input.find('input[type=checkbox]:checked').each(function () {
                        val.push($(this).val());
                    });
                } else if (field.type === 'file') {
                    val = $input.find('input[type=hidden]').val() || '';
                } else {
                    val = $input.find('input, select, textarea').val();
                }
                let $form = $container.closest('form');
                let currentVals = $form.data('currentValues') || {};
                currentVals[field.name] = val;
                $form.data('currentValues', currentVals);

                if (field.type !== 'children') {
                    $('#previewForm').dynamicField({
                        fields: window.formFields,
                        userCode: window.currentUserCode,
                        showAccessBadges: options.showAccessBadges,
                        mode: options.mode,
                        beforeBind: options.beforeBind,
                        onComplete: options.onComplete,
                        emptyMessage: options.emptyMessage  // <<< NEW (propagate)
                    });
                }
            });

            applyRules(field, $input.find('input, select, textarea').first(), $input, currentValues || {});
        }

        if (!showField) $input.hide();

        // generic field-name marker
        $input.attr('data-field-name', field.name);

        // apply dynamic data-* attributes built from the field definition
        applyFieldDataAttributesDynamic($input, field, options);

        $inputContainer.append($input);
        $container.append($inputContainer);


        if (field.type === 'richtext' && options.mode === 'READONLY') {
            setTimeout(function () {

                $(`#${field.guid}`).summernote({
                    height: 300,
                    toolbar: [
                        ['view', ['fullscreen', 'help']]
                    ],
                    codemirror: {
                        theme: 'monokai'
                    },
                    callbacks: {
                        onInit: function () {
                 
                            // Make editor read-only
                            $(`#${field.guid}`).summernote('disable');
                        }
                    }
                });
                 
            }, 0);
       
        } 
    }

    // --- Main Plugin Entrypoint ---
    $.fn.dynamicField = function (userOptions) {
        const defaults = {
            fields: [],
            mode: 'WRITE',
            onValueChange: null,
            userCode: null,
            showAccessBadges: true,
            // Hooks
            beforeBind: null,   // function(fields, options) { return transformedFields; }
            onComplete: null,   // function($container, options) { ... }
            displayEmptyMessage: true,
            emptyMessage: 'No Link Form found for this node.', // <<< NEW default

            /**
             * Callback to build any custom data-* attributes from the field definition.
             * It should return an object where:
             *   key   -> data attribute name suffix (without "data-")
             *   value -> attribute value
             *
             * Example:
             * buildFieldDataAttributes: function(field, options) {
             *   return {
             *     "field-type": field.type,
             *     "is-collection": field.type === "checkboxcollection",
             *     "has-rules": field.rules && field.rules.length > 0
             *   };
             * }
             */
            buildFieldDataAttributes: null,

            // NEW: what to include in dynamicFieldGetValues
            includeDataAttributesInValues: true,        // include DOM data-* attributes
            includeBuiltDataAttributesInValues: true    // include attributes from buildFieldDataAttributes
        };
        const options = $.extend(true, {}, defaults, userOptions);

        return this.each(function () {
            const $container = $(this).empty();

            // store options so dynamicFieldGetValues can access them later
            $container.data('dynamicFieldOptions', options);

            let currentValues = {};
            if ($container.is('form')) {
                $container.data('currentValues', currentValues);
            }

            function renderFields(fields, $parentContainer) {
                fields.forEach(field => renderField(field, $parentContainer, null, undefined, currentValues, options));
            }

            // --- beforeBind: transform fields before render ---
            let fieldsToRender = options.fields;
            if (typeof options.beforeBind === 'function') {
                const transformed = options.beforeBind(options.fields, options);
                if (Array.isArray(transformed)) {
                    fieldsToRender = transformed;
                }
            }

            // If there are no fields after transformation, show message and stop
            if (!fieldsToRender || fieldsToRender.length === 0) {          // <<< NEW
                const msg = options.displayEmptyMessage ? options.emptyMessage || 'No Link Form found for this node.' : "";
                const $msg = $(
                    '<div class="text-muted" style="font-size:0.8rem;">' +
                    '<i>' + msg + '</i>' +
                    '</div>'
                );
                $container.append($msg);

                if (typeof options.onComplete === 'function') {
                    options.onComplete($container, options);
                }
                return; // stop for this container
            }

            renderFields(fieldsToRender, $container);

            // --- onComplete: called after all fields are rendered ---
            if (typeof options.onComplete === 'function') {
                options.onComplete($container, options);
            }
        });
    };

    // Helper: collect data-* attributes from a jQuery element as a plain object
    function collectDataAttributes($el) {
        const el = $el[0];
        if (!el || !el.attributes) return {};

        const result = {};
        Array.prototype.forEach.call(el.attributes, function (attr) {
            if (!attr.name || attr.name.indexOf('data-') !== 0) return;

            // Strip "data-" prefix and convert kebab-case to camelCase
            const rawName = attr.name.slice(5);
            const camelName = rawName.replace(/-([a-z])/g, (m, g1) => g1.toUpperCase());
            result[camelName] = attr.value;
        });
        return result;
    }

    // --- Value Extraction ---
    $.fn.dynamicFieldGetValues = function () {
        const $root = this;

        // Options from dynamicField init
        const pluginOptions = $root.data('dynamicFieldOptions') || {};

        const includeDomAttrs = pluginOptions.includeDataAttributesInValues !== false;         // default true
        const includeBuiltAttrs = pluginOptions.includeBuiltDataAttributesInValues !== false;    // default true
        const buildAttrsFn = pluginOptions.buildFieldDataAttributes;

        function buildFieldResult(meta) {
            // Convenience helper so the shape is consistent
            return {
                id: meta.id || null,
                name: meta.name || null,
                type: meta.type || null,
                value: meta.value,
                rawValue: meta.rawValue !== undefined ? meta.rawValue : meta.value,
                multiple: !!meta.multiple,
                dataAttributes: meta.dataAttributes || {},
                builtDataAttributes: meta.builtDataAttributes || {}
            };
        }

        function getSingleInputMeta($input) {
            const tag = ($input.prop('tagName') || '').toLowerCase();
            let type = ($input.attr('type') || '').toLowerCase();

            if (!type && tag === 'textarea') type = 'textarea';
            if (!type && tag === 'select') type = 'select';

            return {
                id: $input.attr('id') || null,
                name: $input.attr('name') || null,
                type
            };
        }

        // Build meta about data-* and buildFieldDataAttributes
        function buildAttributeMeta($wrapper) {
            const meta = {
                dataAttributes: {},
                builtDataAttributes: {}
            };

            if (includeDomAttrs) {
                meta.dataAttributes = collectDataAttributes($wrapper);
            }

            if (includeBuiltAttrs && typeof buildAttrsFn === 'function') {
                const dataAttrs = meta.dataAttributes;
                // Pseudo field: user can use dataAttributes to reconstruct what they need
                const pseudoField = Object.assign({}, dataAttrs);

                try {
                    const computed = buildAttrsFn(pseudoField, pluginOptions) || {};
                    if (computed && typeof computed === 'object') {
                        meta.builtDataAttributes = computed;
                    }
                } catch (e) {
                    // Fail silently; do not break value extraction
                }
            }

            return meta;
        }

        function getFieldValueObject($wrapper) {
            const attrMeta = buildAttributeMeta($wrapper);

            // 1. Direct input/select/textarea with bootstrap classes
            let $input = $wrapper.find('input.form-control, select.form-select, textarea.form-control').first();
            if ($input.length) {
                const meta = getSingleInputMeta($input);
                let value = '';
                let rawValue = '';

                if (meta.type === 'select') {
                    value = $input.val();
                } else if (meta.type === 'number') {
                    rawValue = $input.val();
                    value = rawValue ? parseFloat(rawValue) : '';
                } else if (meta.type === 'checkbox') {
                    value = $input.is(':checked');
                } else if (meta.type === 'file') {
                    const file = $input[0].files && $input[0].files[0];
                    value = file ? file.name : '';
                } else {
                    // text, password, email, url, textarea, etc.
                    value = $input.val();
                }

                return buildFieldResult({
                    id: meta.id,
                    name: meta.name,
                    type: meta.type,
                    value,
                    rawValue,
                    dataAttributes: attrMeta.dataAttributes,
                    builtDataAttributes: attrMeta.builtDataAttributes
                });
            }

            // 2. Children (repeating group)
            let $childrenContainer = $wrapper.children('.children-container');
            if ($childrenContainer.length) {
                let arr = [];
                $childrenContainer.children('.child-row').each(function () {
                    let rowObj = {};
                    $(this).children('[data-field-name]').each(function () {
                        let fieldName = $(this).attr('data-field-name');
                        if (!fieldName) return;
                        rowObj[fieldName] = getFieldValueObject($(this));
                    });
                    // Keep row only if any field has non-empty value
                    if (Object.values(rowObj).some(v => {
                        if (!v) return false;
                        let val = v.value;
                        return val !== "" && val !== undefined && !(Array.isArray(val) && val.length === 0);
                    })) {
                        arr.push(rowObj);
                    }
                });
                return buildFieldResult({
                    id: $wrapper.attr('id') || null,
                    name: $wrapper.attr('data-field-name') || null,
                    type: 'children',
                    value: arr,
                    multiple: true,
                    dataAttributes: attrMeta.dataAttributes,
                    builtDataAttributes: attrMeta.builtDataAttributes
                });
            }

            // 3. Radio group
            let $radioGroup = $wrapper.find('input[type=radio]');
            if ($radioGroup.length) {
                let $checked = $radioGroup.filter(':checked');
                let meta = getSingleInputMeta($radioGroup.first());
                let value = $checked.length ? $checked.val() : '';
                return buildFieldResult({
                    id: meta.id,
                    name: meta.name,
                    type: 'radio',
                    value,
                    dataAttributes: attrMeta.dataAttributes,
                    builtDataAttributes: attrMeta.builtDataAttributes
                });
            }

            // 4. Checkbox group (collection)
            let $checkboxGroup = $wrapper.find('input[type=checkbox]');
            if ($checkboxGroup.length > 1) {
                let meta = getSingleInputMeta($checkboxGroup.first());
                let arr = [];
                $checkboxGroup.filter(':checked').each(function () {
                    arr.push($(this).val());
                });
                return buildFieldResult({
                    id: meta.id,
                    name: meta.name,
                    type: 'checkboxcollection',
                    value: arr,
                    multiple: true,
                    dataAttributes: attrMeta.dataAttributes,
                    builtDataAttributes: attrMeta.builtDataAttributes
                });
            }

            // 5. Single hidden field (used by file widget, etc.)
            let $hiddenFile = $wrapper.find('input[type=hidden]');
            if ($hiddenFile.length) {
                let meta = getSingleInputMeta($hiddenFile.first());
                let value = $hiddenFile.val();
                return buildFieldResult({
                    id: meta.id,
                    name: meta.name,
                    type: 'hidden',
                    value,
                    dataAttributes: attrMeta.dataAttributes,
                    builtDataAttributes: attrMeta.builtDataAttributes
                });
            }

            // 6. Summernote / richtext (detect by summernote wrapper)
            let $summernote = $wrapper.find('textarea.form-control');
            if ($summernote.length && $summernote.next('.note-editor').length) {
                let meta = getSingleInputMeta($summernote.first());
                let value = $summernote.summernote('code');
                return buildFieldResult({
                    id: meta.id,
                    name: meta.name,
                    type: 'richtext',
                    value,
                    dataAttributes: attrMeta.dataAttributes,
                    builtDataAttributes: attrMeta.builtDataAttributes
                });
            }

            // 7. Fallback
            return buildFieldResult({
                id: $wrapper.attr('id') || null,
                name: $wrapper.attr('data-field-name') || null,
                type: 'unknown',
                value: '',
                dataAttributes: attrMeta.dataAttributes,
                builtDataAttributes: attrMeta.builtDataAttributes
            });
        }

        let result = {};

        // Top-level fields
        if (this.find('> [data-field-name]').length > 0) {
            this.find('> [data-field-name]').each(function () {
                let fieldName = $(this).attr('data-field-name');
                if (!fieldName) return;
                result[fieldName] = getFieldValueObject($(this));
            });
        } else {
            // Any depth
            this.find('[data-field-name]').each(function () {
                let fieldName = $(this).attr('data-field-name');
                if (!fieldName) return;
                result[fieldName] = getFieldValueObject($(this));
            });
        }

        return result;
    };

    /**
     * Generate a thumbnail image (PNG data URL) for a form definition.
     * @param {Array} formFields - Array of field definitions.
     * @param {Object} options - Options for rendering (userCode, showAccessBadges, etc).
     * @param {Function} callback - Called with the PNG data URL.
     */
    $.generateFormThumbnail = function (formFields, options, callback) {
        var $hidden = $('#hiddenFormPreview');
        if ($hidden.length === 0) {
            $hidden = $('<div id="hiddenFormPreview"></div>').css({
                position: 'absolute',
                left: '-9999px',
                top: '-9999px',
                width: '400px'
            }).appendTo('body');
        }
        $hidden.empty();
        $hidden.dynamicField($.extend({
            fields: formFields,
            showAccessBadges: false
        }, options));
        html2canvas($hidden[0], { backgroundColor: "#fff", scale: 0.5 }).then(function (canvas) {
            var imgData = canvas.toDataURL("image/png");
            callback(imgData);
        });
    };
})(jQuery);