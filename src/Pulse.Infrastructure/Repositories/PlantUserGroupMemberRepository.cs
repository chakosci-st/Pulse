
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
    public class PlantUserGroupMemberRepository : BaseRepository<PlantUserGroupMember, string>, IPlantUserGroupMemberRepository
    {

        public PlantUserGroupMemberRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(PlantUserGroupMember member)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<PlantUserGroupMember>("INSERT INTO PLANTUSERGROUPMEMBERS (PLANTCODE, USERGROUPID, USERID, CREATEDBY) VALUES (:PLANTCODE, :USERGROUPID, :USERID, :CREATEDBY) RETURNING PLANTUSERGROUPMEMBERSYSID INTO :PLANTUSERGROUPMEMBERSYSID", member, "PLANTUSERGROUPMEMBERSYSID");
        }

        public override async Task<int> UpdateAsync(PlantUserGroupMember member)
        {
            return await _dataAccess.SaveDataAsync<PlantUserGroupMember>("UPDATE PLANTUSERGROUPMEMBERS SET MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE PLANTUSERGROUPMEMBERSYSID = :PLANTUSERGROUPMEMBERSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", member);
        }


        public async override Task<int> DeleteAsync(string plantusergroupmembersysid)
        {
            return await _dataAccess.SaveDataAsync<PlantUserGroupMember>("DELETE FROM PLANTUSERGROUPMEMBERS WHERE PLANTUSERGROUPMEMBERSYSID = :PLANTUSERGROUPMEMBERSYSID", new PlantUserGroupMember { PlantUserGroupMemberSysId = plantusergroupmembersysid });
        }

 
        public override async Task<PlantUserGroupMember> GetAsync(string plantusergroupmembersysid)
        {
            return await _dataAccess.FindDataAsync<PlantUserGroupMember>("SELECT PLANTUSERGROUPMEMBERSYSID, PLANTCODE, USERGROUPID, USERID, CREATEDBY, CREATEDDATE, MODIFIEDBY, MODIFIEDDATE, TRANSACTIONKEY FROM PLANTUSERGROUPMEMBERS WHERE PLANTUSERGROUPMEMBERSYSID = :PLANTUSERGROUPMEMBERSYSID ",
        new PlantUserGroupMember { PlantUserGroupMemberSysId = plantusergroupmembersysid });
        }

        public async override Task<IEnumerable<PlantUserGroupMember>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<PlantUserGroupMember>(@"SELECT * FROM PLANTUSERGROUPMEMBERS ")
              .ContinueWith(t => (IEnumerable<PlantUserGroupMember>)t.Result);
        }
        public async Task<IEnumerable<PlantUserGroupMember>> GetListAsync(string plantcode, int usergroupid)
        {
            return await _dataAccess.LoadDataAsync<PlantUserGroupMember>("SELECT * FROM PLANTUSERGROUPMEMBERS  WHERE PLANTCODE = :PLANTCODE AND USERGROUPID = :USERGROUPID ", new PlantUserGroupMember { PlantCode = plantcode, UserGroupId = usergroupid })
       .ContinueWith(t => (IEnumerable<PlantUserGroupMember>)t.Result);
        }


        public async Task<IEnumerable<PlantUserGroupMember>> GetListByUserIdAsync(string userid)
        {
            return await _dataAccess.LoadDataAsync<PlantUserGroupMember>("SELECT * FROM PLANTUSERGROUPMEMBERS  WHERE USERID = :USERID ", new PlantUserGroupMember { UserId = userid })
       .ContinueWith(t => (IEnumerable<PlantUserGroupMember>)t.Result);
        }


        public async Task<IEnumerable<PlantMember>> GetMembersOnlyListAsync(string plantcode)
        {
            return await _dataAccess.LoadDataAsync<PlantMember>(@"
SELECT DISTINCT usr.*
FROM PLANTUSERGROUPMEMBERS pug INNER JOIN USERS usr 
ON usr.USERID = pug.USERID  
WHERE (:PLANTCODE IS NULL OR PLANTCODE = :PLANTCODE)  
", new PlantMember { PlantCode = plantcode  }).ContinueWith(t => (IEnumerable<PlantMember>)t.Result);
        }
    }
}
