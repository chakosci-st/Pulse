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
    public class WorkItemMemberRepository : BaseRepository<WorkItemMember, string>, IWorkItemMemberRepository
    {

        public WorkItemMemberRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(WorkItemMember workitemmember)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<WorkItemMember>(@"INSERT INTO WORKITEMMEMBERS (WORKITEMSTSID, USERID, USERGROUPID, ADGROUP, ISACTIVE, CREATEDBY) 
VALUES (:WORKITEMSTSID, :USERID, :USERGROUPID, :ADGROUP, :ISACTIVE, :CREATEDBY)) 
RETURNING WORKITEMMEMBERSYSID = :WORKITEMMEMBERSYSID", workitemmember, "WORKITEMMEMBERSYSID");
        }

        public override async Task<int> UpdateAsync(WorkItemMember workitemmember)
        {
            return await _dataAccess.SaveDataAsync<WorkItemMember>("UPDATE WORKITEMMEMBERS SET WORKITEMSTSID = :WORKITEMSTSID, USERID = :USERID, USERGROUPID = :USERGROUPID, ADGROUP = :ADGROUP, ISACTIVE = :MODIFIEDBY, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE WORKITEMMEMBERSYSID = :WORKITEMMEMBERSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", workitemmember);
        }

        public override async Task<int> DeleteAsync(string workitemmembersysid)
        {
            return await _dataAccess.SaveDataAsync<WorkItemMember>("DELETE FROM WORKITEMMEMBERS WHERE WORKITEMMEMBERSYSID = :WORKITEMMEMBERSYSID", new WorkItemMember { WorkItemMemberSysId = workitemmembersysid });
        }
        public async override Task<IEnumerable<WorkItemMember>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<WorkItemMember>("SELECT * FROM WORKITEMMEMBERS")
       .ContinueWith(t => (IEnumerable<WorkItemMember>)t.Result);
        }
        public async Task<IEnumerable<WorkItemMember>> GetListAsync(string workitemsysid)
        {
            return await _dataAccess.LoadDataAsync<WorkItemMember>("SELECT * FROM WORKITEMMEMBERS WHERE WORKITEMSTSID = :WORKITEMSTSID", new WorkItemMember { WorkItemMemberSysId = workitemsysid })
       .ContinueWith(t => (IEnumerable<WorkItemMember>)t.Result);
        }

        public override async Task<WorkItemMember> GetAsync(string workitemmembersysid)
        {
            return await _dataAccess.FindDataAsync<WorkItemMember>("SELECT * FROM WORKITEMMEMBERS WHERE WORKITEMMEMBERSYSID = :WORKITEMMEMBERSYSID", new WorkItemMember { WorkItemMemberSysId = workitemmembersysid });
        }



    }
}
