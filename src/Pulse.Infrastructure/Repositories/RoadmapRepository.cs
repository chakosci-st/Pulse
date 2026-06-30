
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
    public class RoadmapRepository : BaseRepository<Roadmap, string>, IRoadmapRepository
    {

        public RoadmapRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(Roadmap roadmap)
        {
            var rowsaffected =  await _dataAccess.SaveDataAsync(@"INSERT INTO ROADMAPS (ROADMAPSYSID, ROADMAPNAME, ROADMAPDESCRIPTION, CATEGORYCODE, CREATEDBY) 
VALUES (:ROADMAPSYSID, :ROADMAPNAME, :ROADMAPDESCRIPTION, :CATEGORYCODE, :CREATEDBY)", roadmap);

            if (rowsaffected > 0) return roadmap.RoadmapSysId;
            else
                throw new Exception("Insert failed");
        }

        public override async Task<int> UpdateAsync(Roadmap roadmap)
        {
            return await _dataAccess.SaveDataAsync<Roadmap>(@"UPDATE ROADMAPS 
                        SET ROADMAPNAME = :ROADMAPNAME, ROADMAPDESCRIPTION = :ROADMAPDESCRIPTION, CATEGORYCODE = :CATEGORYCODE, ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE ROADMAPSYSID = :ROADMAPSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", roadmap);
        }

        public override async Task<int> DeleteAsync(string roadmapsysid)
        {
            return await _dataAccess.SaveDataAsync<Roadmap>("DELETE FROM ROADMAPS WHERE ROADMAPSYSID = :ROADMAPSYSID", new Roadmap { RoadmapSysId = roadmapsysid });
        }


        public override async Task<IEnumerable<Roadmap>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Roadmap>("SELECT * FROM ROADMAPS")
                .ContinueWith(t => (IEnumerable<Roadmap>)t.Result);
        }


        public async Task<PagedResult<RoadmapExtended>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
  SELECT *
  FROM (SELECT rm.*, ROW_NUMBER () OVER (ORDER BY {sortBy} {sortDirection}) rn
            FROM (SELECT rm.*,
                         c.categoryname,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname
                    FROM roadmaps rm
                         INNER JOIN CATEGORIES c
                            ON c.categorycode = rm.categorycode
                         INNER JOIN USERS cb
                            ON cb.userid = rm.createdby
                         LEFT OUTER JOIN USERS mb
                            ON mb.userid = rm.modifiedby
                   WHERE (LOWER (rm.roadmapname) LIKE LOWER (:searchvalue) || '%'
                          OR LOWER (rm.roadmapdescription) LIKE                                LOWER (:searchvalue) || '%')
                         AND (:isactivestate IS NULL
                              OR rm.ISACTIVE = :isactivestate)) rm)
   WHERE rn BETWEEN :offset + 1 AND :offset + :pagesize
ORDER BY rn
";

            string countQuery = @"
SELECT COUNT(1)
FROM roadmaps rm
WHERE (LOWER (rm.roadmapname) LIKE LOWER (:searchvalue) || '%'
    OR LOWER (rm.roadmapdescription) LIKE  '%' || LOWER (:searchvalue) || '%')
    AND (:isactivestate IS NULL OR rm.ISACTIVE = :isactivestate)
";


            var parameters = new
            {
                searchvalue = searchValue,
                isactivestate = (isActive == null ? (char?)null : (isActive.Value ? '1' : '0')),
                offset = (pageNumber - 1) * pageSize,
                pagesize = pageSize
            };
            try
            {
                // Use Dapper's QueryAsync for mapping
                int totalRecords = await _dataAccess.ExecuteScalarAsync<int>(countQuery, parameters);

                var data = (await _dataAccess.QueryAsync<RoadmapExtended>(pagedQuery, parameters)).ToList();

                return new PagedResult<RoadmapExtended>
                {
                    TotalRecords = totalRecords,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public override async Task<Roadmap> GetAsync(string roadmapsysid)
        {
            return await GetInternalAsync(roadmapsysid);
        }
        public Roadmap Get(string roadmapsysid)
        {
            return GetInternalAsync(roadmapsysid).GetAwaiter().GetResult();
        }


        private async Task<Roadmap> GetInternalAsync(string roadmapsysid)
        {
            return await _dataAccess.FindDataAsync<Roadmap>("SELECT * FROM ROADMAPS WHERE ROADMAPSYSID = :ROADMAPSYSID",
                new Roadmap { RoadmapSysId = roadmapsysid });
        }

        private async Task<RoadmapExtended> GetCompleteInfoInternalAsync(string roadmapsysid)
        {
            var sql = @"SELECT rm.*,
                         cb.firstname || ' ' || cb.lastname createdbyname,
                         mb.firstname || ' ' || mb.lastname modifiedbyname
                    FROM roadmaps rm
                         INNER JOIN USERS cb
                            ON cb.userid = rm.createdby
                         LEFT OUTER JOIN USERS mb
                            ON mb.userid = rm.modifiedby
                    WHERE rm.roadmapsysid = :roadmapsysid";

            return await _dataAccess.FindDataAsync<RoadmapExtended>(sql, new RoadmapExtended { RoadmapSysId = roadmapsysid });

        }


        public RoadmapExtended GetCompleteInfo(string roadmapsysid)
        {
            return GetCompleteInfoInternalAsync(roadmapsysid).GetAwaiter().GetResult();
        }

        public async Task<RoadmapExtended> GetCompleteInfoAsync(string roadmapsysid)
        {
            return await GetCompleteInfoInternalAsync(roadmapsysid);
        }

        public async Task<IEnumerable<NodeRow>> GetNodes(string roadmapsysid)
        {
            var sql = @"SELECT
        act.roadmapactivitysysid AS nodeid,
        act.roadmapactivitysysid AS nodekey,
        act.roadmapsysid,
        act.parenttype,
        act.parentsysid,
        NULL                      AS datamaturitycode,
        act.activityname          AS dataname,
        act.activitydescription   AS datadescription,
        act.estimatedmandays      AS datamandays,
        act.isrequired            AS dataisrequired,
        act.orderindex,
        act.isactive, 
        act.transactionkey,
        'activity'                AS nodetype
    FROM ROADMAPACTIVITIES act WHERE act.roadmapsysid = :roadmapsysid
    UNION ALL 
    SELECT
        mil.roadmapmilestonesysid AS nodeid,
        mil.roadmapmilestonesysid AS nodekey,
        mil.roadmapsysid,
        mil.parenttype,
        mil.parentsysid,
        mil.maturitycode          AS datamaturitycode,
        mil.milestonealias        AS dataname,
        mil.milestonedescription  AS datadescription,
        NULL                      AS datamandays,
        isrequired                AS dataisrequired,
        mil.orderindex,
        mil.isactive,
        mil.transactionkey,
        'milestone'               AS nodetype
    FROM ROADMAPMILESTONES mil WHERE mil.roadmapsysid = :roadmapsysid";

            return await _dataAccess.LoadDataAsync<NodeRow>(sql, new NodeRow { RoadmapSysId = roadmapsysid });
        }
    }
}
