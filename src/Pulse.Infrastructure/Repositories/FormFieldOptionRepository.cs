
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
    public class FormFieldOptionRepository : BaseRepository<FormFieldOption, string>, IFormFieldOptionRepository
    {

        public FormFieldOptionRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(FormFieldOption option)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<FormFieldOption>(@"
INSERT INTO FORMFIELDOPTIONS (
   FORMFIELDSYSID, OPTIONVALUE, 
   OPTIONLABEL, ORDERINDEX, CREATEDBY) 
VALUES (:FORMFIELDSYSID, :OPTIONVALUE, 
   :OPTIONLABEL, :ORDERINDEX, :CREATEDBY)
RETURNING FORMFIELDOPTIONSYSID INTO :FORMFIELDOPTIONSYSID", option, "FORMFIELDOPTIONSYSID");
        }

        public override async Task<int> UpdateAsync(FormFieldOption option)
        {
            return await _dataAccess.SaveDataAsync<FormFieldOption>(@"
UPDATE FORMFIELDOPTIONS 
SET ORDERINDEX=:ORDERINDEX, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
WHERE OPTIONVALUE=:OPTIONVALUE AND FORMFIELDSYSID = :FORMFIELDSYSID ", option);
        }

        public async override Task<int> DeleteAsync(string formfieldoptionsysid)
        {
            return await _dataAccess.SaveDataAsync<FormFieldOption>("DELETE FROM FORMFIELDOPTIONS WHERE FORMFIELDOPTIONSYSID = :FORMFIELDOPTIONSYSID", new FormFieldOption { FormFieldOptionSysId = formfieldoptionsysid });
        }

        public async override Task<FormFieldOption> GetAsync(string formfieldoptionsysid)
        {
            return await _dataAccess.FindDataAsync<FormFieldOption>("SELECT * FROM FORMFIELDOPTIONS  WHERE FORMFIELDOPTIONSYSID = :FORMFIELDOPTIONSYSID ",
                new FormFieldOption { FormFieldOptionSysId = formfieldoptionsysid });
        }
        public async override Task<IEnumerable<FormFieldOption>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<FormFieldOption>(@"SELECT * FROM FORMFIELDOPTIONS ")
              .ContinueWith(t => (IEnumerable<FormFieldOption>)t.Result);
        }
 

    }
}
