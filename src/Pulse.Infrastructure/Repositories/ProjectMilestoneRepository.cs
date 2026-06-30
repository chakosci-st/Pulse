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
    public class ProjectMilestoneRepository : BaseRepository<ProjectMilestone, string>, IProjectMilestoneRepository
    {
        public ProjectMilestoneRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectMilestone projectmilestone)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectMilestone>(@"  INSERT INTO PROJECTMILESTONES
      (
        PROJECTNO
        ,PLANTROADMAPLINKSYSID
        ,ROADMAPSYSID
        ,ROADMAPMILESTONESYSID
        ,TARGETSTARTYEAR
        ,TARGETSTARTWORKWEEK
        ,TARGETSTARTEDBY
        ,TARGETCOMPLETIONYEAR
        ,TARGETCOMPLETIONWORKWEEK
        ,TARGETCOMPLETEDBY
        ,ACTUALSTARTDATE
        ,ACTUALSTARTEDBY
        ,ACTUALCOMPLETIONDATE
        ,ACTUALCOMPLETEDBY
        ,STATUS
        ,REMARKS 
        ,ISREQUIRED
        ,CREATEDBY  
      )
    VALUES
      (
        :PROJECTNO
        ,:PLANTROADMAPLINKSYSID
        ,:ROADMAPSYSID
        ,:ROADMAPMILESTONESYSID
        ,:TARGETSTARTYEAR
        ,:TARGETSTARTWORKWEEK
        ,:TARGETSTARTEDBY
        ,:TARGETCOMPLETIONYEAR
        ,:TARGETCOMPLETIONWORKWEEK
        ,:TARGETCOMPLETEDBY
        ,:ACTUALSTARTDATE
        ,:ACTUALSTARTEDBY
        ,:ACTUALCOMPLETIONDATE
        ,:ACTUALCOMPLETEDBY
        ,:STATUS
        ,:REMARKS 
        ,:ISREQUIRED
        ,:CREATEDBY 
      )
RETURNING MILESTONESYSID INTO :MILESTONESYSID
", projectmilestone, "MILESTONESYSID");
        }

        public override async Task<int> UpdateAsync(ProjectMilestone projectmilestone)
        {
            return await _dataAccess.SaveDataAsync<ProjectMilestone>(@"UPDATE PROJECTMILESTONES 
                        SET   
        TARGETSTARTYEAR=:TARGETSTARTYEAR
        ,TARGETSTARTWORKWEEK=:TARGETSTARTWORKWEEK
        ,TARGETSTARTEDBY=:TARGETSTARTEDBY
        ,TARGETCOMPLETIONYEAR=:TARGETCOMPLETIONYEAR
        ,TARGETCOMPLETIONWORKWEEK=:TARGETCOMPLETIONWORKWEEK
        ,TARGETCOMPLETEDBY=:TARGETCOMPLETEDBY
        ,ACTUALSTARTDATE=:ACTUALSTARTDATE
        ,ACTUALSTARTEDBY=:ACTUALSTARTEDBY
        ,ACTUALCOMPLETIONDATE=:ACTUALCOMPLETIONDATE
        ,ACTUALCOMPLETEDBY=:ACTUALCOMPLETEDBY
        ,STATUS=:STATUS
        ,REMARKS=:REMARKS
        ,ISACTIVE=:ISACTIVE 
        ,ISREQUIRED=:ISREQUIRED
                            ,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE MILESTONESYSID = :MILESTONESYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", projectmilestone);
        }

        public async Task<int> UpdateTargetDateAsync(ProjectMilestone milestone)
        {
            return await _dataAccess.SaveDataAsync<ProjectMilestone>(@"
UPDATE PROJECTMILESTONES
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
 WHERE MILESTONESYSID = :MILESTONESYSID
", milestone);
        }

        public override async Task<int> DeleteAsync(string milestoneid)
        {
            return await _dataAccess.SaveDataAsync<ProjectMilestone>("DELETE FROM PROJECTMILESTONES WHERE MILESTONESYSID = :MILESTONESYSID", new ProjectMilestone { MilestoneSysId = milestoneid });
        }

        public override async Task<IEnumerable<ProjectMilestone>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ProjectMilestone>("SELECT * FROM PROJECTMILESTONES")
                  .ContinueWith(t => (IEnumerable<ProjectMilestone>)t.Result);
        }

        public override async Task<ProjectMilestone> GetAsync(string milestoneid)
        {
            return await _dataAccess.FindDataAsync<ProjectMilestone>("SELECT * FROM PROJECTMILESTONES WHERE MILESTONESYSID = :MILESTONESYSID",
                new ProjectMilestone { MilestoneSysId = milestoneid });
        }


        public async Task<IEnumerable<ProjectMilestone>> GetListAsync(string projectno)
        {
            return await _dataAccess.LoadDataAsync<ProjectMilestone>("SELECT * FROM PROJECTMILESTONES WHERE PROJECTNO = :PROJECTNO",
       new ProjectMilestone { ProjectNo = projectno });
        }

 
         
    }
}
