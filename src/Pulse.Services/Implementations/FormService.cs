using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pulse.SharedUtilities.Helpers;

namespace Pulse.Services.Implementations
{
    public class FormService : IFormService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IFormRepository _formRepository;
        private readonly IFormFieldRepository _formfieldRepository;
        private readonly IFormFieldOptionRepository _formfieldoptionRepository;
        private readonly IFormFieldRuleRepository _formfieldruleRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IFieldOptionRepository _fieldoptionRepository;
        private readonly IFieldRuleRepository _fieldruleRepository;

        public FormService(
            OracleDataAccessLayer dataAccess,
            IFormRepository formRepository,
            IFormFieldRepository formfieldRepository,
            IFormFieldOptionRepository formfieldoptionRepository,
            IFormFieldRuleRepository formfieldruleRepository,
            IFieldRepository fieldRepository,
            IFieldOptionRepository fieldoptionRepository,
            IFieldRuleRepository fieldruleRepository)
        {
            _dataAccess = dataAccess;
            _formRepository = formRepository;
            _formfieldRepository = formfieldRepository;
            _formfieldoptionRepository = formfieldoptionRepository;
            _formfieldruleRepository = formfieldruleRepository;
            _fieldRepository = fieldRepository;
            _fieldoptionRepository = fieldoptionRepository;
            _fieldruleRepository = fieldruleRepository;
        }

        public async Task<IEnumerable<Form>> GetAllFormsAsync()
        {
            return await _formRepository.GetListAsync();
        }

        public async Task<Form> GetFormByIdAsync(string formsysid)
        {
            return await _formRepository.GetAsync(formsysid);
        }

        public Form GetFormById(string formsysid)
        {
            return _formRepository.Get(formsysid);
        }

        private static bool IsSelectionLikeField(string fieldType)
        {
            return string.Equals(fieldType, "selection", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldType, "radio", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldType, "checkboxcollection", StringComparison.OrdinalIgnoreCase);
        }

        private static Field BuildReferenceFieldFromFormField(FormField source, string loggeduser)
        {
            var mappedOptions = (source.Options ?? new List<FormFieldOption>())
                .Select(o => new FieldOption
                {
                    OptionValue = o.OptionValue,
                    OptionLabel = o.OptionLabel,
                    OrderIndex = o.OrderIndex
                })
                .ToList();

            var mappedRules = (source.Rules ?? new List<FormFieldRule>())
                .Select(r => new FieldRule
                {
                    RuleField = r.RuleField,
                    RuleOperator = r.RuleOperator,
                    RuleValue = r.RuleValue,
                    RuleAction = r.RuleAction,
                    RuleActionValue = r.RuleActionValue
                })
                .ToList();

            return new Field
            {
                FieldName = source.FieldName,
                FieldTitle = source.FieldTitle,
                FieldType = source.FieldType,
                Placeholder = source.Placeholder,
                Tooltip = source.Tooltip, 
                MinLength = source.MinLength,
                MaxLength = source.MaxLength,
                CaseOption = source.CaseOption,
                FileType = source.FileType,
                FileMaxSize = source.FileMaxSize,
                FieldValidate = source.FieldValidate,
                DataSource = source.DataSource,
                DataSourceParamField = source.DataSourceParamField,
                ParentFieldSysId = source.ParentFieldSysId,
                DefaultPattern = source.DefaultPattern,
                UrlIsParam = source.UrlIsParam,
                UrlDefaultPattern = source.UrlDefaultPattern,
                DefaultValue = source.DefaultValue,
                DefaultClobValue = source.DefaultClobValue,
                IsActive = 1, 
                CreatedBy = loggeduser,
                Options = mappedOptions,
                Rules = mappedRules
            };
        }

        private async Task<string> EnsureReferenceFieldAsync(FormField field, string loggeduser)
        {
            if (!string.IsNullOrWhiteSpace(field.FieldSysId) && !field.CreateAsReference)
            {
                return field.FieldSysId;
            }

            var referenceField = BuildReferenceFieldFromFormField(field, loggeduser);
            referenceField.FieldSysId = Guid.NewGuid().ToString();
            await _fieldRepository.AddAsync(referenceField);

            var optionIndex = 0;
            foreach (var option in referenceField.Options)
            {
                option.FieldSysId = referenceField.FieldSysId;
                option.OrderIndex = optionIndex;
                option.CreatedBy = loggeduser;
                await _fieldoptionRepository.AddAsync(option);
                optionIndex++;
            }

            foreach (var rule in referenceField.Rules)
            {
                rule.FieldSysId = referenceField.FieldSysId;
                rule.CreatedBy = loggeduser;
                await _fieldruleRepository.AddAsync(rule);
            }

            field.FieldSysId = referenceField.FieldSysId;
            field.CreateAsReference = false;
            return referenceField.FieldSysId;
        }

        private static void PrepareForInheritance(FormField field)
        {
            field.FieldName = null;
            field.FieldTitle = null;
            field.FieldType = null;
            field.Placeholder = null;
            field.Tooltip = null;
            field.CaseOption = null;
            field.FileType = null;
            field.FieldValidate = null;
            field.DataSource = null;
            field.DataSourceParamField = null;
            field.ParentFieldSysId = null;
            field.DefaultPattern = null;
            field.UrlDefaultPattern = null;
            field.DefaultValue = null;
            field.DefaultClobValue = null;
            field.Options = new List<FormFieldOption>();
            field.Rules = new List<FormFieldRule>();
        }

        private void NormalizeIncomingField(FormField field)
        {
            if (field == null)
            {
                return;
            }

            field.Options = field.Options ?? new List<FormFieldOption>();
            field.Rules = field.Rules ?? new List<FormFieldRule>();

            if (field.UseFieldDefaults)
            {
                PrepareForInheritance(field);
                return;
            }

            if (!IsSelectionLikeField(field.FieldType) && string.IsNullOrWhiteSpace(field.DataSource))
            {
                field.Options = new List<FormFieldOption>();
            }
        }

        public async Task<string> BuildFormAsync(Form form, string loggeduser)
        {
            _dataAccess.BeginTransaction();
            try
            {
                form.CreatedBy = loggeduser;

                var sysid = await this.AddFormAsync(form);
                var orderindex_field = 0;
                foreach (var field in form.Fields)
                {
                    await EnsureReferenceFieldAsync(field, loggeduser);
                    NormalizeIncomingField(field);
                    field.FormSysId = sysid;
                    field.CreatedBy = loggeduser;
                    field.OrderIndex = orderindex_field;
                    var fieldsysid = await _formfieldRepository.AddAsync(field);
                    var orderindex_option = 0;
                    foreach (var option in field.Options)
                    {
                        option.FormFieldSysId = fieldsysid;
                        option.CreatedBy = loggeduser;
                        option.OrderIndex = orderindex_option;
                        await _formfieldoptionRepository.AddAsync(option);
                        orderindex_option++;
                    }

                    foreach (var rule in field.Rules)
                    {
                        rule.FormFieldSysId = fieldsysid;
                        rule.CreatedBy = loggeduser;
                        await _formfieldruleRepository.AddAsync(rule);
                    }

                    orderindex_field++;
                }


                _dataAccess.CommitTransaction();

                return sysid;
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }

        }
        public async System.Threading.Tasks.Task RebuildFormAsync(Form incomingForm, string transactionkey, string loggeduser)
        {
            //Load existing form and all children from DB
            var existingForm = JsonHelper.ParseFormJson<Form>(this.GetCompleteInfoFormByIdAsync(incomingForm.FormSysId).GetAwaiter().GetResult().FormJson);
 

            _dataAccess.BeginTransaction();
            try
            {
                //Update form properties
                //var affectedrows = await this.UpdateFormAsync(existingForm);
                var index = 0;
                //Process incoming fields
                foreach (var incomingField in incomingForm.Fields)
                {
                    await EnsureReferenceFieldAsync(incomingField, loggeduser);
                    NormalizeIncomingField(incomingField);

                    var existingField = existingForm.Fields.FirstOrDefault(f => f.FormFieldSysId == incomingField.FormFieldSysId);

                    if (existingField != null)
                    {
                        // Update properties
                        existingField.FieldName = incomingField.FieldName;
                        existingField.FieldSysId = incomingField.FieldSysId;
                        existingField.FieldTitle = incomingField.FieldTitle;
                        existingField.FieldType = incomingField.FieldType;
                        existingField.Placeholder = incomingField.Placeholder;
                        existingField.Tooltip = incomingField.Tooltip;
                        existingField.IsRequired = incomingField.IsRequired;
                        existingField.MinLength = incomingField.MinLength;
                        existingField.MaxLength = incomingField.MaxLength;
                        existingField.CaseOption = incomingField.CaseOption;
                        existingField.FileType = incomingField.FileType;
                        existingField.FileMaxSize = incomingField.FileMaxSize;
                        existingField.ReadAccess = incomingField.ReadAccess;
                        existingField.WriteAccess = incomingField.WriteAccess;
                        existingField.FieldValidate = incomingField.FieldValidate;
                        existingField.DataSource = incomingField.DataSource;
                        existingField.DataSourceParamField = incomingField.DataSourceParamField;
                        existingField.ParentFieldSysId = incomingField.ParentFieldSysId;
                        existingField.FormSysId = incomingField.FormSysId;
                        existingField.UrlDefaultPattern = incomingField.UrlDefaultPattern;
                        existingField.UrlIsParam = incomingField.UrlIsParam;
                        existingField.IsActive = 1;
                        existingField.OrderIndex = index;
                        existingField.ModifiedBy = loggeduser;
                        await _formfieldRepository.UpdateAsync(existingField);

                        // Update options
                        if (incomingField.UseFieldDefaults)
                        {
                            foreach (var existingOption in existingField.Options)
                            {
                                await _formfieldoptionRepository.DeleteAsync(existingOption.FormFieldOptionSysId);
                            }
                        }

                        var optionIndex = 0;
                        foreach (var incomingOption in incomingField.Options)
                        {

                            var existingOption = existingField.Options.FirstOrDefault(o => o.OptionValue == incomingOption.OptionValue);
                            if (existingOption != null)
                            {
                                // Update
                                existingOption.FormFieldSysId = existingField.FormFieldSysId;
                                existingOption.OptionValue = incomingOption.OptionValue;
                                existingOption.OptionLabel = incomingOption.OptionLabel;
                                existingOption.OrderIndex = optionIndex;
                                existingOption.ModifiedBy = loggeduser;

                                await _formfieldoptionRepository.UpdateAsync(existingOption);
                            }
                            else
                            {
                                incomingOption.FormFieldSysId = existingField.FormFieldSysId;
                                incomingOption.OrderIndex = optionIndex;
                                incomingOption.CreatedBy = loggeduser;
                                // Insert new option
                                await _formfieldoptionRepository.AddAsync(incomingOption);
                            }

                            optionIndex++;
                        }

                           if (!incomingField.UseFieldDefaults)
                           {
                            // Delete removed options
                            var optionsToDelete = existingField.Options
                                .Where(o => !incomingField.Options.Select(i => i.OptionValue).Contains(o.OptionValue))
                                .ToList();

                            foreach (var opt in optionsToDelete)
                                await _formfieldoptionRepository.DeleteAsync(opt.FormFieldOptionSysId);
                           }




                        // Update rules
                        if (incomingField.UseFieldDefaults)
                        {
                            foreach (var existingRule in existingField.Rules)
                            {
                                await _formfieldruleRepository.DeleteAsync(existingRule.FormFieldRuleSysId);
                            }
                        }

                        foreach (var incomingRule in incomingField.Rules)
                        {

                            var existingRule = existingField.Rules.FirstOrDefault(o => o.FormFieldRuleSysId == incomingRule.FormFieldRuleSysId ||
                            (o.RuleField == incomingRule.RuleField
                            && o.RuleOperator == incomingRule.RuleOperator
                            && o.RuleValue == incomingRule.RuleValue
                            && o.RuleAction == incomingRule.RuleAction));
                            if (existingRule != null)
                            {
                                // Update
                                existingRule.RuleField = incomingRule.RuleField;
                                existingRule.RuleOperator = incomingRule.RuleOperator;
                                existingRule.RuleValue = incomingRule.RuleValue;
                                existingRule.RuleAction = incomingRule.RuleAction;
                                existingRule.RuleActionValue = incomingRule.RuleActionValue;
                                existingRule.ModifiedBy = loggeduser;
                                await _formfieldruleRepository.UpdateAsync(existingRule);
                            }
                            else
                            {
                                incomingRule.FormFieldSysId = existingField.FormFieldSysId;
                                incomingRule.CreatedBy = loggeduser;
                                // Insert new option
                                await _formfieldruleRepository.AddAsync(incomingRule);
                            }
                        }
                        if (!incomingField.UseFieldDefaults)
                        {
                            // Delete removed options
                            var rulesToDelete = existingField.Rules
                                .Where(o => !incomingField.Rules.Any(io => (io.FormFieldRuleSysId == o.FormFieldRuleSysId)
                                || (io.RuleField == o.RuleField
                                && io.RuleOperator == o.RuleOperator
                                && io.RuleValue == o.RuleValue
                                && io.RuleAction == o.RuleAction
                                && io.RuleActionValue == o.RuleActionValue)))
                                .ToList();

                            foreach (var opt in rulesToDelete)
                                await _formfieldruleRepository.DeleteAsync(opt.FormFieldRuleSysId);
                        }


                    }
                    else
                    {
                        // Insert new field
                        //existingForm.Fields.Add(incomingField);

                        incomingField.FormSysId = incomingForm.FormSysId;
                        incomingField.OrderIndex = index;
                        incomingField.IsActive = 1;
                        incomingField.CreatedBy = loggeduser;

                        var fieldsysid = await _formfieldRepository.AddAsync(incomingField);
                        var optionIndex = 0;
                        foreach (var option in incomingField.Options)
                        {
                            option.FormFieldSysId = fieldsysid;
                            option.OrderIndex = optionIndex;
                            option.CreatedBy = loggeduser;
                            await _formfieldoptionRepository.AddAsync(option);
                        }

                        foreach (var rule in incomingField.Rules)
                        {
                            rule.FormFieldSysId = fieldsysid;
                            rule.CreatedBy = loggeduser;
                            await _formfieldruleRepository.AddAsync(rule);
                        }
                    }

                    index++;
                }

                // Process removed fields. If referenced in submissions, soft-delete; otherwise hard-delete.
                var incomingExistingFieldIds = new HashSet<string>(
                    incomingForm.Fields
                        .Where(f => !string.IsNullOrWhiteSpace(f.FormFieldSysId))
                        .Select(f => f.FormFieldSysId),
                    StringComparer.OrdinalIgnoreCase);

                var removedFields = existingForm.Fields
                    .Where(f => !string.IsNullOrWhiteSpace(f.FormFieldSysId) && !incomingExistingFieldIds.Contains(f.FormFieldSysId))
                    .ToList();

                foreach (var removedField in removedFields)
                {
                    var isReferenced = await _formfieldRepository.IsReferencedAsync(removedField.FormFieldSysId);
                    if (isReferenced)
                    {
                        await _formfieldRepository.ChangeStatusAsync(removedField.FormFieldSysId, 0, loggeduser);
                        continue;
                    }

                    foreach (var option in removedField.Options)
                        await _formfieldoptionRepository.DeleteAsync(option.FormFieldOptionSysId);

                    foreach (var rule in removedField.Rules)
                        await _formfieldruleRepository.DeleteAsync(rule.FormFieldRuleSysId);

                    await _formfieldRepository.DeleteAsync(removedField.FormFieldSysId);
                }


                _dataAccess.CommitTransaction();

            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);
            }


        }

        private async Task<string> AddFormAsync(Form form)
        {
            return await _formRepository.AddAsync(form);

        }

        private async Task<int> UpdateFormAsync(Form form)
        {

            var rowsaffected = await _formRepository.UpdateAsync(form);

            // Publish the UserUpdatedEvent
            //var formUpdatedEvent = new FormUpdatedEvent
            //{
            //    FormCode = form.FormCode,
            //    ActionBy = form.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(formUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public async Task<int> DeleteFormAsync(string formsysid, string userid)
        {

            try
            {
                //GET FORM INFO
                var obj = await _formRepository.GetAsync(formsysid);


                //SET USER WHO DELETES THE Form
                obj.ModifiedBy = userid;
                await _formRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _formRepository.DeleteAsync(formsysid);


                // Publish the UserUpdatedEvent
                //var formDeletedEvent = new FormDeletedEvent
                //{
                //    FormCode = form.FormCode,
                //    ActionBy = form.CreatedBy
                //};

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(formDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<FormExtended> GetCompleteInfoFormByIdAsync(string formsysid)
        {
            return await _formRepository.GetCompleteInfoAsync(formsysid);
        }




        public async Task<PagedResult<FormExtended>> GetPagedFormsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _formRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }

        public async System.Threading.Tasks.Task UpdateAsync(Form incomingForm, string transactionkey, string loggeduser)
        {
            //Load existing form and all children from DB
            var existingForm = await this.GetFormByIdAsync(incomingForm.FormSysId);
            existingForm.FormName = incomingForm.FormName;
            existingForm.FormDescription = incomingForm.FormDescription;
            existingForm.ModifiedBy = loggeduser;
            existingForm.TransactionKey = transactionkey;
            //Update form properties
            var affectedrows = await this.UpdateFormAsync(existingForm);

        }

        public async System.Threading.Tasks.Task ChangeStatusAsync(Form incomingForm, string transactionkey, string loggeduser)
        {
            //Load existing form and all children from DB
            var existingForm = await this.GetFormByIdAsync(incomingForm.FormSysId);
            existingForm.IsActive = incomingForm.IsActive;
            existingForm.ModifiedBy = loggeduser;
            existingForm.TransactionKey = transactionkey;
            //Update form properties
            var affectedrows = await this.UpdateFormAsync(existingForm);
        }

        public async Task ChangeFieldStatusAsync(string formFieldSysId, bool isActive, string loggeduser)
        {
            var existingField = await _formfieldRepository.GetAsync(formFieldSysId);
            if (existingField == null)
            {
                throw new KeyNotFoundException("Form field not found.");
            }

            await _formfieldRepository.ChangeStatusAsync(formFieldSysId, isActive ? 1 : 0, loggeduser);
        }
    }
}
