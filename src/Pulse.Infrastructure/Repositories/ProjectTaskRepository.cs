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
    public class ProjectTaskRepository : BaseRepository<ProjectTask, string>, IProjectTaskRepository
    {
        public ProjectTaskRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectTask task)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectTask>(@"  INSERT INTO PROJECTTASKS
      (
        PROJECTNO,
        ROADMAPACTIVITYSYSID,
        PARENTTYPE,
        PARENTSYSID,
        PLANTROADMAPLINKSYSID,
        ROADMAPSYSID,
        ALTTASKNAME,
        ALTTASKDESCRIPTION,
        ESTIMATEDMANDAYS,
        TARGETSTARTYEAR,
        TARGETSTARTWORKWEEK,
        TARGETSTARTEDBY,
        TARGETCOMPLETIONYEAR,
        TARGETCOMPLETIONWORKWEEK,
        TARGETCOMPLETEDBY,
        ACTUALSTARTDATE,
        ACTUALSTARTEDBY,
        ACTUALCOMPLETIONDATE,
        ACTUALCOMPLETEDBY,
        STATUS,
        REMARKS,
        ISREQUIRED,
        ORDERINDEX,
        ISACTIVE,
        CREATEDBY 
      )
    VALUES
      (
        :PROJECTNO,
        :ROADMAPACTIVITYSYSID,
        :PARENTTYPE,
        :PARENTSYSID,
        :PLANTROADMAPLINKSYSID,
        :ROADMAPSYSID,
        :ALTTASKNAME,
        :ALTTASKDESCRIPTION,
        :ESTIMATEDMANDAYS,
        :TARGETSTARTYEAR,
        :TARGETSTARTWORKWEEK,
        :TARGETSTARTEDBY,
        :TARGETCOMPLETIONYEAR,
        :TARGETCOMPLETIONWORKWEEK,
        :TARGETCOMPLETEDBY,
        :ACTUALSTARTDATE,
        :ACTUALSTARTEDBY,
        :ACTUALCOMPLETIONDATE,
        :ACTUALCOMPLETEDBY,
        :STATUS,
        :REMARKS,
        :ISREQUIRED,
        :ORDERINDEX,
        :ISACTIVE,
        :CREATEDBY 
      )
RETURNING PROJECTTASKSYSID INTO :PROJECTTASKSYSID
", task, "PROJECTTASKSYSID");
        }

        public override async Task<int> UpdateAsync(ProjectTask task)
        {
            return await _dataAccess.SaveDataAsync<ProjectTask>(@"UPDATE PROJECTTASKS 
                        SET 
        ALTTASKNAME=:ALTTASKNAME,
        ALTTASKDESCRIPTION=:ALTTASKDESCRIPTION,
        ESTIMATEDMANDAYS=:ESTIMATEDMANDAYS,
        TARGETSTARTYEAR=:TARGETSTARTYEAR,
        TARGETSTARTWORKWEEK=:TARGETSTARTWORKWEEK,
        TARGETSTARTEDBY=:TARGETSTARTEDBY,
        TARGETCOMPLETIONYEAR=:TARGETCOMPLETIONYEAR,
        TARGETCOMPLETIONWORKWEEK=:TARGETCOMPLETIONWORKWEEK,
        TARGETCOMPLETEDBY=:TARGETCOMPLETEDBY,
        ACTUALSTARTDATE=:ACTUALSTARTDATE,
        ACTUALSTARTEDBY=:ACTUALSTARTEDBY,
        ACTUALCOMPLETIONDATE=:ACTUALCOMPLETIONDATE,
        ACTUALCOMPLETEDBY=:ACTUALCOMPLETEDBY,
        STATUS=:STATUS,
        REMARKS=:REMARKS,
        ISREQUIRED=:ISREQUIRED,
        ORDERINDEX=:ORDERINDEX,
        ISACTIVE=:ISACTIVE 
                            ,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PROJECTTASKSYSID = :PROJECTTASKSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", task);
        }

        public async Task<int> UpdateTargetDateAsync(ProjectTask task)
        {
            return await _dataAccess.SaveDataAsync<ProjectTask>(@"
UPDATE projecttasks
   SET targetstartdate = :targetstartdate,
       targetcompletiondate = :targetcompletiondate,
       isresched =
          (CASE
              WHEN ( (targetstartdate IS NULL OR targetcompletiondate IS NULL) AND (targetstartdate <> :targetstartdate OR targetcompletiondate <> :targetcompletiondate)) THEN '1'
              ELSE isresched
           END),
       modifiedby = :modifiedby,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID ()
 WHERE projecttasksysid = :projecttasksysid
", task);
        }


        public override async Task<int> DeleteAsync(string taskid)
        {
            return await _dataAccess.SaveDataAsync<ProjectTask>("DELETE FROM PROJECTTASKS WHERE PROJECTTASKSYSID = :PROJECTTASKSYSID", new ProjectTask { ProjectTaskSysId = taskid });
        }

        public async Task<IEnumerable<ProjectTask>> GetListAsync(string projectno, string parenttype = null, string parentsysid = null)
        {
            return await _dataAccess.LoadDataAsync<ProjectTask>("SELECT * FROM PROJECTTASKS WHERE (:PROJECTNO is null OR PROJECTNO = :PROJECTNO) AND (:PARENTTYPE IS NULL OR (PARENTTYPE = :PARENTTYPE AND PARENTSYSID = :PARENTSYSID))",
    new ProjectTask { ProjectNo = projectno, ParentType = parenttype, ParentSysId = parentsysid });
        }

        public override Task<IEnumerable<ProjectTask>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public async override Task<ProjectTask> GetAsync(string id)
        {
            return await _dataAccess.FindDataAsync<ProjectTask>("SELECT * FROM PROJECTTASKS WHERE PROJECTTASKSYSID = :PROJECTTASKSYSID",
    new ProjectTask { ProjectTaskSysId = id });
        }

        public async Task<IEnumerable<ProjectTaskItem>> GetTaskItemListAsync(string projecttasksysid = null, string userid = null)
        {
            var sql = @" 
WITH lastfiscaldate AS (  SELECT MAX (fiscaldate) fiscaldate, calendaryear, calendarworkweek
                            FROM productioncalendars
                        GROUP BY calendaryear, calendarworkweek),
     validplants AS (SELECT plantcode, userid
                       FROM plantmembers
                      WHERE :userid IS NULL OR userid = :userid),
     validprojects AS (SELECT p.*,
                              pc.fiscaldate projectwkfiscaldate,
                              pl.plantname,
                              cat.categoryname,
                              pg.productgroupname,
                              pd.productdivisionname,
                              (SELECT listagg (pm.userid, ', ') WITHIN GROUP (ORDER BY pm.userid)
                                 FROM projectmembers pm
                                WHERE pm.projectno = p.projectno)
                                 prjmembers
                         FROM projects p
                              INNER JOIN validplants vp
                                 ON vp.plantcode = p.plantcode
                              INNER JOIN categories cat
                                 ON cat.categorycode = p.categorycode
                              INNER JOIN plants pl
                                 ON pl.plantcode = p.plantcode
                              INNER JOIN productgroups pg
                                 ON pg.productgroupcode = p.productgroupcode
                              INNER JOIN productdivisions pd
                                 ON pd.productdivisioncode = p.productdivisioncode
                              LEFT OUTER JOIN lastfiscaldate pc
                                 ON pc.calendaryear = p.targetcompletionyear AND pc.calendarworkweek = p.targetcompletionworkweek),
     validtasks AS (SELECT pt.*,
                           vp.projectname,
                           vp.projectdescription,
                           vp.projecticon,
                           vp.projecticoncolor,
                           vp.plantcode,
                           vp.plantname,
                           vp.categoryname,
                           vp.productgroupname,
                           vp.productdivisionname,
                           vp.targetcompletionyear projecttargetcompletionyear,
                           vp.targetcompletionworkweek projectcompletionworkweek,
                           vp.projectwkfiscaldate,
                           vp.prjmembers,
                           pc.fiscaldate taskwkfiscaldate,
                              (SELECT listagg (po.userid, ', ') WITHIN GROUP (ORDER BY po.userid)
                                 FROM projectowners po
                                WHERE po.projectno = pt.projectno
                                AND po.parenttype = 'TASK' and po.parentsysid = pt.projecttasksysid )
                                 taskmembers
                      FROM projecttasks pt
                           INNER JOIN validprojects vp
                              ON vp.projectno = pt.projectno
                           LEFT OUTER JOIN lastfiscaldate pc
                              ON pc.calendaryear = pt.targetcompletionyear AND pc.calendarworkweek = pt.targetcompletionworkweek
                     WHERE :projecttasksysid IS NULL OR projecttasksysid = :projecttasksysid)
SELECT DISTINCT vt.projecttasksysid,
                vt.roadmapactivitysysid,
                NVL (vt.alttaskname, NVL (pra.activityname, ra.activityname)) activityname,
                NVL (vt.alttaskdescription, NVL (pra.activitydescription, ra.activitydescription)) activitydescription,
                vt.projectno,
                vt.projectname,
                vt.projectdescription,
                vt.projecticon,
                vt.projecticoncolor,
                vt.plantcode,
                vt.plantname,
                vt.categoryname,
                vt.productgroupname,
                vt.productdivisionname,
                nvl(vt.taskmembers, vt.prjmembers) members,
                vt.estimatedmandays,
                vt.targetstartyear,
                vt.targetstartworkweek,
                vt.targetstartdate,
                vt.targetcompletionyear,
                vt.targetcompletionworkweek,
                vt.targetcompletiondate,
                vt.projecttargetcompletionyear,
                vt.projectcompletionworkweek,
                vt.projectwkfiscaldate,
                vt.taskwkfiscaldate,
                vt.actualstartdate,
                vt.actualcompletiondate,
                vt.status,
                vt.isrequired,
                vt.transactionkey
    FROM validtasks vt
         LEFT JOIN projectroadmapactivities pra
            ON     pra.projectno = vt.projectno
               AND pra.roadmapactivitysysid = vt.roadmapactivitysysid
         LEFT JOIN roadmapactivities ra
            ON ra.roadmapactivitysysid = vt.roadmapactivitysysid
";

            return await _dataAccess.LoadDataAsync<ProjectTaskItem>(sql, new ProjectTaskItem { UserId = userid, ProjectTaskSysId = projecttasksysid });
        }

    }
}
