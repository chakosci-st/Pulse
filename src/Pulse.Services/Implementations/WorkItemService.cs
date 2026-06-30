using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class WorkItemService : IWorkItemService
    {
        private readonly IWorkItemRepository _workitemRepository;

        public WorkItemService(IWorkItemRepository workitemRepositorysitory)
        {
            _workitemRepository = workitemRepositorysitory;
        }


        public async Task<IEnumerable<WorkItem>> GetWorkItemListAsync(string plantcode, string categorycode, string maturitycode = null)
        {
            return await _workitemRepository.GetListAsync(plantcode, categorycode, maturitycode);
        }

        public async Task<WorkItem> GetByIdAsync(string workitemsysid)
        {
            return await _workitemRepository.GetAsync(workitemsysid);
        }


        public async Task<string> AddWorkItemAsync(WorkItem workitem)
        {
            return await _workitemRepository.AddAsync(workitem);

            // Publish the ProductCreatedEvent
            //var plantCreatedEvent = new PlantCreatedEvent
            //{
            //    PlantCode = plant.PlantCode,
            //    ActionBy = plant.CreatedBy
            //};

        }

        public async Task<int> UpdateWorkItemAsync(WorkItem workitem)
        {

            var rowsaffected = await _workitemRepository.UpdateAsync(workitem);

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

        public async Task<int> DeleteWorkItemAsync(string workitemsysid, string userid)
        { 

            try
            {
                //GET INFO
                var obj = await _workitemRepository.GetAsync(workitemsysid);


                //SET USER WHO DELETES THE WorkItem
                obj.ModifiedBy = userid;
                await _workitemRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _workitemRepository.DeleteAsync(workitemsysid);


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
