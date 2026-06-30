using Entities = Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class UserGroupMemberService : IUserGroupMemberService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IUserGroupMemberRepository _usergroupmemberRepositoryRepository;
        private readonly IUserRepository _userRepository;
        public UserGroupMemberService(OracleDataAccessLayer dataAccess, IUserRepository userRepository, IUserGroupMemberRepository usergroupmemberRepositoryRepository)
        {
            _dataAccess = dataAccess;
            _usergroupmemberRepositoryRepository = usergroupmemberRepositoryRepository;
            _userRepository = userRepository;
        }

        public async Task<string> AddUserGroupMemberAsync(Entities.UserGroupMember usergroupmember)
        {
            _dataAccess.BeginTransaction();
            try
            {

                usergroupmember.CreatedDate = DateTime.UtcNow;

                var user = await _userRepository.GetAsync(usergroupmember.UserId);

                if (user == null)
                {
                    usergroupmember.User.CreatedBy = usergroupmember.CreatedBy;
                    //Create user
                    await _userRepository.AddAsync(usergroupmember.User);

                }

                var usergroupmembersysid = await _usergroupmemberRepositoryRepository.AddAsync(usergroupmember);


                _dataAccess.CommitTransaction();

                return usergroupmembersysid;
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw;
            }

             
        }

        public async Task<int> DeleteUserGroupMemberAsync(string usergroupmemberid, string userid)
        {
            _dataAccess.BeginTransaction();

            try
            {
                //GET ACCESS INFO
                var obj = await _usergroupmemberRepositoryRepository.GetAsync(usergroupmemberid);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = userid;
                await _usergroupmemberRepositoryRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _usergroupmemberRepositoryRepository.DeleteAsync(usergroupmemberid);
                ;

                _dataAccess.CommitTransaction();

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(plantDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<IEnumerable<Entities.UserGroupMember>> GetAllUserGroupMembersAsync()
        {
            return await _usergroupmemberRepositoryRepository.GetListAsync();
        }

        public async Task<IEnumerable<Entities.UserGroupMember>> GetAllUserGroupMembersAsync(int usergroupid, string userid)
        {
            return await _usergroupmemberRepositoryRepository.GetListAsync(userid, usergroupid);
        }

        public async Task<Entities.UserGroupMember> GetUserGroupMemberByIdAsync(string usergroupmemberid)
        {
            return await _usergroupmemberRepositoryRepository.GetAsync(usergroupmemberid);
        }

        public async Task<int> UpdateUserGroupMemberAsync(Entities.UserGroupMember usergroupmember)
        {
            var rowsaffected = await _usergroupmemberRepositoryRepository.UpdateAsync(usergroupmember);


            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(plantUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }






    }
}
