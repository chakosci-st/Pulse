
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
    public class WorkItemPrerequisiteRepository : BaseRepository<WorkItemPrerequisite, string>, IWorkItemPrerequisiteRepository
    {

        public WorkItemPrerequisiteRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(WorkItemPrerequisite prerequisite)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<WorkItemPrerequisite>("INSERT INTO WORKITEMPREREQUISITES (WORKITEMSYSID, PREREQUISITEWORKITEMSYSID, CREATEDBY) VALUES (:WORKITEMSYSID, :PREREQUISITEWORKITEMSYSID, :CREATEDBY) RETURNING WORKITEMSYSID INTO :WORKITEMSYSID", prerequisite, "WORKITEMSYSID");
        }

        public override async Task<int> UpdateAsync(WorkItemPrerequisite prerequisite)
        {
            return await _dataAccess.SaveDataAsync<WorkItemPrerequisite>("UPDATE WORKITEMPREREQUISITES SET MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE WORKITEMPREREQUISITESYSID = :WORKITEMPREREQUISITESYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", prerequisite);
        }

        public override async Task<int> DeleteAsync(string workitemprerequisitesysid)
        {
            return await _dataAccess.SaveDataAsync<WorkItemPrerequisite>("DELETE FROM WORKITEMPREREQUISITES WHERE WORKITEMPREREQUISITESYSID = :WORKITEMPREREQUISITESYSID", new WorkItemPrerequisite { WorkItemPrerequisiteSysId = workitemprerequisitesysid });
        }





        public override async Task<IEnumerable<WorkItemPrerequisite>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<WorkItemPrerequisite>("SELECT * FROM WORKITEMPREREQUISITES")
            .ContinueWith(t => (IEnumerable<WorkItemPrerequisite>)t.Result);
        }
        public async Task<IEnumerable<WorkItemPrerequisite>> GetListAsync(string workitemsysid = null, string prerequisiteworkitemsysid = null)
        {
            return await _dataAccess.LoadDataAsync<WorkItemPrerequisite>(@"
                    SELECT *
                    FROM WORKITEMPREREQUISITES 
                    WHERE (:WORKITEMSYSID IS NULL OR WORKITEMSYSID = :WORKITEMSYSID) AND  (:PREREQUISITEWORKITEMSYSID IS NULL OR WORKITEMSYSID = :PREREQUISITEWORKITEMSYSID)",
                new WorkItemPrerequisite { WorkItemSysId = workitemsysid, PrerequisiteWorkItemSysId = prerequisiteworkitemsysid })
                  .ContinueWith(t => (IEnumerable<WorkItemPrerequisite>)t.Result);
        }

        public override async Task<WorkItemPrerequisite> GetAsync(string workitemprerequisitesysid)
        {
            return await _dataAccess.FindDataAsync<WorkItemPrerequisite>(@"
                    SELECT *
                    FROM WORKITEMPREREQUISITES
                    WHERE WORKITEMPREREQUISITESYSID = :WORKITEMPREREQUISITESYSID
                ", new WorkItemPrerequisite { WorkItemPrerequisiteSysId = workitemprerequisitesysid });
        }
    }
}
