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
    public class FieldRuleRepository : BaseRepository<FieldRule, string>, IFieldRuleRepository
    {

        public FieldRuleRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(FieldRule rule)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<FieldRule>(@"
INSERT INTO FieldRULES (
  FieldSYSID,
  RULEFIELD,
  RULEOPERATOR,
  RULEVALUE,
  RULEACTION,
  RULEACTIONVALUE, CREATEDBY) 
VALUES (:FieldSYSID,
  :RULEFIELD,
  :RULEOPERATOR,
  :RULEVALUE,
  :RULEACTION,
  :RULEACTIONVALUE, :CREATEDBY)
RETURNING FieldRULESYSID INTO :FieldRULESYSID", rule, "FieldRULESYSID");
        }

        public override async Task<int> UpdateAsync(FieldRule rule)
        {
            return await _dataAccess.SaveDataAsync<FieldRule>(@"
UPDATE FieldRULES 
SET   RULEFIELD=:RULEFIELD,
  RULEOPERATOR=:RULEOPERATOR,
  RULEVALUE=:RULEVALUE,
  RULEACTION=:RULEACTION,
  RULEACTIONVALUE=:RULEACTIONVALUE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE FieldRULESYSID = :FieldRULESYSID", rule);
        }

        public async override Task<int> DeleteAsync(string Fieldsysid)
        {
            return await _dataAccess.SaveDataAsync<FieldRule>("DELETE FROM FieldRULES WHERE FieldRULESYSID = :FieldRULESYSID", new FieldRule { FieldRuleSysId = Fieldsysid });
        }

        public async override Task<FieldRule> GetAsync(string Fieldrulesysid)
        {
            return await _dataAccess.FindDataAsync<FieldRule>("SELECT * FROM FieldRULES  WHERE FieldRULESYSID = :FieldRULESYSID ",
                new FieldRule { FieldRuleSysId = Fieldrulesysid });
        }
        public async override Task<IEnumerable<FieldRule>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<FieldRule>(@"SELECT * FROM FieldRULES ")
              .ContinueWith(t => (IEnumerable<FieldRule>)t.Result);
        }
        public async Task<IEnumerable<FieldRule>> GetListAsync(string fieldsysid)
        {
            return await _dataAccess.LoadDataAsync<FieldRule>(@"SELECT * FROM FieldRULES WHERE FieldSysId = :FieldSysId", new FieldRule { FieldSysId = fieldsysid })
              .ContinueWith(t => (IEnumerable<FieldRule>)t.Result);
        }

    }
}
