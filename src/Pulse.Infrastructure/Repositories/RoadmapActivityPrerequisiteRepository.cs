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
    public class RoadmapActivityPrerequisiteRepository : BaseRepository<RoadmapActivityPrerequisite, string>, IRoadmapActivityPrerequisiteRepository
    {
        public RoadmapActivityPrerequisiteRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(RoadmapActivityPrerequisite link)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<RoadmapActivityPrerequisite>(@"INSERT INTO ROADMAPACTIVITYPREREQUISITES (ROADMAPACTIVITYSYSID, PREREQUISITESYSID) VALUES (:ROADMAPACTIVITYSYSID, :PREREQUISITESYSID) RETURNING ROADMAPACTIVITYPREREQSYSID INTO :ROADMAPACTIVITYPREREQSYSID",
                link, "ROADMAPACTIVITYPREREQSYSID");

        }



        public async Task<IEnumerable<RoadmapActivityPrerequisite>> GetListAsync(string roadmapsysid, string roadmapactivitysysid = null)
        {
            return await _dataAccess.LoadDataAsync<RoadmapActivityPrerequisite>(@"
SELECT *
  FROM ROADMAPACTIVITYPREREQUISITES p
 WHERE (EXISTS
           (SELECT *
              FROM ROADMAPACTIVITIES a
             WHERE a.roadmapactivitysysid = p.roadmapactivitysysid
                   AND roadmapsysid = :roadmapsysid)
        OR EXISTS
              (SELECT *
                 FROM ROADMAPACTIVITIES a
                WHERE a.roadmapactivitysysid = p.prerequisitesysid
                      AND roadmapsysid = :roadmapsysid))
       AND (:roadmapactivitysysid IS NULL
            OR roadmapactivitysysid = :roadmapactivitysysid)
", new RoadmapActivityPrerequisiteExt { RoadMapSysId = roadmapsysid, RoadMapActivitySysId = roadmapactivitysysid });
        }


        public override async Task<RoadmapActivityPrerequisite> GetAsync(string roadmapactivityprereqsysid)
        {
            return await _dataAccess.FindDataAsync<RoadmapActivityPrerequisite>("SELECT * FROM ROADMAPACTIVITYPREREQUISITES WHERE ROADMAPACTIVITYPREREQSYSID = :ROADMAPACTIVITYPREREQSYSID",
            new RoadmapActivityPrerequisite { RoadmapActivityPrereqSysId = roadmapactivityprereqsysid });
        }

        public override async Task<int> DeleteAsync(string roadmapactivityprereqsysid)
        {
            return await _dataAccess.SaveDataAsync<RoadmapActivityPrerequisite>("DELETE FROM ROADMAPACTIVITYPREREQUISITES WHERE ROADMAPACTIVITYPREREQSYSID = :ROADMAPACTIVITYPREREQSYSID",
            new RoadmapActivityPrerequisite { RoadmapActivityPrereqSysId = roadmapactivityprereqsysid });
        }

        //[Obsolete("Use GetListAsync(string roadmapsysid = null, string formsysid = null) instead.", true)]
        public override Task<IEnumerable<RoadmapActivityPrerequisite>> GetListAsync() => throw new NotImplementedException();
        public override Task<int> UpdateAsync(RoadmapActivityPrerequisite link) => throw new NotImplementedException();
    }
}
