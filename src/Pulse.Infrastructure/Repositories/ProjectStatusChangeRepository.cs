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
    public class ProjectStatusChangeRepository : BaseRepository<ProjectStatusChange, string>, IProjectStatusChangeRepository
    {
        public ProjectStatusChangeRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectStatusChange status)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectStatusChange>(@"  INSERT INTO PROJECTSTATUSCHANGES
      (
        PROJECTNO,
        ENTITYTYPE,
        ENTITYSYSID,
        ACTUALDATE,
        STATUS,
        REMARKS,
        CREATEDBY
      )
    VALUES
      (
        :PROJECTNO,
        :ENTITYTYPE,
        :ENTITYSYSID,
        :ACTUALDATE,
        :STATUS,
        :REMARKS,
        :CREATEDBY
      )
RETURNING STATUSCHANGESYSID INTO :STATUSCHANGESYSID
", status, "STATUSCHANGESYSID");
        }

        public async Task<IEnumerable<ProjectStatusChange>> GetListByProject(string projectno) 
        {
            return await _dataAccess.LoadDataAsync<ProjectStatusChange>("SELECT * FROM PROJECTSTATUSCHANGES WHERE PROJECTNO = :PROJECTNO",
                new ProjectStatusChange { ProjectNo = projectno });
        }

        public async Task<IEnumerable<ProjectStatusChange>> GetListByEntity(string entitytype, string entitysysid)
        {
            return await _dataAccess.LoadDataAsync<ProjectStatusChange>("SELECT * FROM PROJECTSTATUSCHANGES WHERE ENTITYTYPE = :ENTITYTYPE AND ENTITYSYSID = :ENTITYSYSID",
                new ProjectStatusChange { EntityType = entitytype, EntitySysId = entitysysid });
        } 

        public override Task<int> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<ProjectStatusChange> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<ProjectStatusChange>> GetListAsync()
        {
            throw new NotImplementedException();
        }



        public override Task<int> UpdateAsync(ProjectStatusChange entity)
        {
            throw new NotImplementedException();
        }
    }
}
