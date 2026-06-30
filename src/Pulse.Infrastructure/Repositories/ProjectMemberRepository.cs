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
    public class ProjectMemberRepository : BaseRepository<ProjectMember, string>, IProjectMemberRepository
    {
        string sql = @"SELECT pm.projectmembersysid,
       pm.projectno,
       pm.createdby,
       pm.createddate,
       pm.modifiedby,
       pm.modifieddate,
       pm.transactionkey,
        pm.isowner,
       pm.userid,
       u.username,
       u.firstname,
       u.lastname,
       u.email
  FROM projectmembers pm INNER JOIN users u ON u.userid = pm.userid";

        public ProjectMemberRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(ProjectMember projectmember)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<ProjectMember>(@"  INSERT INTO PROJECTMEMBERS
      (
       PROJECTNO
      ,USERID
        ,ISOWNER
      ,CREATEDBY  
      )
    VALUES
      (
       :PROJECTNO
      ,:USERID
        ,:ISOWNER
      ,:CREATEDBY  
      )
RETURNING PROJECTMEMBERSYSID INTO :PROJECTMEMBERSYSID
", projectmember, "PROJECTMEMBERSYSID");
        }

        public override async Task<int> UpdateAsync(ProjectMember projectmember)
        {
            return await _dataAccess.SaveDataAsync<ProjectMember>(@"UPDATE PROJECTMEMBERS 
                        SET PROJECTNO = :PROJECTNO
      ,USERID = :USERID
        ,ISOWNER = :ISOWNER
      ,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PROJECTMEMBERSYSID = :PROJECTMEMBERSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", projectmember);
        }

        public override async Task<int> DeleteAsync(string projectmemberno)
        {
            return await _dataAccess.SaveDataAsync<ProjectMember>("DELETE FROM PROJECTMEMBERS WHERE PROJECTMEMBERSYSID = :PROJECTMEMBERSYSID", new ProjectMember { ProjectMemberSysId = projectmemberno });
        }

        public override async Task<IEnumerable<ProjectMember>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<ProjectMember>("SELECT * FROM PROJECTMEMBERS")
                  .ContinueWith(t => (IEnumerable<ProjectMember>)t.Result);
        }

        public override async Task<ProjectMember> GetAsync(string projectmemberno)
        {

            var returnvalue =
             await _dataAccess.GetMappedDataAsync<ProjectMember, User, ProjectMember>(
   dataQuery: sql + "  WHERE PROJECTMEMBERSYSID = :PROJECTMEMBERSYSID",
   parameters: new { ProjectMemberSysId = projectmemberno },
   map: (pm, u) => new ProjectMember
   {
       ProjectMemberSysId = pm.ProjectMemberSysId,
       ProjectNo = pm.ProjectNo,
       IsOwner = pm.IsOwner,
       CreatedBy = pm.CreatedBy,
       CreatedDate = pm.CreatedDate,
       ModifiedBy = pm.ModifiedBy,
       ModifiedDate = pm.ModifiedDate,
       TransactionKey = pm.TransactionKey,
       UserId = u.UserId,
       // From User
       User = new User
       {
           UserId = u.UserId,
           FirstName = u.FirstName,
           LastName = u.LastName,
           Email = u.Email,
           UserName = u.UserName
       }
   },
   splitOn: "userid"
);

            return returnvalue.SingleOrDefault();

        }


        public async Task<IEnumerable<ProjectMember>> GetListAsync(string projectno)
        {

            var returnvalue =
             await _dataAccess.GetMappedDataAsync<ProjectMember, User, ProjectMember>(
   dataQuery: sql + "  WHERE PROJECTNO = :PROJECTNO",
   parameters: new { ProjectNo = projectno },
   map: (pm, u) => new ProjectMember
   {
       ProjectMemberSysId = pm.ProjectMemberSysId,
       ProjectNo = pm.ProjectNo,
       IsOwner = pm.IsOwner,
       CreatedBy = pm.CreatedBy,
       CreatedDate = pm.CreatedDate,
       ModifiedBy = pm.ModifiedBy,
       ModifiedDate = pm.ModifiedDate,
       TransactionKey = pm.TransactionKey,
       UserId = u.UserId,
       // From User
       User = new User
       {
           UserId = u.UserId,
           FirstName = u.FirstName,
           LastName = u.LastName,
           Email = u.Email,
           UserName = u.UserName
       }
   },
   splitOn: "userid"
);

            return returnvalue;

        }

        public async Task<IEnumerable<ProjectMember>> GetByMemberIdAsync(string memberid)
        {
            var returnvalue =
             await _dataAccess.GetMappedDataAsync<ProjectMember, User, ProjectMember>(
   dataQuery: sql + "  WHERE pm.USERID = :USERID",
        parameters: new { UserId = memberid },
   map: (pm, u) => new ProjectMember
   {
       ProjectMemberSysId = pm.ProjectMemberSysId,
       ProjectNo = pm.ProjectNo,
       IsOwner = pm.IsOwner,
       CreatedBy = pm.CreatedBy,
       CreatedDate = pm.CreatedDate,
       ModifiedBy = pm.ModifiedBy,
       ModifiedDate = pm.ModifiedDate,
       TransactionKey = pm.TransactionKey,
       UserId = u.UserId,
       // From User
       User = new User
       {
           UserId = u.UserId,
           FirstName = u.FirstName,
           LastName = u.LastName,
           Email = u.Email,
           UserName = u.UserName
       }
   },
   splitOn: "userid"
);

            return returnvalue;


        }



    }
}
