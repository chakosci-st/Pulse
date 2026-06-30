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
    public class UserRepository : BaseRepository<User, string>, IUserRepository
    {
        public UserRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        public override async Task<string> AddAsync(User user)
        {
            try {
                var returnvalue = await _dataAccess.SaveDataAsync<User>("INSERT INTO USERS (USERID, USERNAME, FIRSTNAME, LASTNAME, EMAIL, ISACTIVE, CREATEDBY, DASHBOARDSHOWALLUSERS) VALUES (:USERID, :USERNAME, :FIRSTNAME, :LASTNAME, :EMAIL, 1,  :CREATEDBY, :DASHBOARDSHOWALLUSERS)", user);
                return user.UserId;
            }
            catch (Exception e) {
                throw new Exception(e.Message);
            }
 
        }

        public override async Task<int> UpdateAsync(User user)
        {
            return await _dataAccess.SaveDataAsync<User>("UPDATE USERS SET USERNAME = :USERNAME, FIRSTNAME = :FIRSTNAME, LASTNAME = :LASTNAME, EMAIL = :EMAIL, ISACTIVE = :ISACTIVE, DASHBOARDSHOWALLUSERS = :DASHBOARDSHOWALLUSERS, MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() WHERE USERID = :USERID AND TRANSACTIONKEY = :TRANSACTIONKEY", user);
        }

        public override async Task<int> DeleteAsync(string userid)
        {
            return await _dataAccess.SaveDataAsync<User>("DELETE FROM USERS WHERE USERID = :USERID", new User { UserId = userid });
        }

        public override async Task<IEnumerable<User>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<User>("SELECT * FROM USERS")
            .ContinueWith(t => (IEnumerable<User>)t.Result);
        }

        public override async Task<User> GetAsync(string userid)
        {
            return await _dataAccess.FindDataAsync<User>("SELECT usr.*, 1 Registered FROM USERS usr WHERE USERID = :USERID", new User { UserId = userid });
        }

        public async Task<User> GetByUserNameAsync(string username)
        {
            return await _dataAccess.FindDataAsync<User>("SELECT usr.*, 1 Registered FROM USERS usr WHERE USERNAME = :USERNAME", new User { UserName = username });
        }


      
    }
}
