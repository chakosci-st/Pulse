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
    public class FormEntityLinkRepository : BaseRepository<FormEntityLink, string>, IFormEntityLinkRepository
    {
        public FormEntityLinkRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(FormEntityLink link)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<FormEntityLink>(@"INSERT INTO FORMENTITYLINKS (FORMSYSID, ENTITYTYPE, ENTITYSYSID, ORDERINDEX, CREATEDBY) VALUES (:FORMSYSID, :ENTITYTYPE, :ENTITYSYSID, :ORDERINDEX, :CREATEDBY) RETURNING FORMENTITYLINKSYSID INTO :FORMENTITYLINKSYSID",
                link, "FORMENTITYLINKSYSID");

        }

        public override async Task<int> UpdateAsync(FormEntityLink link)
        {
            return await _dataAccess.SaveDataAsync<FormEntityLink>(@"UPDATE FORMENTITYLINKS SET ORDERINDEX=:ORDERINDEX, ISACTIVE=:ISACTIVE, MODIFIEDBY=:MODIFIEDBY, MODIFIEDDATE=:MODIFIEDDATE WHERE FORMENTITYLINKSYSID = :FORMENTITYLINKSYSID", link);
        }

        public async Task<IEnumerable<FormEntityLink>> GetListAsync(string entitysysid)
        {
            return await _dataAccess.LoadDataAsync<FormEntityLink>("SELECT * FROM FORMENTITYLINKS WHERE :ENTITYSYSID = :ENTITYSYSID)", new FormEntityLink { EntitySysId = entitysysid });
        }

        public async Task<IEnumerable<FormEntityLink>> GetListByRoadmapAsync(string roadmapsysid)
        {
            return await _dataAccess.LoadDataAsync<FormEntityLink>(@"
SELECT *
  FROM FORMENTITYLINKS
 WHERE entitytype = 'roadmap' AND entitysysid = :EntitySysId
UNION
SELECT*
  FROM FORMENTITYLINKS
 WHERE entitytype = 'milestone'
       AND EXISTS
              (SELECT*
                 FROM roadmapmilestones
                WHERE roadmapsysid = :EntitySysId
                      AND roadmapmilestonesysid = entitysysid)
UNION
SELECT *
  FROM FORMENTITYLINKS
 WHERE entitytype = 'activity'
       AND EXISTS
              (SELECT*
                 FROM roadmapactivities
                WHERE roadmapsysid = :EntitySysId
                      AND roadmapactivitysysid = entitysysid)
", new FormEntityLink { EntitySysId = roadmapsysid });
        }






        public override async Task<FormEntityLink> GetAsync(string formentitylinksysid)
        {
            return await _dataAccess.FindDataAsync<FormEntityLink>("SELECT * FROM FORMENTITYLINKS WHERE FORMENTITYLINKSYSID = :FORMENTITYLINKSYSID",
            new FormEntityLink { FormEntityLinkSysId = formentitylinksysid });
        }

        public override async Task<int> DeleteAsync(string formentitylinksysid)
        {
            return await _dataAccess.SaveDataAsync<FormEntityLink>("DELETE FROM FORMENTITYLINKS WHERE FORMENTITYLINKSYSID = :FORMENTITYLINKSYSID",
            new FormEntityLink { FormEntityLinkSysId = formentitylinksysid });
        }
         
        //[Obsolete("Use GetListAsync(string roadmapsysid = null, string formsysid = null) instead.", true)]
        public override Task<IEnumerable<FormEntityLink>> GetListAsync() => throw new NotImplementedException();

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
