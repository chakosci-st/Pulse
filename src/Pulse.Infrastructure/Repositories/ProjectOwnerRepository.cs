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
    public class ProjectOwnerRepository : BaseRepository<ProjectOwner, string>, IProjectOwnerRepository
    {
        public ProjectOwnerRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectOwner projectmember)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectOwner>(@"  INSERT INTO PROJECTOWNERS
        (
            PROJECTNO
            ,USERID
            ,PARENTTYPE
            ,PARENTSYSID
        )
        VALUES
        (
            :PROJECTNO
            ,:USERID
            ,:PARENTTYPE
            ,:PARENTSYSID
        )
RETURNING PROJECTOWNERSYSID INTO :PROJECTOWNERSYSID
", projectmember, "PROJECTOWNERSYSID");
        }



        public override async Task<int> DeleteAsync(string projectownerno)
        {
            return await _dataAccess.SaveDataAsync<ProjectOwner>("DELETE FROM PROJECTOWNERS WHERE PROJECTOWNERSYSID = :PROJECTOWNERSYSID", new ProjectOwner { ProjectOwnerSysId = projectownerno });
        }
        public Task<IEnumerable<ProjectOwner>> GetListAsync(string parentsysid, string parenttype)
        {
            throw new NotImplementedException();
        }

        public async Task<ProjectOwner> GetAsync(string projectno, string memberid, string parenttype, string parentsysid)
        {
            return await _dataAccess.FindDataAsync<ProjectOwner>("SELECT * FROM PROJECTOWNERS WHERE PROJECTNO = :PROJECTNO AND USERID = :USERID AND PARENTTYPE = :PARENTTYPE AND PARENTSYSID = :PARENTSYSID", new ProjectOwner { ProjectNo = projectno, UserId = memberid, ParentSysId = parentsysid, ParentType = parenttype });
        }


        public override Task<IEnumerable<ProjectOwner>> GetListAsync()
        {
            throw new NotImplementedException();
        }
        public override Task<ProjectOwner> GetAsync(string id)
        {
            throw new NotImplementedException();
        }
        public override Task<int> UpdateAsync(ProjectOwner entity)
        {
            throw new NotImplementedException();
        }
    }
}
