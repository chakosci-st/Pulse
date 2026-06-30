using log4net; 
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Repositories
{
    public class FormFieldRuleRepository : BaseRepository<FormFieldRule, string>, IFormFieldRuleRepository
    {

        public FormFieldRuleRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(FormFieldRule rule)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<FormFieldRule>(@"
INSERT INTO FORMFIELDRULES (
  FORMFIELDSYSID,
  RULEFIELD,
  RULEOPERATOR,
  RULEVALUE,
  RULEACTION,
  RULEACTIONVALUE, CREATEDBY) 
VALUES (:FORMFIELDSYSID,
  :RULEFIELD,
  :RULEOPERATOR,
  :RULEVALUE,
  :RULEACTION,
  :RULEACTIONVALUE, :CREATEDBY)
RETURNING FORMFIELDRULESYSID INTO :FORMFIELDRULESYSID", rule, "FORMFIELDRULESYSID");
        }

        public override async Task<int> UpdateAsync(FormFieldRule rule)
        {
            return await _dataAccess.SaveDataAsync<FormFieldRule>(@"
UPDATE FORMFIELDRULES 
SET   RULEFIELD=:RULEFIELD,
  RULEOPERATOR=:RULEOPERATOR,
  RULEVALUE=:RULEVALUE,
  RULEACTION=:RULEACTION,
  RULEACTIONVALUE=:RULEACTIONVALUE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE FORMFIELDRULESYSID = :FORMFIELDRULESYSID", rule);
        }

        public async override Task<int> DeleteAsync(string formfieldsysid)
        {
            return await _dataAccess.SaveDataAsync<FormFieldRule>("DELETE FROM FORMFIELDRULES WHERE FORMFIELDRULESYSID = :FORMFIELDRULESYSID", new FormFieldRule { FormFieldRuleSysId = formfieldsysid });
        }

        public async override Task<FormFieldRule> GetAsync(string formfieldrulesysid)
        {
            return await _dataAccess.FindDataAsync<FormFieldRule>("SELECT * FROM FORMFIELDRULES  WHERE FORMFIELDRULESYSID = :FORMFIELDRULESYSID ",
                new FormFieldRule { FormFieldRuleSysId = formfieldrulesysid });
        }
        public async override Task<IEnumerable<FormFieldRule>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<FormFieldRule>(@"SELECT * FROM FORMFIELDRULES ")
              .ContinueWith(t => (IEnumerable<FormFieldRule>)t.Result);
        }
 

    }
}
