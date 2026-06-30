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
    public class PlantRoadmapLinkRepository : BaseRepository<PlantRoadmapLink, string>, IPlantRoadmapLinkRepository 
    {
        public PlantRoadmapLinkRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(PlantRoadmapLink link)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<PlantRoadmapLink>(@"INSERT INTO PLANTROADMAPLINKS (PLANTCODE, ROADMAPSYSID) VALUES (:PLANTCODE, :ROADMAPSYSID) RETURNING PLANTROADMAPLINKSYSID INTO :PLANTROADMAPLINKSYSID",
                link, "PLANTROADMAPLINKSYSID");

        }

        public override async Task<int> UpdateAsync(PlantRoadmapLink link)
        {
            return await _dataAccess.SaveDataAsync<PlantRoadmapLink>(@"UPDATE PLANTROADMAPLINKS SET ISACTIVE=:ISACTIVE WHERE (:PLANTROADMAPLINKSYSID IS NULL OR PLANTROADMAPLINKSYSID = :PLANTROADMAPLINKSYSID) AND  (PLANTCODE = :PLANTCODE AND ROADMAPSYSID = :ROADMAPSYSID)", link);
        }

        public async Task<IEnumerable<PlantRoadmapLinkExtended>> GetLinkListAsync(string plantcode = null, string roadmapsysid = null)
        {
            var returnvalue = await _dataAccess.GetMappedDataAsync<Plant, Roadmap, Category, PlantRoadmapLinkExtended, PlantRoadmapLinkExtended >(
                           dataQuery: @"
SELECT rmpl.*,
       lnk.plantroadmaplinksysid,
       lnk.isactive,
       DECODE (lnk.plantroadmaplinksysid, NULL, 0, lnk.isactive) isselected
  FROM    (SELECT pl.plantcode,
                  pl.plantname,
                  rm.roadmapsysid,
                  rm.roadmapname,
                  rm.roadmapdescription,
                  rm.categorycode,
                  c.categoryname,
                  rm.isactive rmisactive
             FROM roadmaps rm INNER JOIN categories c ON c.categorycode = rm.categorycode CROSS JOIN plants pl) rmpl
       LEFT OUTER JOIN
          plantroadmaplinks lnk
       ON lnk.roadmapsysid = rmpl.roadmapsysid
          AND lnk.plantcode = rmpl.plantcode
WHERE (:PLANTCODE IS NULL OR rmpl.plantcode = :PLANTCODE)
AND (:ROADMAPSYSID IS NULL OR rmpl.roadmapsysid = :ROADMAPSYSID) 
",
                           parameters: new { PlantCode = plantcode, RoadmapSysid = roadmapsysid },
                           map: (p, r, c, lnk) => new PlantRoadmapLinkExtended
                           {
                               PlantRoadmapLinkSysId = lnk.PlantRoadmapLinkSysId,
                               PlantCode = p.PlantCode,
                               RoadmapSysId = r.RoadmapSysId,
                               IsActive = lnk.IsActive,
                               IsSelected = lnk.IsSelected,
                               // From Plant
                               Plant = new Plant
                               {
                                   PlantCode = p.PlantCode,
                                   PlantName = p.PlantName 
                               },

                               // From Roadmap
                               Roadmap = new Roadmap
                               {
                                   RoadmapSysId = r.RoadmapSysId,
                                   RoadmapName = r.RoadmapName,
                                   RoadmapDescription = r.RoadmapDescription,
                                   CategoryCode = c.CategoryCode,
                                   IsActive = lnk.RMIsActive,
                                   Category = new Category {
                                       CategoryCode = c.CategoryCode,
                                       CategoryName = c.CategoryName,
                                       CategoryDescription = c.CategoryDescription
                                   }
                               }
                           },
                           splitOn: "roadmapsysid,categorycode,rmisactive"
                       );

            return returnvalue;
        }
         





        public override async Task<PlantRoadmapLink> GetAsync(string plantroadmaplinksysid)
        {
            return await _dataAccess.FindDataAsync<PlantRoadmapLink>("SELECT * FROM FORMENTITYLINKS WHERE FORMENTITYLINKSYSID = :FORMENTITYLINKSYSID",
            new PlantRoadmapLink { PlantRoadmapLinkSysId = plantroadmaplinksysid });
        }

        public override async Task<int> DeleteAsync(string plantRoadmapLinkSysId)
        {
            return await _dataAccess.SaveDataAsync<PlantRoadmapLinkExtended>("DELETE FROM FORMENTITYLINKS WHERE FORMENTITYLINKSYSID = :FORMENTITYLINKSYSID",
            new PlantRoadmapLinkExtended { PlantRoadmapLinkSysId = plantRoadmapLinkSysId });
        }
         
        //[Obsolete("Use GetListAsync(string roadmapsysid = null, string formsysid = null) instead.", true)]
        public override Task<IEnumerable<PlantRoadmapLink>> GetListAsync() => throw new NotImplementedException();

        public async Task<IEnumerable<NodeFormRow>> GetNodeFormsAsync(string parentsysid, string type)
        {
            return await _dataAccess.LoadDataAsync<NodeFormRow>(@"
SELECT
    lnk.formentitylinksysid AS formnodekey,
    lnk.formentitylinksysid AS formnodeid,
    lnk.formsysid           AS sysid,
    f.formname,
    f.formdescription,
    lnk.entitysysid         AS nodekey,
    lnk.entitytype ParentType,
    lnk.entitysysid ParentSysId
FROM formentitylinks lnk
JOIN forms f ON f.formsysid = lnk.formsysid
WHERE lnk.entitytype = :ParentType
  AND lnk.entitysysid = :ParentSysId", new NodeFormRow { ParentSysId = parentsysid, ParentType = type });
        }

        public async Task<IEnumerable<RootFormRow>> GetRootNodeFormsAsync(string roadmapsysid)
        {
            return await _dataAccess.LoadDataAsync<RootFormRow>(@"
SELECT
    lnk.formentitylinksysid AS formrootkey,
    lnk.formentitylinksysid AS formrootid,
    lnk.formsysid           AS sysid,
    f.formname,
    f.formdescription,
    lnk.entitytype ParentType,
    lnk.entitysysid ParentSysId
FROM formentitylinks lnk
JOIN forms f ON f.formsysid = lnk.formsysid
WHERE lnk.entitytype = 'roadmap'
  AND lnk.entitysysid = :ParentSysId", new RootFormRow { ParentSysId = roadmapsysid });
        }

 
    }
}
