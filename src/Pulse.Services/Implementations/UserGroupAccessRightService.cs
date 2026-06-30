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
    public class UserGroupAccessRightService : IUserGroupAccessRightService
    {
        private readonly OracleDataAccessLayer _dataAccess;
        private readonly IUserGroupAccessRightRepository _usergroupaccessrightRepositoryRepository;

        public UserGroupAccessRightService(OracleDataAccessLayer dataAccess, IUserGroupAccessRightRepository usergroupaccessrightRepositoryRepository)
        {
            _dataAccess = dataAccess;
            _usergroupaccessrightRepositoryRepository = usergroupaccessrightRepositoryRepository;
        }

        public async Task<string> AddAccessRightAsync(Entities.UserGroupAccessRight useraccess)
        {
            var returnvalue = await _usergroupaccessrightRepositoryRepository.AddAsync(useraccess);

            // Publish the ProductCreatedEvent
            //var plantCreatedEvent = new PlantCreatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};



            return returnvalue;
        }

        public async Task<int> DeleteAccessRightAsync(string usergroupaccessrightsysid, string loggeduserid)
        {
            _dataAccess.BeginTransaction();

            try
            {
                //GET ACCESS INFO
                var obj = await _usergroupaccessrightRepositoryRepository.GetAsync(usergroupaccessrightsysid);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = loggeduserid;
                await _usergroupaccessrightRepositoryRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _usergroupaccessrightRepositoryRepository.DeleteAsync(usergroupaccessrightsysid);


                // Publish the ProductUpdatedEvent
                //var plantDeletedEvent = new PlantDeletedEvent
                //{
                //    PlantCode = plant.PlantCode,
                //    ActionBy = plant.CreatedBy
                //};

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

        public async Task<Entities.UserGroupAccessRight> GetAccessRightByIdAsync(string usergroupaccessrightsysid)
        {
            return await _usergroupaccessrightRepositoryRepository.GetAsync(usergroupaccessrightsysid);
        }

        public async Task<IEnumerable<Entities.UserGroupAccessRight>> GetAllAccessRightsAsync(int? usergroupid = null, string modulecode = null)
        {
            return await _usergroupaccessrightRepositoryRepository.GetListAsync(usergroupid, modulecode);
        }



        public async Task<int> UpdateAccessRightAsync(Entities.UserGroupAccessRight useraccess)
        {
            var rowsaffected = await _usergroupaccessrightRepositoryRepository.UpdateAsync(useraccess);

            // Publish the ProductUpdatedEvent
            //var plantUpdatedEvent = new PlantUpdatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(plantUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }
    }
}
