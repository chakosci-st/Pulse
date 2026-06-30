using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IFieldOptionRepository _fieldoptionRepository;
        private readonly IFieldRuleRepository _fieldruleRepository;

        public FieldService(IFieldRepository fieldRepositorysitory, IFieldOptionRepository fieldoptionRepository, IFieldRuleRepository fieldruleRepository)
        {
            _fieldRepository = fieldRepositorysitory;
            _fieldoptionRepository = fieldoptionRepository;
            _fieldruleRepository = fieldruleRepository;
        }

        public async Task<int> AddAsync(Field field)
        {
            field.FieldSysId = Guid.NewGuid().ToString();

            var guid = await _fieldRepository.AddAsync(field);
            var orderindex_option = 0;
            foreach (var option in field.Options)
            {
                option.FieldSysId = field.FieldSysId;
                option.CreatedBy = field.CreatedBy;
                option.OrderIndex = orderindex_option;
                await _fieldoptionRepository.AddAsync(option);
                orderindex_option++;
            }

            foreach (var rule in field.Rules)
            {
                rule.FieldSysId = field.FieldSysId;
                rule.CreatedBy = field.CreatedBy;
                await _fieldruleRepository.AddAsync(rule);
            }



            // Publish the UserCreatedEvent
            //var fieldCreatedEvent = new FieldCreatedEvent
            //{
            //    FieldCode = field.FieldCode,
            //    ActionBy = field.CreatedBy
            //};

            if (!string.IsNullOrEmpty(guid))
            {
                //_eventPublisher.Publish(fieldCreatedEvent);
                return 1;
            }

            return 0;
        }
        public async Task<int> UpdateAsync(Field field)
        {
            var rowsaffected = await _fieldRepository.UpdateAsync(field);
            var currentOptions = await _fieldoptionRepository.GetListAsync(fieldsysid: field.FieldSysId);
            var currentRules = await _fieldruleRepository.GetListAsync(fieldsysid: field.FieldSysId);

            // Update options
            var optionIndex = 0;
            foreach (var incomingOption in field.Options)
            {

                incomingOption.FieldSysId = field.FieldSysId;
                incomingOption.OrderIndex = optionIndex;
                incomingOption.CreatedBy = field.ModifiedBy;
                // Insert new option
                await _fieldoptionRepository.AddAsync(incomingOption);

                optionIndex++;
            }



            // Delete removed options
            var optionsToDelete = currentOptions
                   .Where(o => !field.Options.Select(i => i.OptionValue).Contains(o.OptionValue))
                   .ToList();

            foreach (var opt in optionsToDelete)
                await _fieldoptionRepository.DeleteAsync(opt.FieldOptionSysId);




            // Update rules
            foreach (var incomingRule in field.Rules)
            {

                var existingRule = currentRules.FirstOrDefault(o => o.FieldRuleSysId == incomingRule.FieldRuleSysId ||
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
                    existingRule.ModifiedBy = field.ModifiedBy;
                    await _fieldruleRepository.UpdateAsync(existingRule);
                }
                else
                {
                    incomingRule.FieldSysId = field.FieldSysId;
                    incomingRule.CreatedBy = field.ModifiedBy;
                    // Insert new option
                    await _fieldruleRepository.AddAsync(incomingRule);
                }
            }
            // Delete removed options
            var rulesToDelete = currentRules
                .Where(o => !field.Rules.Any(io => (io.FieldRuleSysId == o.FieldRuleSysId)
                || (io.RuleField == o.RuleField
                && io.RuleOperator == o.RuleOperator
                && io.RuleValue == o.RuleValue
                && io.RuleAction == o.RuleAction
                && io.RuleActionValue == o.RuleActionValue)))
                .ToList();

            foreach (var opt in rulesToDelete)
                await _fieldruleRepository.DeleteAsync(opt.FieldRuleSysId);




            // Publish the UserUpdatedEvent
            //var fieldUpdatedEvent = new FieldUpdatedEvent
            //{
            //    FieldCode = field.FieldCode,
            //    ActionBy = field.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(fieldUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }
        public async Task<int> DeleteAsync(string fieldsysid, string userid)
        {
            try
            {
                //GET PLANT INFO
                var obj = await _fieldRepository.GetAsync(fieldsysid);


                //SET USER WHO DELETES THE Field
                obj.ModifiedBy = userid;
                await _fieldRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _fieldRepository.DeleteAsync(fieldsysid);


                // Publish the UserUpdatedEvent
                //var fieldDeletedEvent = new FieldDeletedEvent
                //{
                //    FieldCode = field.FieldCode,
                //    ActionBy = field.CreatedBy
                //};

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(fieldDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<IEnumerable<Field>> GetAllFieldsAsync()
        {
            return await _fieldRepository.GetListAsync();
        }

        public async Task<Field> GetFieldByIdAsync(string fieldsysid)
        {
            var field = await _fieldRepository.GetAsync(fieldsysid);
            if (field == null)
            {
                return null;
            }

            field.Options = (await _fieldoptionRepository.GetListAsync(fieldsysid)).ToList();
            field.Rules = (await _fieldruleRepository.GetListAsync(fieldsysid)).ToList();
            return field;
        }

        public async Task<PagedResult<FieldWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _fieldRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
    }
}
