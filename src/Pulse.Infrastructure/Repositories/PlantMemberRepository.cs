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
    public class PlantMemberRepository : BaseRepository<PlantMember, string>, IPlantMemberRepository
    {

        public PlantMemberRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(PlantMember link)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<PlantMember>("INSERT INTO PLANTMEMBERS (PLANTCODE, USERID, CREATEDBY) VALUES (:PLANTCODE, :USERID, :CREATEDBY) RETURNING PLANTMEMBERSYSID INTO :PLANTMEMBERSYSID", link, "PLANTMEMBERSYSID");
        }

        public override async Task<int> UpdateAsync(PlantMember link)
        {
            return await _dataAccess.SaveDataAsync<PlantMember>("UPDATE PLANTMEMBERS SET ISACTIVE = :ISACTIVE, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE PLANTMEMBERSYSID = :PLANTMEMBERSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", link);
        }


        public override async Task<int> DeleteAsync(string id)
        {
            return await _dataAccess.SaveDataAsync<PlantMember>("DELETE FROM PLANTMEMBERS WHERE PLANTMEMBERSYSID = :PLANTMEMBERSYSID", new PlantMember { PlantMemberSysId = id });
        }

        public async override Task<PlantMember> GetAsync(string plantmembersysid)
        {
            return await _dataAccess.FindDataAsync<PlantMember>("SELECT * FROM PLANTMEMBERS  WHERE PLANTMEMBERSYSID = :PLANTMEMBERSYSID ",
                new PlantMember { PlantMemberSysId = plantmembersysid });
        }
        public async override Task<IEnumerable<PlantMember>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<PlantMember>("SELECT * FROM USERGROUPMEMBERS");
        }

        public async Task<IEnumerable<PlantMember>> GetListAsync(string plantcode = null, string userid = null)
        {

            var returnvalue = await _dataAccess.GetMappedDataAsync<PlantMember, Plant, User, PlantMember>(
                           dataQuery: @"
SELECT pm.*,
       p.PLANTCODE plant_code,
       p.PLANTNAME,
       u.USERID user_id,
       u.FIRSTNAME,
       u.LASTNAME,
       u.EMAIL
  FROM PLANTMEMBERS pm
       INNER JOIN PLANTS p
          ON p.PLANTCODE = pm.PLANTCODE
       INNER JOIN USERS u
          ON u.USERID = pm.USERID
 WHERE (:PLANTCODE IS NULL OR pm.PLANTCODE = :PLANTCODE)
       AND (:USERID IS NULL OR pm.USERID = :USERID)
",
                           parameters: new { PlantCode = plantcode, UserId = userid },
                           map: (pm, p, u) => new PlantMember
                           {
                               PlantMemberSysId = pm.PlantMemberSysId,
                               PlantCode = pm.PlantCode,
                               UserId = pm.UserId,
                               IsActive = pm.IsActive,
                               CreatedBy = pm.CreatedBy,
                               CreatedDate = pm.CreatedDate,
                               ModifiedBy = pm.ModifiedBy,
                               ModifiedDate = pm.ModifiedDate,
                               TransactionKey = pm.TransactionKey,
                               // From User
                               UserInfo = new User
                               {
                                   UserId = pm.UserId,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName
                               },

                               // From UserGroup
                               PlantInfo = new Plant
                               {
                                   PlantCode = pm.PlantCode,
                                   PlantName = p.PlantName,
                               }
                           },
                           splitOn: "plant_code,user_id"
                       );

            return returnvalue;

        }

    }
}
