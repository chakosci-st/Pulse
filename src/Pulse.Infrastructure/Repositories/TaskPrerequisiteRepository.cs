using Entity = Pulse.Core.Entities;
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
    public class TaskPrerequisiteRepository : BaseRepository<Entity.TaskPrerequisite, string>, ITaskPrerequisiteRepository
    {
        public TaskPrerequisiteRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Entity.TaskPrerequisite taskprerequisite)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<Entity.TaskPrerequisite>(@"  INSERT INTO TASKPREREQUISITES
      (
       PROJECTNO
      ,TASKSYSID
      ,PREREQUISITESYSID 
      )
    VALUES
      (
       :PROJECTNO
      ,:TASKSYSID
      ,:PREREQUISITESYSID 
      )
RETURNING TASKPREREQUISITESYSID INTO :TASKPREREQUISITESYSID
", taskprerequisite, "TASKPREREQUISITESYSID");
        }

        public override async Task<int> UpdateAsync(Entity.TaskPrerequisite taskprerequisite)
        {
            return await _dataAccess.SaveDataAsync<Entity.TaskPrerequisite>(@"UPDATE TASKPREREQUISITES 
                        SET PROJECTNO = :PROJECTNO
                          ,TASKSYSID = :TASKSYSID
                          ,PREREQUISITESYSID=:PREREQUISITESYSID 
                        WHERE TASKPREREQUISITESYSID = :TASKPREREQUISITESYSID", taskprerequisite);
        }

        public override async Task<int> DeleteAsync(string taskid)
        {
            return await _dataAccess.SaveDataAsync<Entity.TaskPrerequisite>("DELETE FROM TASKPREREQUISITES WHERE TASKPREREQUISITESYSID = :TASKPREREQUISITESYSID", new Entity.TaskPrerequisite { TaskSysId = taskid });
        }

        public override async Task<IEnumerable<Entity.TaskPrerequisite>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Entity.TaskPrerequisite>("SELECT * FROM TASKPREREQUISITES")
                  .ContinueWith(t => (IEnumerable<Entity.TaskPrerequisite>)t.Result);
        }

        public override async Task<Entity.TaskPrerequisite> GetAsync(string taskprerequisiteid)
        {
            return await _dataAccess.FindDataAsync<Entity.TaskPrerequisite>("SELECT * FROM TASKPREREQUISITES WHERE TASKPREREQUISITESYSID = :TASKPREREQUISITESYSID",
                new Entity.TaskPrerequisite { PrerequisiteTaskSysId = taskprerequisiteid });
        }


        public async Task<IEnumerable<Entity.TaskPrerequisite>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<Entity.TaskPrerequisite>("SELECT * FROM TASKPREREQUISITES WHERE PROJECTNO = :PROJECTNO",
       new Entity.TaskPrerequisite { ProjectNo = projectno });
        }

    
    }
}
