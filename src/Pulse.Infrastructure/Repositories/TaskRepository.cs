using Entity = Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.Collections.Concurrent; 
using log4net;

namespace Pulse.Infrastructure.Repositories
{
    public class TaskRepository : BaseRepository<Entity.Task, string>, ITaskRepository
    {

        public TaskRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Entity.Task task)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<Entity.Task>(@"  INSERT INTO TASKS
      (
       PROJECTNO
      ,MILESTONESYSID
      ,WORKITEMSYSID
      ,TASKNAME
      ,TARGETSTART
      ,TARGETCOMPLETION
      ,ACTUALSTARTDATE
      ,ACTUALCOMPLETIONDATE
      ,STATUS
      ,TASKVALUE 
      ,TASKTYPE
      ,REMARKS
      ,ISREQUIRED
      ,CREATEDBY  
      )
    VALUES
      (
       :PROJECTNO
      ,:MILESTONESYSID
      ,:WORKITEMSYSID
      ,:TASKNAME
      ,:TARGETSTART
      ,:TARGETCOMPLETION
      ,:ACTUALSTARTDATE
      ,:ACTUALCOMPLETIONDATE
      ,:STATUS
      ,:TASKVALUE 
      ,:TASKTYPE
      ,:REMARKS
      ,:ISREQUIRED
      ,:CREATEDBY  
      )
RETURNING TASKSYSID INTO :TASKSYSID
", task, "TASKSYSID");
        }

        public override async Task<int> UpdateAsync(Entity.Task task)
        {
            return await _dataAccess.SaveDataAsync<Entity.Task>(@"UPDATE TASKS 
                        SET PROJECTNO = :PROJECTNO
                          ,MILESTONESYSID = :MILESTONESYSID
                          ,WORKITEMSYSID=:WORKITEMSYSID
                          ,TASKNAME = :TASKNAME
                          ,TARGETSTART = :TARGETSTART
                          ,TARGETCOMPLETION = :TARGETCOMPLETION
                          ,ACTUALSTARTDATE = :ACTUALSTARTDATE
                          ,ACTUALCOMPLETIONDATE = :ACTUALCOMPLETIONDATE
                          ,STATUS = :STATUS
                          ,TASKVALUE  = :TASKVALUE
                          ,TASKTYPE = :TASKTYPE
                          ,REMARKS = :REMARKS
                          ,ISREQUIRED = :ISREQUIRED
                            ,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = NVL(:MODIFIEDDATE,SYSTIMESTAMP), TRANSACTIONKEY = SYS_GUID() 
                        WHERE TASKSYSID = :TASKSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", task);
        }

        public override async Task<int> DeleteAsync(string taskid)
        {
            return await _dataAccess.SaveDataAsync<Entity.Task>("DELETE FROM TASKS WHERE TASKSYSID = :TASKSYSID", new Entity.Task { TaskSysId = taskid });
        }



        public override async Task<Entity.Task> GetAsync(string taskid)
        {
            return await _dataAccess.FindDataAsync<Entity.Task>("SELECT * FROM TASKS WHERE TASKSYSID = :TASKSYSID",
                new Entity.Task { TaskSysId = taskid });
        }

        public override async Task<IEnumerable<Entity.Task>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Entity.Task>("SELECT * FROM TASKS")
                  .ContinueWith(t => (IEnumerable<Entity.Task>)t.Result);
        }
        public async Task<IEnumerable<Entity.TaskSearchQuery>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null, string maturitycode = null, string userid = null, int? usergroupid = null, string adgroup = null, DateTime? displayrangefrom = null, DateTime? displayrangeto = null, string status = null)
        {
            return await _dataAccess.LoadDataAsync<Entity.TaskSearchQuery>(@"
SELECT *
  FROM TASKS
 WHERE (:USERID IS NULL
        OR (EXISTS
               (SELECT tasksysid
                  FROM TASKMEMBERS tm
                 WHERE     USERID = :USERID
                       AND tm.tasksysid = tasksysid
                       AND tm.isactive = 1
                UNION
                SELECT tasksysid
                  FROM    TASKMEMBERS tm
                       INNER JOIN
                          USERGROUPMEMBERS ugm
                       ON UGM.USERGROUPID = TM.USERGROUPID
                 WHERE     ugm.USERID = :USERID
                       AND tm.tasksysid = tasksysid
                       AND tm.isactive = 1
                UNION
                SELECT tasksysid
                  FROM    TASKMEMBERS tm
                       INNER JOIN
                          ACTIVEDIRECTORYGROUPMEMBERS adgm
                       ON adgm.ADGROUP = TM.ADGROUP
                 WHERE     adgm.USERID = :USERID
                       AND tm.tasksysid = tasksysid
                       AND tm.isactive = 1)))
       AND (:USERGROUPID IS NULL
            OR (EXISTS
                   (SELECT tasksysid
                      FROM TASKMEMBERS tm
                     WHERE     USERGROUPID = :USERGROUPID
                           AND tm.tasksysid = tasksysid
                           AND tm.isactive = 1)))
       AND (:ADGROUP IS NULL
            OR (EXISTS
                   (SELECT tasksysid
                      FROM TASKMEMBERS tm
                     WHERE     ADGROUP = :ADGROUP
                           AND tm.tasksysid = tasksysid
                           AND tm.isactive = 1)))
       AND (:COMPLETIONDATERANGEFROM IS NULL
            OR EXISTS
                  (SELECT *
                     FROM PRODUCTIONCALENDARS
                    WHERE TRUNC (fiscaldate) BETWEEN TRUNC (
                                                        :DISPLAYRANGEFROM)
                                                 AND TRUNC (:DISPLAYRANGETO)
                          AND (calendarworkweek = targetstart
                               OR calendarworkweek = targetcompletion
                               OR TRUNC (actualstartdate) BETWEEN TRUNC (
                                                                     :DISPLAYRANGEFROM)
                                                              AND TRUNC (
                                                                     :DISPLAYRANGETO)
                               OR TRUNC (actualcompletiondate) BETWEEN TRUNC (
                                                                          :DISPLAYRANGEFROM)
                                                                   AND TRUNC (
                                                                          :DISPLAYRANGETO))))
       AND (:STATUS IS NULL OR status = :STATUS)
       AND (:PROJECTNO IS NULL OR PROJECTNO = :PROJECTNO)
       AND (:MILESTONESYSID IS NULL OR MILESTONESYSID = :MILESTONESYSID)
       AND (:TASKSYSID IS NULL OR TASKSYSID = :TASKSYSID)
       AND (:MATURITYCODE IS NULL
            OR (EXISTS
                   (SELECT pm.tasksysid
                      FROM PROJECTMILESTONE pm
                     WHERE     pm.MATURITYCODE = :MATURITYCODE
                           AND pm.MILESTONESYSID = MILESTONESYSID)))
",
          new Entity.TaskSearchQuery { ProjectNo = projectno, MilestoneSysId = milestonesysid, TaskSysId = tasksysid, MaturityCode = maturitycode, UserId = userid, UserGroupId = usergroupid, ADGroup = adgroup, DisplayRangeFrom = displayrangefrom, DisplayRangeTo = displayrangeto, Status = status });



        }


    }
}
