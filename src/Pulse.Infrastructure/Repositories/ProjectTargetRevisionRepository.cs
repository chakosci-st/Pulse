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
    public class ProjectTargetRevisionRepository : BaseRepository<ProjectTargetRevision, string>, IProjectTargetRevisionRepository
    {
        public ProjectTargetRevisionRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectTargetRevision revision)
        {
            try
            {
                var returnvalue = await _dataAccess.SaveDataReturnParameterNameAsync<ProjectTargetRevision>(@"  
INSERT INTO PROJECTTARGETREVISIONS
      ( 
          PROJECTNO,
          PROJECTNODESYSID,
          ENTITYSYSID,
          ENTITYTYPE,
          TARGETSTARTDATE,
          TARGETCOMPLETIONDATE,
          REASON,
          CREATEDBY 
      )
    VALUES
      (
          :PROJECTNO,
          :PROJECTNODESYSID,
          :ENTITYSYSID,
          :ENTITYTYPE,
          :TARGETSTARTDATE,
          :TARGETCOMPLETIONDATE,
          :REASON,
          :CREATEDBY 
      ) 
RETURNING TARGETREVISIONSYSID INTO :TARGETREVISIONSYSID
", revision, "TARGETREVISIONSYSID");


                return returnvalue;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message.ToString());
            }

        }



        public async Task<IList<ProjectTargetRevision>> GetAllRevisionsAsync(string projectNo, string nodeType, string nodeId)
        {
            return await _dataAccess.LoadDataAsync<ProjectTargetRevision>("SELECT * FROM PROJECTTARGETREVISIONS WHERE PROJECTNO = :PROJECTNO AND ENTITYTYPE = :ENTITYTYPE AND ENTITYSYSID = :ENTITYSYSID",
new ProjectTargetRevision { ProjectNo = projectNo, EntityType = nodeType, EntitySysId = nodeId });
        }

        public override Task<ProjectTargetRevision> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ProjectTargetRevision>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<int> UpdateAsync(ProjectTargetRevision entity)
        {
            throw new NotImplementedException();
        }

        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
