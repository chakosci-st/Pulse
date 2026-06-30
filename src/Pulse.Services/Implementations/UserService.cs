using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupMemberRepository _usergroupmemberRepository;

        public UserService(IUserRepository userRepositorysitory, IUserGroupMemberRepository usergroupmemberRepository)
        {
            _userRepository = userRepositorysitory;
            _usergroupmemberRepository = usergroupmemberRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetListAsync();
        }

        public async Task<User> GetUserByIdAsync(string userid)
        {
            return await _userRepository.GetAsync(userid);
        }
        public async Task<User> GetUserByUserNameAsync(string username)
        {
            return await _userRepository.GetByUserNameAsync(username);
        }
        public async Task<string> AddUserAsync(User user)
        {
            var returnvalue = await _userRepository.AddAsync(user);

            // Publish the UserCreatedEvent
            //var userCreatedEvent = new UserCreatedEvent
            //{
            //    UserCode = user.UserCode,
            //    ActionBy = user.CreatedBy
            //};



            return returnvalue;
        }

        public async Task<int> UpdateUserAsync(User user)
        {

            var rowsaffected = await _userRepository.UpdateAsync(user);

            // Publish the UserUpdatedEvent
            //var userUpdatedEvent = new UserUpdatedEvent
            //{
            //    UserCode = user.UserCode,
            //    ActionBy = user.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(userUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public async Task<int> DeleteUserAsync(string userid, string loggeduser)
        {


            try
            {
                //GET PLANT INFO
                var obj = await _userRepository.GetAsync(userid);


                //SET USER WHO DELETES THE User
                obj.ModifiedBy = loggeduser;
                await _userRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _userRepository.DeleteAsync(userid);


                // Publish the UserUpdatedEvent
                //var userDeletedEvent = new UserDeletedEvent
                //{
                //    UserCode = user.UserCode,
                //    ActionBy = user.CreatedBy
                //};


                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(userDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<IEnumerable<UserGroupMember>> GetUserGroupsAsync(string userid)
        { 
            return await _usergroupmemberRepository.GetListAsync(userid: userid);
        }

 
    }
}
