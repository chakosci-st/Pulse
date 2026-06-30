// scripts/rules-builder.js

window.RulesBuilder = (function () {
    // Initialize the rules UI for the designer
    function initRulesUI(fields) {
        // Show/hide the rules group based on available fields
        $('#rulesGroup').toggle(fields.length > 0);

        // Populate all .rule-field selects with current fields
        function populateRuleFieldOptions() {
            $('#rulesList .rule-field').each(function () {
                const $sel = $(this).empty();
                fields.forEach(f => {
                    $sel.append(`<option value="${f.name}">${f.title}</option>`);
                });
            });
        }

        // Add a new rule row
        function addRuleRow(rule) {
            const $row = $(`
        <div class="input-group mb-2 rule-row">
          <select class="form-select rule-field"></select>
          <select class="form-select rule-operator">
            <option value="eq">=</option>
            <option value="neq">≠</option>
            <option value="in">in</option>
            <option value="notin">not in</option>
          </select>
          <input type="text" class="form-control rule-value" placeholder="Value">
          <select class="form-select rule-action">
            <option value="visible">Visible</option>
            <option value="enabled">Enabled</option>
            <option value="options">Options</option>
          </select>
          <input type="text" class="form-control rule-action-value" placeholder="(for options)">
          <button type="button" class="btn btn-outline-danger rule-remove"><i class="bi bi-x"></i></button>
        </div>
      `);

            // Set values if editing
            if (rule) {
                $row.find('.rule-field').val(rule.field);
                $row.find('.rule-operator').val(rule.operator);
                $row.find('.rule-value').val(rule.value);
                $row.find('.rule-action').val(rule.action);
                $row.find('.rule-action-value').val(rule.actionValue);
            }

            $('#rulesList').append($row);
            populateRuleFieldOptions();
            if (rule) {
                $row.find('.rule-field').val(rule.field);
                $row.find('.rule-operator').val(rule.operator);
                $row.find('.rule-value').val(rule.value);
                $row.find('.rule-action').val(rule.action);
                $row.find('.rule-action-value').val(rule.actionValue);
            }
        }

        // Add rule button
        $('#addRuleBtn').off('click').on('click', function () {
            addRuleRow();
        });

        // Remove rule button
        $('#rulesList').off('click', '.rule-remove').on('click', '.rule-remove', function () {
            $(this).closest('.rule-row').remove();
        });

        // If you want to load existing rules (for edit)
        function loadRules(rules) {
            $('#rulesList').empty();
            if (Array.isArray(rules)) {
                rules.forEach(rule => addRuleRow(rule));
            }
        }

        // Expose for loading rules when editing a field
        return {
            loadRules
        };
    }

    // Collect all rules from the UI
    function collectRules() {
        const rules = [];
        $('#rulesList .rule-row').each(function () {
            rules.push({
                field: $(this).find('.rule-field').val(),
                operator: $(this).find('.rule-operator').val(),
                value: $(this).find('.rule-value').val(),
                action: $(this).find('.rule-action').val(),
                actionValue: $(this).find('.rule-action-value').val()
            });
        });
        return rules;
    }

    // Expose API
    return {
        initRulesUI,
        collectRules
    };
})();