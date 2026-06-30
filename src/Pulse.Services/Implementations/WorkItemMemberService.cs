using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class WorkItemMemberService : IWorkItemMemberService
    {
        private readonly IWorkItemMemberRepository _workitemmemberRepository;

        public WorkItemMemberService(IWorkItemMemberRepository workitemmemberRepositorysitory)
        {
            _workitemmemberRepository = workitemmemberRepositorysitory;
        }

 

        public async Task<WorkItemMember> GetByIdAsync(string workitemmembersysid)
        {
            return await _workitemmemberRepository.GetAsync(workitemmembersysid);
        }


        public async Task<string> AddWorkItemMemberAsync(WorkItemMember workitemmember)
        {
            return await _workitemmemberRepository.AddAsync(workitemmember);

            // Publish the ProductCreatedEvent
            //var plantCreatedEvent = new PlantCreatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};

        }

        public async Task<int> UpdateWorkItemMemberAsync(WorkItemMember workitemmember)
        {

            var rowsaffected = await _workitemmemberRepository.UpdateAsync(workitemmember);

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

        public async Task<int> DeleteWorkItemMemberAsync(string workitemmembersysid, string userid)
        { 

            try
            {
                //GET INFO
                var obj = await _workitemmemberRepository.GetAsync(workitemmembersysid);


                //SET USER WHO DELETES THE WorkItemOwner
                obj.ModifiedBy = userid;
                await _workitemmemberRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _workitemmemberRepository.DeleteAsync(workitemmembersysid);


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

        public async Task<IEnumerable<WorkItemMember>> GetAllWorkItemMembersAsync()
        {
            return await _workitemmemberRepository.GetListAsync();
        }

        public async Task<IEnumerable<WorkItemMember>> GetAllWorkItemMembersAsync(string workitemsysid)
        {
            return await _workitemmemberRepository.GetListAsync(workitemsysid);
        }

        public async Task<WorkItemMember> GetWorkItemMemberByIdAsync(string workitemmemberid)
        {
            return await _workitemmemberRepository.GetAsync(workitemmemberid);
        }
    }
}
