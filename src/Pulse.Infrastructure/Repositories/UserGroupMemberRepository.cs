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
    public class UserGroupMemberRepository : BaseRepository<UserGroupMember, string>, IUserGroupMemberRepository
    {

        public UserGroupMemberRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(UserGroupMember usergroupmember)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<UserGroupMember>(@"
INSERT INTO USERGROUPMEMBERS (USERGROUPID, USERID, CREATEDBY)
     VALUES (:USERGROUPID, :USERID, :CREATEDBY)
  RETURNING USERGROUPMEMBERSYSID
       INTO :USERGROUPMEMBERSYSID
", usergroupmember, "USERGROUPMEMBERSYSID");
        }

        public override async Task<int> UpdateAsync(UserGroupMember usergroupmember)
        {
            return await _dataAccess.SaveDataAsync<UserGroupMember>("UPDATE USERGROUPMEMBERS SET   ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE USERGROUPMEMBERSYSID = :USERGROUPMEMBERSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", usergroupmember);
        }

        public override async Task<int> DeleteAsync(string workitemmembersysid)
        {
            return await _dataAccess.SaveDataAsync<UserGroupMember>("DELETE FROM USERGROUPMEMBERS WHERE USERGROUPMEMBERSYSID = :USERGROUPMEMBERSYSID", new UserGroupMember { UserGroupMemberSysId = workitemmembersysid });
        }
        public async override Task<IEnumerable<UserGroupMember>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<UserGroupMember>("SELECT * FROM USERGROUPMEMBERS")
       .ContinueWith(t => (IEnumerable<UserGroupMember>)t.Result);
        }
        public async Task<IEnumerable<UserGroupMember>> GetListAsync(string userid = null, int? usergroupid = null)
        {
            var returnvalue =
                         await _dataAccess.GetMappedDataAsync<UserGroupMember, UserGroup, User, UserGroupMember>(
               dataQuery: @"
SELECT ugm.usergroupmembersysid,
       ugm.createdby,
       ugm.createddate,
       ugm.TransactionKey,
       ug.usergroupid,
       ug.usergroupname,
       ug.usergroupdescription,
       u.userid,
       u.firstname,
       u.lastname,
       u.firstname,
       u.lastname,
       u.email,
       u.username
  FROM usergroupmembers ugm
       INNER JOIN usergroups ug
          ON ug.usergroupid = ugm.usergroupid
       INNER JOIN users u
          ON u.userid = ugm.userid
 WHERE (:usergroupid IS NULL OR ugm.usergroupid = :usergroupid) AND (:userid IS NULL OR ugm.userid = :userid)
",
               parameters: new { UserGroupId = usergroupid, UserId = userid },
               map: (ugm, ug, u) => new UserGroupMember
               {
                   UserGroupMemberSysId = ugm.UserGroupMemberSysId,
                   UserGroupId = ug.UserGroupId,
                   UserId = u.UserId,
                   IsActive = ugm.IsActive,
                   CreatedBy = ugm.CreatedBy,
                   CreatedDate = ugm.CreatedDate,
                   ModifiedBy = ugm.ModifiedBy,
                   ModifiedDate = ugm.ModifiedDate,
                   TransactionKey = ugm.TransactionKey,

        // From User
        User = new User
                   {
                       UserId = u.UserId,
                       FirstName = u.FirstName,
                       LastName = u.LastName,
                       Email = u.Email,
                       UserName = u.UserName
        },

        // From UserGroup
        UserGroup = new UserGroup
                   {
                       UserGroupId = ug.UserGroupId,
                       UserGroupName = ug.UserGroupName,
                   }
               },
               splitOn: "usergroupid,userid"
           );

            return returnvalue;
        }

        public override async Task<UserGroupMember> GetAsync(string usergroupmembersysid)
        {
            return await _dataAccess.FindDataAsync<UserGroupMember>("SELECT * FROM USERGROUPMEMBERS WHERE USERGROUPMEMBERSYSID = :USERGROUPMEMBERSYSID", new UserGroupMember { UserGroupMemberSysId = usergroupmembersysid });
        }



    }
}
