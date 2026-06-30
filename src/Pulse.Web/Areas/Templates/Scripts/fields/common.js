(function () {
    function generateGuid() {
        if (window.FormUtils && typeof window.FormUtils.generateGUID === "function") {
            return window.FormUtils.generateGUID();
        }

        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0;
            var v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    function ensureFieldSysId() {
        var current = $("#FieldSysId").val();
        if (current && current.trim()) {
            return current.trim();
        }

        var generated = generateGuid();
        $("#FieldSysId").val(generated);
        return generated;
    }

    function safeParseJsonArray(input) {
        if (!input) {
            return [];
        }

        try {
            var parsed = JSON.parse(input);
            return Array.isArray(parsed) ? parsed : [];
        } catch (error) {
            return [];
        }
    }

    function sanitizeFieldName(value) {
        if (!value) {
            return "";
        }

        return String(value)
            .trim()
            .replace(/\s+/g, "_")
            .replace(/[^a-zA-Z0-9_]/g, "")
            .toLowerCase();
    }

    function parseOptionsText(value) {
        if (!value) {
            return [];
        }

        return value
            .split(",")
            .map(function (item) { return item.trim(); })
            .filter(Boolean);
    }

    function toNullableNumber(value) {
        if (value === undefined || value === null || value === "") {
            return null;
        }

        var parsed = Number(value);
        return Number.isFinite(parsed) ? parsed : null;
    }

    function normalizeOptionPayload(options) {
        return (options || []).map(function (option, index) {
            if (typeof option === "object") {
                var optionValue = option.value || option.OptionValue || option.optionValue || option.label || option.OptionLabel || option.optionLabel;
                var optionLabel = option.label || option.OptionLabel || option.optionLabel || optionValue;
                return {
                    OptionValue: optionValue,
                    OptionLabel: optionLabel,
                    OrderIndex: index + 1
                };
            }

            return {
                OptionValue: option,
                OptionLabel: option,
                OrderIndex: index + 1
            };
        });
    }

    function normalizeRulePayload(rules) {
        return (rules || []).map(function (rule) {
            return {
                RuleField: rule.field || rule.RuleField || "",
                RuleOperator: rule.operator || rule.RuleOperator || "eq",
                RuleValue: rule.value || rule.RuleValue || "",
                RuleAction: rule.action || rule.RuleAction || "visible",
                RuleActionValue: rule.actionValue || rule.RuleActionValue || ""
            };
        });
    }

    function applyTypeVisibility(type) {
        var isOptionType = ["selection", "radio", "checkboxcollection"].indexOf(type) >= 0;
        var isTextType = ["text", "textarea", "url"].indexOf(type) >= 0;
        var isValidationType = ["text", "number", "url"].indexOf(type) >= 0;
        var isFileType = type === "file";
        var isUrlType = type === "url";
        var isClobType = type === "textarea" || type === "richtext";

        $("#optionsGroup").toggle(isOptionType);
        $("#datasourceGroup").toggle(isOptionType);
        $("#datasourceParamGroup").toggle(isOptionType);
        $("#minLengthGroup, #maxLengthGroup, #caseOptionGroup").toggle(isTextType);
        $("#validationGroup").toggle(isValidationType);
        $("#fileTypesGroup, #fileMaxSizeGroup").toggle(isFileType);
        $("#urlParamGroup").toggle(isUrlType);
        $("#fieldDefaultClobValueGroup").toggle(isClobType);
        $("#fieldDefaultValueGroup").toggle(!isClobType);
    }

    function buildDesignerField() {
        var type = $("#fieldType").val();
        var title = $("#fieldTitle").val().trim();
        var rawName = $("#fieldName").val().trim();
        var name = rawName || sanitizeFieldName(title);
        var options = parseOptionsText($("#fieldOptions").val());
        var rules = safeParseJsonArray($("#fieldRulesJson").val());

        var field = {
            title: title,
            name: name,
            type: type,
            placeholder: $("#fieldPlaceholder").val().trim() || undefined,
            tooltip: $("#fieldTooltip").val().trim() || undefined,
            isrequired: false,
            readAccess: "*",
            writeAccess: "*",
            minLength: toNullableNumber($("#fieldMinLength").val()) || undefined,
            maxLength: toNullableNumber($("#fieldMaxLength").val()) || undefined,
            caseOption: $("#fieldCaseOption").val() || undefined,
            validate: $("#fieldValidation").val().trim() || undefined,
            datasource: $("#fieldDatasource").val().trim() || undefined,
            datasourceParamField: $("#fieldDatasourceParamField").val().trim() || undefined,
            urlIsParam: $("#urlIsParam").is(":checked"),
            urlDefaultPattern: $("#urlDefaultPattern").val().trim() || undefined,
            fileTypes: $("#fieldFileTypes").val().trim() || undefined,
            fileMaxSize: toNullableNumber($("#fieldFileMaxSize").val()) || undefined,
            defaultValue: $("#fieldDefaultValue").val() || undefined,
            defaultClobValue: $("#fieldDefaultClobValue").val() || undefined,
            options: options,
            rules: rules
        };

        if (!field.urlIsParam) {
            field.urlDefaultPattern = undefined;
        }

        field.guid = field.guid || field.name || field.title || generateGuid();
        field.FieldSysId = field.FieldSysId || field.guid;
        field.FieldTitle = field.title;
        field.FieldName = field.name;
        field.FieldType = field.type;
        field.IsRequired = field.isrequired;
        field.UrlIsParameter = field.urlIsParam;
        field.DefaultPattern = field.urlDefaultPattern;
        field.ReadAccess = field.readAccess;
        field.WriteAccess = field.writeAccess;

        return field;
    }

    function buildApiPayload(existingField) {
        var designed = buildDesignerField();
        var fieldId = ensureFieldSysId();

        return {
            id: fieldId,
            name: designed.name,
            title: designed.title,
            type: designed.type,
            placeholder: designed.placeholder || null,
            tooltip: designed.tooltip || null,
            isrequired: false,
            minLength: designed.minLength || 0,
            maxLength: designed.maxLength || 0,
            caseOption: designed.caseOption || null,
            fileTypes: designed.fileTypes || null,
            fileMaxSize: designed.fileMaxSize || 0,
            validate: designed.validate || null,
            datasource: designed.datasource || null,
            datasourceParamField: designed.datasourceParamField || null,
            parentFieldId: existingField && existingField.parentFieldId ? existingField.parentFieldId : null,
            urlIsParam: !!designed.urlIsParam,
            urlIsParameter: !!designed.urlIsParam,
            urlDefaultPattern: designed.urlDefaultPattern || null,
            defaultPattern: existingField && existingField.defaultPattern ? existingField.defaultPattern : null,
            defaultValue: designed.defaultValue || null,
            defaultClobValue: designed.defaultClobValue || null,
            isActive: existingField && typeof existingField.isActive === "boolean" ? existingField.isActive : true,
            transactionKey: $("#TransactionKey").val() || (existingField ? existingField.transactionKey : null),
            options: normalizeOptionPayload(designed.options),
            rules: normalizeRulePayload(designed.rules)
        };
    }

    function updateDesignerSummary(field) {
        var title = field.title || "Untitled field";
        var type = field.type || "text";
        var requiredText = field.isrequired ? "Required" : "Optional";
        $("#DesignerSummary").val(title + " | " + type + " | " + requiredText);
    }

    function refreshDesignerPreview() {
        var field = buildDesignerField();
        field.urlIsParameter = !!field.urlIsParam;

        if (!field.title) {
            $("#previewWriteForm").html('<div class="text-muted">Add a field label to preview.</div>');
            $("#previewReadForm").html('<div class="text-muted">Add a field label to preview.</div>');
            $("#fieldStructure").text("{}");
            $("#FieldJson").val("");
            $("#DesignerSummary").val("Configure field details below");
            return field;
        }

        $("#previewWriteForm").empty().dynamicField({
            fields: [field],
            userCode: "*",
            mode: "WRITE"
        });

        $("#previewReadForm").empty().dynamicField({
            fields: [field],
            userCode: "*",
            mode: "READONLY"
        });

        var fieldJson = JSON.stringify(field, null, 2);
        $("#fieldStructure").text(fieldJson);
        $("#FieldJson").val(fieldJson);
        updateDesignerSummary(field);
        return field;
    }

    function renderFieldPreviewInModes(field, writeSelector, readSelector) {
        var previewField = $.extend(true, {}, field, {
            urlIsParameter: !!field.urlIsParam,
            readAccess: field.readAccess || "*",
            writeAccess: field.writeAccess || "*",
            FieldSysId: field.FieldSysId || field.id || field.guid || generateGuid(),
            FieldTitle: field.title,
            FieldName: field.name,
            FieldType: field.type,
            IsRequired: field.isrequired,
            DefaultPattern: field.urlDefaultPattern,
            UrlIsParameter: !!field.urlIsParam
        });

        $(writeSelector).empty().dynamicField({
            fields: [previewField],
            userCode: "*",
            mode: "WRITE"
        });

        $(readSelector).empty().dynamicField({
            fields: [previewField],
            userCode: "*",
            mode: "READONLY"
        });

        return previewField;
    }

    function bindDesignerEvents() {
        $("#fieldType").on("change", function () {
            applyTypeVisibility($(this).val());
            refreshDesignerPreview();
        });

        $("#fieldDesignerConfig").on("input change", "input, select, textarea", function () {
            if (this.id === "fieldTitle" && !$("#fieldName").val().trim()) {
                $("#fieldName").val(sanitizeFieldName($("#fieldTitle").val()));
            }

            refreshDesignerPreview();
        });

        $("#refreshFieldPreviewBtn").on("click", function () {
            refreshDesignerPreview();
        });

        $("#copyFieldJsonBtn").on("click", function () {
            var jsonText = $("#fieldStructure").text();
            if (!jsonText) {
                return;
            }

            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(jsonText).then(function () {
                    toastr.success("Field JSON copied to clipboard.");
                }).catch(function () {
                    toastr.error("Unable to copy field JSON.");
                });
                return;
            }

            var textarea = document.createElement("textarea");
            textarea.value = jsonText;
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand("copy");
            document.body.removeChild(textarea);
            toastr.success("Field JSON copied to clipboard.");
        });
    }

    function setDesignerField(field) {
        var normalizedOptions = (field.options || []).map(function (option) {
            if (typeof option === "object") {
                return option.OptionValue || option.optionValue || option.value || option.OptionLabel || option.optionLabel || option.label;
            }

            return option;
        });

        var normalizedRules = (field.rules || []).map(function (rule) {
            return {
                field: rule.field || rule.RuleField || "",
                operator: rule.operator || rule.RuleOperator || "eq",
                value: rule.value || rule.RuleValue || "",
                action: rule.action || rule.RuleAction || "visible",
                actionValue: rule.actionValue || rule.RuleActionValue || ""
            };
        });

        $("#fieldTitle").val(field.title || "");
        $("#fieldName").val(field.name || "");
        $("#fieldType").val(field.type || "text");
        $("#fieldPlaceholder").val(field.placeholder || "");
        $("#fieldTooltip").val(field.tooltip || "");
        $("#fieldMinLength").val(field.minLength || "");
        $("#fieldMaxLength").val(field.maxLength || "");
        $("#fieldCaseOption").val(field.caseOption || "");
        $("#fieldOptions").val(normalizedOptions.join(", "));
        $("#fieldDatasource").val(field.datasource || "");
        $("#fieldDatasourceParamField").val(field.datasourceParamField || "");
        $("#fieldValidation").val(field.validate || "");
        $("#fieldFileTypes").val(field.fileTypes || "");
        $("#fieldFileMaxSize").val(field.fileMaxSize || "");
        $("#urlIsParam").prop("checked", !!field.urlIsParam);
        $("#urlDefaultPattern").val(field.urlDefaultPattern || "");
        $("#fieldDefaultValue").val(field.defaultValue || "");
        $("#fieldDefaultClobValue").val(field.defaultClobValue || "");
        $("#fieldRulesJson").val(normalizedRules.length ? JSON.stringify(normalizedRules, null, 2) : "");

        applyTypeVisibility($("#fieldType").val());
        refreshDesignerPreview();
    }

    function resetDesigner() {
        $("#singleFieldDesigner").find("input[type='text'], input[type='number'], textarea").val("");
        $("#fieldType").val("text");
        $("#urlIsParam").prop("checked", false);
        applyTypeVisibility("text");
        refreshDesignerPreview();
    }

    function normalizeApiField(apiField) {
        var normalized = {
            id: apiField.id || apiField.FieldSysId,
            name: apiField.name || apiField.FieldName,
            title: apiField.title || apiField.FieldTitle,
            type: apiField.type || apiField.FieldType,
            placeholder: apiField.placeholder || apiField.Placeholder,
            tooltip: apiField.tooltip || apiField.Tooltip,
            isrequired: false,
            minLength: apiField.minLength || apiField.MinLength,
            maxLength: apiField.maxLength || apiField.MaxLength,
            caseOption: apiField.caseOption || apiField.CaseOption,
            fileTypes: apiField.fileTypes || apiField.FileType,
            fileMaxSize: apiField.fileMaxSize || apiField.FileMaxSize,
            validate: apiField.validate || apiField.FieldValidate,
            datasource: apiField.datasource || apiField.DataSource,
            datasourceParamField: apiField.datasourceParamField || apiField.DataSourceParamField,
            readAccess: "*",
            writeAccess: "*",
            parentFieldId: apiField.parentFieldId || apiField.ParentFieldSysId,
            urlIsParam: apiField.urlIsParam === true || apiField.UrlIsParameter === true,
            urlIsParameter: apiField.urlIsParam === true || apiField.UrlIsParameter === true,
            urlDefaultPattern: apiField.urlDefaultPattern || apiField.UrlDefaultPattern,
            defaultPattern: apiField.defaultPattern || apiField.DefaultPattern,
            defaultValue: apiField.defaultValue || apiField.DefaultValue,
            defaultClobValue: apiField.defaultClobValue || apiField.DefaultClobValue,
            isActive: typeof apiField.isActive === "boolean" ? apiField.isActive : apiField.IsActive,
            transactionKey: apiField.transactionKey || apiField.TransactionKey,
            options: apiField.options || apiField.Options || [],
            rules: apiField.rules || apiField.Rules || []
        };

        normalized.guid = normalized.id || normalized.name || normalized.title || generateGuid();
        normalized.FieldSysId = normalized.id || normalized.guid;
        normalized.FieldTitle = normalized.title;
        normalized.FieldName = normalized.name;
        normalized.FieldType = normalized.type;
        normalized.IsRequired = normalized.isrequired;
        normalized.UrlIsParameter = normalized.urlIsParameter;
        normalized.DefaultPattern = normalized.urlDefaultPattern;
        normalized.ReadAccess = normalized.readAccess;
        normalized.WriteAccess = normalized.writeAccess;

        return normalized;
    }

    window.FieldsDesigner = {
        bindDesignerEvents: bindDesignerEvents,
        buildApiPayload: buildApiPayload,
        refreshDesignerPreview: refreshDesignerPreview,
        renderFieldPreviewInModes: renderFieldPreviewInModes,
        setDesignerField: setDesignerField,
        resetDesigner: resetDesigner,
        normalizeApiField: normalizeApiField,
        applyTypeVisibility: applyTypeVisibility,
        ensureFieldSysId: ensureFieldSysId
    };
})();
