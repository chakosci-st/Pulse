using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class PlantUserGroupMemberService : IPlantUserGroupMemberService
    {
        private readonly IPlantUserGroupMemberRepository _plantusergroupmemberRepositoryRepository;

        public PlantUserGroupMemberService(IPlantUserGroupMemberRepository plantusergroupmemberRepositoryRepository)
        {
            _plantusergroupmemberRepositoryRepository = plantusergroupmemberRepositoryRepository;
        }

        public async Task<string> AddMemberAsync(PlantUserGroupMember member)
        {
           return await _plantusergroupmemberRepositoryRepository.AddAsync(member);
             
        }

        public async Task<int> UpdateMemberAsync(PlantUserGroupMember member)
        {
            var rowsaffected = await _plantusergroupmemberRepositoryRepository.UpdateAsync(member);

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

        public async Task<int> DeleteMemberAsync(string plantusergroupmembersysid, string loggeduserid)
        {
        
            try
            {
                //GET PLANT INFO
                var obj = await _plantusergroupmemberRepositoryRepository.GetAsync(plantusergroupmembersysid);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = loggeduserid;
                await _plantusergroupmemberRepositoryRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _plantusergroupmemberRepositoryRepository.DeleteAsync(plantusergroupmembersysid);


                // Publish the ProductUpdatedEvent
                //var plantDeletedEvent = new PlantDeletedEvent
                //{
                //    PlantCode = plant.PlantCode,
                //    ActionBy = plant.CreatedBy
                //};
                 

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(plantDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            { 
                throw new Exception(ex.Message);

            }



            return 0;
        }
 

        public async Task<PlantUserGroupMember> GetMemberByIdAsync(string plantusergroupmembersysid)
        {
            return await _plantusergroupmemberRepositoryRepository.GetAsync(plantusergroupmembersysid);
        }

        public async Task<IEnumerable<PlantUserGroupMember>> GetAlMembersByPlantUserGroupAsync(string plantcode, int usergroupid)
        {
            return await _plantusergroupmemberRepositoryRepository.GetListAsync(plantcode, usergroupid);
        }
    }
}
