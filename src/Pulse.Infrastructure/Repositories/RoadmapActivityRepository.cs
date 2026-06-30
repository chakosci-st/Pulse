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
    public class RoadmapActivityRepository : BaseRepository<RoadmapActivity, string>, IRoadmapActivityRepository
    {
        public RoadmapActivityRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(RoadmapActivity activity)
        {
            var rowsaffected = await _dataAccess.SaveDataAsync<RoadmapActivity>(@"INSERT INTO ROADMAPACTIVITIES (ROADMAPACTIVITYSYSID, ROADMAPSYSID, PARENTTYPE, PARENTSYSID, ACTIVITYNAME, ACTIVITYDESCRIPTION, ESTIMATEDMANDAYS, ISREQUIRED, ORDERINDEX, CREATEDBY) VALUES (:ROADMAPACTIVITYSYSID, :ROADMAPSYSID, :PARENTTYPE, :PARENTSYSID, :ACTIVITYNAME, :ACTIVITYDESCRIPTION, :ESTIMATEDMANDAYS, :ISREQUIRED,  :ORDERINDEX, :CREATEDBY)",
                activity);

            if (rowsaffected > 0)
                return activity.RoadmapActivitySysId;
            else
                throw new Exception("Insert failed");
        }

        public override async Task<int> UpdateAsync(RoadmapActivity activity)
        {
            return await _dataAccess.SaveDataAsync<RoadmapActivity>(@"UPDATE ROADMAPACTIVITIES SET ACTIVITYNAME=:ACTIVITYNAME, ACTIVITYDESCRIPTION=:ACTIVITYDESCRIPTION, ESTIMATEDMANDAYS=:ESTIMATEDMANDAYS, ISREQUIRED=:ISREQUIRED, ORDERINDEX=:ORDERINDEX,  ISACTIVE=:ISACTIVE, MODIFIEDBY=:MODIFIEDBY, MODIFIEDDATE=:MODIFIEDDATE, TRANSACTIONKEY = SYS_GUID()  WHERE ROADMAPACTIVITYSYSID = :ROADMAPACTIVITYSYSID", activity);
        }

        public async Task<IEnumerable<RoadmapActivity>> GetListAsync(string roadmapsysid, string parenttype = null, string parentsysid = null)
        {
            return await _dataAccess.LoadDataAsync<RoadmapActivity>("SELECT * FROM ROADMAPACTIVITIES WHERE ROADMAPSYSID = :ROADMAPSYSID AND (:PARENTTYPE IS NULL OR PARENTTYPE =:PARENTTYPE)  AND (:PARENTSYSID IS NULL OR PARENTSYSID =:PARENTSYSID)", new RoadmapActivity { RoadmapSysId = roadmapsysid,   ParentSysId = parentsysid });
        }

 


        public override async Task<RoadmapActivity> GetAsync(string roadmapactivitylinksysid)
        {
            return await _dataAccess.FindDataAsync<RoadmapActivity>("SELECT * FROM ROADMAPACTIVITIES WHERE ROADMAPACTIVITYSYSID = :ROADMAPACTIVITYSYSID",
            new RoadmapActivity { RoadmapActivitySysId = roadmapactivitylinksysid });
        }

        public override async Task<int> DeleteAsync(string roadmapactivitylinksysid)
        {
            return await _dataAccess.SaveDataAsync<RoadmapActivity>("DELETE FROM ROADMAPACTIVITIES WHERE ROADMAPACTIVITYSYSID = :ROADMAPACTIVITYSYSID",
            new RoadmapActivity { RoadmapActivitySysId = roadmapactivitylinksysid });
        }

        public async Task<int> DeleteAsync(string roadmapactivitylinksysid, string roadmapsysid)
        {
            return await _dataAccess.SaveDataAsync<RoadmapActivity>("DELETE FROM ROADMAPACTIVITIES WHERE ROADMAPACTIVITYSYSID = :ROADMAPACTIVITYSYSID AND ROADMAPSYSID = :ROADMAPSYSID",
            new RoadmapActivity { RoadmapActivitySysId = roadmapactivitylinksysid, RoadmapSysId = roadmapsysid });
        }
         
        //[Obsolete("Use GetListAsync(string roadmapsysid = null, string formsysid = null) instead.", true)]
        public override Task<IEnumerable<RoadmapActivity>> GetListAsync() => throw new NotImplementedException();

        public async Task<IEnumerable<PrereqRow>> GetPrerequisites(string roadmapactivitysysid)
        {
            return await _dataAccess.LoadDataAsync<PrereqRow>(@"
SELECT
    roadmapactivityprereqsysid AS prereqlinkkey,
    roadmapactivitysysid       AS nodekey,
    prerequisitesysid          AS prereqkey
FROM roadmapactivityprerequisites
WHERE roadmapactivitysysid = :roadmapactivitysysid",
              new PrereqRow { RoadmapActivitySysId = roadmapactivitysysid });
        }
    }
}
