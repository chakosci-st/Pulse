using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class WorkItemPrerequisiteService : IWorkItemPrerequisiteService
    {
        private readonly IWorkItemPrerequisiteRepository _workitemprerequisiteRepository;

        public WorkItemPrerequisiteService(IWorkItemPrerequisiteRepository workitemprerequisiteRepository)
        {
            _workitemprerequisiteRepository = workitemprerequisiteRepository;
        }

        public async Task<IEnumerable<WorkItemPrerequisite>> GetAllPrerequisitesAsync(string workitemsysid)
        {
            return await _workitemprerequisiteRepository.GetListAsync(workitemsysid: workitemsysid);
        }

        public async Task<IEnumerable<WorkItemPrerequisite>> GetAllWorkItemsAsync(string prerequisiteworkitemsysid)
        {
            return await _workitemprerequisiteRepository.GetListAsync(prerequisiteworkitemsysid: prerequisiteworkitemsysid);
        }



        public async Task<WorkItemPrerequisite> GetByIdAsync(string workitemsysid)
        {
            return await _workitemprerequisiteRepository.GetAsync(workitemsysid);
        }


        public async Task<string> AddWorkItemPrerequisiteAsync(WorkItemPrerequisite workitem)
        {
            return await _workitemprerequisiteRepository.AddAsync(workitem);

            // Publish the ProductCreatedEvent
            //var plantCreatedEvent = new PlantCreatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};


        }

        public async Task<int> UpdateWorkItemPrerequisiteAsync(WorkItemPrerequisite workitem)
        {

            var rowsaffected = await _workitemprerequisiteRepository.UpdateAsync(workitem);

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





        public async Task<int> DeleteWorkItemPrerequisiteAsync(string workitemprerequisitesysid)
        {
 

            try
            {
                //GET INFO
                var obj = await _workitemprerequisiteRepository.GetAsync(workitemprerequisitesysid);


                //SET USER WHO DELETES THE WorkItemPrerequisite
                //obj.ModifiedBy = userid;
                await _workitemprerequisiteRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _workitemprerequisiteRepository.DeleteAsync(workitemprerequisitesysid);


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
    }
}
