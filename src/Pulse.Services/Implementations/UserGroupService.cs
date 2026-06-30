using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class UserGroupService : IUserGroupService
    {
        private readonly IUserGroupRepository _usergroupRepository;
        private readonly IUserGroupAccessRightRepository _usergroupaccessrightRepository;
        public UserGroupService(IUserGroupRepository usergroupRepositorysitory, IUserGroupAccessRightRepository usergroupaccessrightRepository)
        {
            _usergroupRepository = usergroupRepositorysitory;
            _usergroupaccessrightRepository = usergroupaccessrightRepository;
        }

        public async Task<IEnumerable<UserGroup>> GetAllUserGroupsAsync()
        {
            return await _usergroupRepository.GetListAsync();
        }

        public async Task<UserGroup> GetUserGroupByIdAsync(int usergroupid)
        {
            return await _usergroupRepository.GetAsync(usergroupid);
        }
        public async Task<PagedResult<UserGroup>> GetPagedUserGroupsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {

            return await _usergroupRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
        public UserGroup GetByUserGroupId(int usergroupid)
        {
            return _usergroupRepository.Get(usergroupid);
        }
        public async Task<int> AddUserGroupAsync(UserGroup usergroup)
        {
            return await _usergroupRepository.AddAsync(usergroup);

        }

        public async Task<int> UpdateUserGroupAsync(UserGroup usergroup)
        {

            var rowsaffected = await _usergroupRepository.UpdateAsync(usergroup);

            // Publish the UserUpdatedEvent
            //var usergroupUpdatedEvent = new UserGroupUpdatedEvent
            //{
            //    UserGroupCode = usergroup.UserGroupCode,
            //    ActionBy = usergroup.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(usergroupUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public async Task<int> DeleteUserGroupAsync(int usergroupid, string userid)
        {


            try
            {
                //GET PLANT INFO
                var obj = await _usergroupRepository.GetAsync(usergroupid);


                //SET USER WHO DELETES THE UserGroup
                obj.ModifiedBy = userid;
                await _usergroupRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _usergroupRepository.DeleteAsync(usergroupid);


                // Publish the UserUpdatedEvent
                //var usergroupDeletedEvent = new UserGroupDeletedEvent
                //{
                //    UserGroupCode = usergroup.UserGroupCode,
                //    ActionBy = usergroup.CreatedBy
                //};



                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(usergroupDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<IEnumerable<UserGroupModule>> GetModulesAsync(int usergroupid)
        {
            return await _usergroupaccessrightRepository.GetModulesAsync(usergroupid);
        }

        public async Task<UserGroupModule> GetModuleAsync(int usergroupid, string modulecode)
        {
            return AutoMapper.Mapper.Map<UserGroupModule>(await _usergroupaccessrightRepository.GetAsync(usergroupid, modulecode));
        }


        public async Task AuthorizeToModule(UserGroupModule module)
        {
            await _usergroupaccessrightRepository.AddAsync(AutoMapper.Mapper.Map<UserGroupAccessRight>(module));
        }
        public async Task RestrictToModule(UserGroupModule module)
        {
            await _usergroupaccessrightRepository.DeleteAsync(module.Id);

        }
    }
}
