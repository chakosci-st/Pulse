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
    public class UserGroupAccessRightRepository : BaseRepository<UserGroupAccessRight, string>, IUserGroupAccessRightRepository
    {

        public UserGroupAccessRightRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(UserGroupAccessRight accessright)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<UserGroupAccessRight>("INSERT INTO USERGROUPACCESSRIGHTS (MODULECODE, USERGROUPID, CREATEDBY) VALUES (:MODULECODE, :USERGROUPID, :CREATEDBY) RETURNING USERGROUPACCESSRIGHTSYSID INTO :USERGROUPACCESSRIGHTSYSID", accessright, "USERGROUPACCESSRIGHTSYSID");
        }

        public override async Task<int> UpdateAsync(UserGroupAccessRight accessright)
        {
            return await _dataAccess.SaveDataAsync<UserGroupAccessRight>("UPDATE USERGROUPACCESSRIGHTS SET MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE USERGROUPACCESSRIGHTSYSID = :USERGROUPACCESSRIGHTSYSID AND TRANSACTIONKEY = :TRANSACTIONKEY", accessright);
        }


        public override async Task<int> DeleteAsync(string usergroupaccessrightsysid)
        {
            return await _dataAccess.SaveDataAsync<UserGroupAccessRight>("DELETE FROM USERGROUPACCESSRIGHTS WHERE USERGROUPACCESSRIGHTSYSID = :USERGROUPACCESSRIGHTSYSID ", new UserGroupAccessRight { UserGroupAccessRightSysId = usergroupaccessrightsysid });
        }
        public override async Task<UserGroupAccessRight> GetAsync(string usergroupaccessrightsysid)
        {
            return await _dataAccess.FindDataAsync<UserGroupAccessRight>("SELECT* FROM USERGROUPACCESSRIGHTS  WHERE USERGROUPACCESSRIGHTSYSID = :USERGROUPACCESSRIGHTSYSID ",
                new UserGroupAccessRight { UserGroupAccessRightSysId = usergroupaccessrightsysid });
        }
        public async Task<UserGroupAccessRight> GetAsync(int usergroupid, string modulecode)
        {
            return await _dataAccess.FindDataAsync<UserGroupAccessRight>("SELECT * FROM USERGROUPACCESSRIGHTS WHERE MODULECODE = :MODULECODE AND USERGROUPID = :USERGROUPID ", new UserGroupAccessRight { UserGroupId = usergroupid, ModuleCode = modulecode });
        } 

        public async override Task<IEnumerable<UserGroupAccessRight>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<UserGroupAccessRight>("SELECT * FROM USERGROUPACCESSRIGHTS")
       .ContinueWith(t => (IEnumerable<UserGroupAccessRight>)t.Result);
        }
        public async Task<IEnumerable<UserGroupAccessRight>> GetListAsync(int? usergroupid = null, string modulecode = null)
        {
            return await _dataAccess.LoadDataAsync<UserGroupAccessRight>("SELECT * FROM USERGROUPACCESSRIGHTS  WHERE (MODULECODE = :MODULECODE OR :MODULECODE IS NULL)  AND (USERGROUPID = :USERGROUPID OR :USERGROUPID IS NULL) ")
       .ContinueWith(t => (IEnumerable<UserGroupAccessRight>)t.Result);
        }



        public async Task<IEnumerable<UserGroupModule>> GetModulesAsync(int usergroupid)
        {

            return await _dataAccess.LoadDataAsync<UserGroupModule>(@"
WITH ug AS (SELECT usergroupaccessrightsysid, UserGroupId, modulecode
              FROM USERGROUPACCESSRIGHTS
             WHERE usergroupid = :UserGroupId)
SELECT mdl.modulecode,
       mdl.modulename,
       mdl.moduledescription,
       mdl.isactive,
       NVL2 (usergroupaccessrightsysid, 1, 0) isselected,
       UserGroupId,
       usergroupaccessrightsysid id
  FROM MODULES mdl LEFT OUTER JOIN ug ON ug.modulecode = mdl.modulecode
", new UserGroupModule { UserGroupId = usergroupid })
  .ContinueWith(t => (IEnumerable<UserGroupModule>)t.Result);
        }


    }
}
