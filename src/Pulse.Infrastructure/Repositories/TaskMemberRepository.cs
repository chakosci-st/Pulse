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
    public class TaskMemberRepository : BaseRepository<TaskMember, string>, ITaskMemberRepository
    {

        public TaskMemberRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(TaskMember taskmember)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<TaskMember>(@"INSERT INTO TASKMEMBERS (PROJECTNO, TASKSYSID, USERID, USERGROUPID, ADGROUP,  CREATEDBY) 
VALUES (:PROJECTNO, :TASKSYSID, :USERID, :USERGROUPID, :ADGROUP, :CREATEDBY)) 
RETURNING TASKMEMBERSYSID = :TASKMEMBERSYSID", taskmember, "TASKMEMBERSYSID");
        }

        public override async Task<int> UpdateAsync(TaskMember taskmember)
        {
            return await _dataAccess.SaveDataAsync<TaskMember>("UPDATE TASKMEMBERS SET PROJECTNO = :PROJECTNO, TASKSYSID = :TASKSYSID, USERID = :USERID, USERGROUPID = :USERGROUPID, ADGROUP = :ADGROUP, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE TASKMEMBERSYSID = :TASKMEMBERSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", taskmember);
        }

        public override async Task<int> DeleteAsync(string taskmembersysid)
        {
            return await _dataAccess.SaveDataAsync<TaskMember>("DELETE FROM TASKMEMBERS WHERE TASKMEMBERSYSID = :TASKMEMBERSYSID", new TaskMember { TaskMemberSysId = taskmembersysid });
        }
        public async override Task<IEnumerable<TaskMember>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<TaskMember>("SELECT * FROM TASKMEMBERS")
       .ContinueWith(t => (IEnumerable<TaskMember>)t.Result);
        }
 

        public override async Task<TaskMember> GetAsync(string taskmembersysid)
        {
            return await _dataAccess.FindDataAsync<TaskMember>("SELECT * FROM TASKMEMBERS WHERE TASKMEMBERSYSID = :TASKMEMBERSYSID", new TaskMember { TaskMemberSysId = taskmembersysid });
        }

        public async Task<IEnumerable<TaskMember>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<TaskMember>("SELECT * FROM TASKMEMBERS WHERE PROJECTNO = :PROJECTNO", new TaskMember { ProjectNo = projectno })
  .ContinueWith(t => (IEnumerable<TaskMember>)t.Result);
        }

        public async Task<IEnumerable<TaskMember>> GetByTaskAsync(string tasksysid)
        {
            return await _dataAccess.LoadDataAsync<TaskMember>("SELECT * FROM TASKMEMBERS WHERE TASKSYSID = :TASKSYSID", new TaskMember { TaskSysId = tasksysid })
.ContinueWith(t => (IEnumerable<TaskMember>)t.Result);
        }

 
    }
}
