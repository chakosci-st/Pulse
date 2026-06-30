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
    public class RoadmapMilestoneRepository : BaseRepository<RoadmapMilestone, string>, IRoadmapMilestoneRepository
    {
        public RoadmapMilestoneRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(RoadmapMilestone link)
        {
            var rowsaffected =  await _dataAccess.SaveDataAsync<RoadmapMilestone>(@"INSERT INTO ROADMAPMILESTONES (ROADMAPMILESTONESYSID, ROADMAPSYSID, MATURITYCODE, PARENTTYPE, PARENTSYSID, MILESTONEALIAS, MILESTONEDESCRIPTION, ORDERINDEX, ISREQUIRED, CREATEDBY) VALUES (:ROADMAPMILESTONESYSID, :ROADMAPSYSID, :MATURITYCODE, :PARENTTYPE, :PARENTSYSID, :MILESTONEALIAS, :MILESTONEDESCRIPTION, :ORDERINDEX, :ISREQUIRED, :CREATEDBY)",
                link);
             
            if (rowsaffected > 0)
                return link.RoadmapMilestoneSysId;
            else
                throw new Exception("Insert failed");
        }

        public override async Task<int> UpdateAsync(RoadmapMilestone link)
        {
            return await _dataAccess.SaveDataAsync<RoadmapMilestone>(@"UPDATE ROADMAPMILESTONES SET MATURITYCODE=:MATURITYCODE, MILESTONEALIAS=:MILESTONEALIAS, MILESTONEDESCRIPTION=:MILESTONEDESCRIPTION, ORDERINDEX=:ORDERINDEX, ISACTIVE=:ISACTIVE, ISREQUIRED=:ISREQUIRED, MODIFIEDBY=:MODIFIEDBY, MODIFIEDDATE=:MODIFIEDDATE,TRANSACTIONKEY=SYS_GUID() WHERE ROADMAPMILESTONESYSID = :ROADMAPMILESTONESYSID", link);
        }

        public async Task<IEnumerable<RoadmapMilestone>> GetListAsync(string roadmapsysid, string parenttype = null, string parentsysid = null)
        {
            return await _dataAccess.LoadDataAsync<RoadmapMilestone>("SELECT * FROM ROADMAPMILESTONES WHERE ROADMAPSYSID = :ROADMAPSYSID AND ((:PARENTTYPE IS NULL OR PARENTTYPE = :PARENTTYPE) AND (:PARENTSYSID IS NULL OR PARENTSYSID = :PARENTSYSID))", new RoadmapMilestone { RoadmapSysId = roadmapsysid, ParentType = parenttype, ParentSysId = parentsysid });
        }


        public override async Task<RoadmapMilestone> GetAsync(string roadmapmilestonesysid)
        {
            return await _dataAccess.FindDataAsync<RoadmapMilestone>("SELECT * FROM ROADMAPMILESTONES WHERE ROADMAPMILESTONESYSID = :ROADMAPMILESTONESYSID",
            new RoadmapMilestone { RoadmapMilestoneSysId = roadmapmilestonesysid });
        }

        public override async Task<int> DeleteAsync(string roadmapmilestonesysid)
        {
            return await _dataAccess.SaveDataAsync<RoadmapMilestone>("DELETE FROM ROADMAPMILESTONES WHERE ROADMAPMILESTONESYSID = :ROADMAPMILESTONESYSID",
            new RoadmapMilestone { RoadmapMilestoneSysId = roadmapmilestonesysid });
        }

        public async Task<int> DeleteAsync(string roadmapmilestonesysid, string roadmapsysid)
        {
            return await _dataAccess.SaveDataAsync<RoadmapMilestone>("DELETE FROM ROADMAPMILESTONES WHERE ROADMAPMILESTONESYSID = :ROADMAPMILESTONESYSID AND ROADMAPSYSID = :ROADMAPSYSID",
            new RoadmapMilestone { RoadmapMilestoneSysId = roadmapmilestonesysid, RoadmapSysId = roadmapsysid });
        }

        //[Obsolete("Use GetListAsync(string roadmapsysid = null, string formsysid = null) instead.", true)]
        public override Task<IEnumerable<RoadmapMilestone>> GetListAsync() => throw new NotImplementedException();
    }
}
