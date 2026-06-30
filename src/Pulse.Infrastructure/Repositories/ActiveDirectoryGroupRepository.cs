using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;  
using log4net;

namespace Pulse.Infrastructure.Repositories
{
    public class ActiveDirectoryGroupRepository : BaseRepository<ActiveDirectoryGroup, string>, IActiveDirectoryGroupRepository
    {

        public ActiveDirectoryGroupRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ActiveDirectoryGroup activedirectorygroup)
        {
            var returnvalue= await _dataAccess.SaveDataAsync<ActiveDirectoryGroup>(@"INSERT INTO ACTIVEDIRECTORYGROUPS (ADGROUP, EMAIL, ISACTIVE, CREATEDBY) 
                        VALUES (:ADGROUP, :EMAIL, :ISACTIVE, :CREATEDBY) ", activedirectorygroup );
            return activedirectorygroup.ADGroup;
        }

        public override async Task<int> UpdateAsync(ActiveDirectoryGroup activedirectorygroup)
        {
            return await _dataAccess.SaveDataAsync<ActiveDirectoryGroup>(@"UPDATE ACTIVEDIRECTORYGROUPS SET EMAIL = :EMAIL, 
                        ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE ADGROUP = :ADGROUP AND TRANSACTIONKEY = :TRANSACTIONKEY", activedirectorygroup);
        }

        public override async Task<int> DeleteAsync(string adgroup)
        {

            return await _dataAccess.SaveDataAsync<ActiveDirectoryGroup>("DELETE FROM ACTIVEDIRECTORYGROUPS WHERE ADGROUP = :ADGROUP", new ActiveDirectoryGroup { ADGroup = adgroup });
        }

        public override async Task<IEnumerable<ActiveDirectoryGroup>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ActiveDirectoryGroup>("SELECT * FROM ACTIVEDIRECTORYGROUPS")
                .ContinueWith(t => (IEnumerable<ActiveDirectoryGroup>)t.Result);
        }

        public override async Task<ActiveDirectoryGroup> GetAsync(string adgroup)
        {
            return await _dataAccess.FindDataAsync<ActiveDirectoryGroup>("SELECT * FROM ACTIVEDIRECTORYGROUPS WHERE ADGROUP = :ADGROUP", new ActiveDirectoryGroup { ADGroup = adgroup });
        }


    }
}
