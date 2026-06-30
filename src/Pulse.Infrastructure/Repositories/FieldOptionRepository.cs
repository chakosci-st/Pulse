
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
    public class FieldOptionRepository : BaseRepository<FieldOption, string>, IFieldOptionRepository
    {

        public FieldOptionRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(FieldOption option)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<FieldOption>(@"
INSERT INTO FieldOPTIONS (
   FieldSYSID, OPTIONVALUE, 
   OPTIONLABEL, ORDERINDEX, CREATEDBY) 
VALUES (:FieldSYSID, :OPTIONVALUE, 
   :OPTIONLABEL, :ORDERINDEX, :CREATEDBY)
RETURNING FieldOPTIONSYSID INTO :FieldOPTIONSYSID", option, "FieldOPTIONSYSID");
        }

        public override async Task<int> UpdateAsync(FieldOption option)
        {
            return await _dataAccess.SaveDataAsync<FieldOption>(@"
UPDATE FieldOPTIONS 
SET ORDERINDEX=:ORDERINDEX, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE OPTIONVALUE=:OPTIONVALUE AND FieldSYSID = :FieldSYSID ", option);
        }

        public async override Task<int> DeleteAsync(string Fieldoptionsysid)
        {
            return await _dataAccess.SaveDataAsync<FieldOption>("DELETE FROM FieldOPTIONS WHERE FieldOPTIONSYSID = :FieldOPTIONSYSID", new FieldOption { FieldOptionSysId = Fieldoptionsysid });
        }

        public async override Task<FieldOption> GetAsync(string Fieldoptionsysid)
        {
            return await _dataAccess.FindDataAsync<FieldOption>("SELECT * FROM FieldOPTIONS  WHERE FieldOPTIONSYSID = :FieldOPTIONSYSID ",
                new FieldOption { FieldOptionSysId = Fieldoptionsysid });
        }
        public async override Task<IEnumerable<FieldOption>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<FieldOption>(@"SELECT * FROM FieldOPTIONS ")
              .ContinueWith(t => (IEnumerable<FieldOption>)t.Result);
        }
        public async Task<IEnumerable<FieldOption>> GetListAsync(string fieldsysid)
        {
            return await _dataAccess.LoadDataAsync<FieldOption>(@"SELECT * FROM FieldOPTIONS WHERE FieldSysId = :FieldSysId", new FieldOption { FieldSysId = fieldsysid })
              .ContinueWith(t => (IEnumerable<FieldOption>)t.Result);
        }

    }
}
