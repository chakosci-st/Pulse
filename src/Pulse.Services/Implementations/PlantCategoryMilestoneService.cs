using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class PlantCategoryMilestoneService : IPlantCategoryMilestoneService
    {
        private readonly IPlantCategoryMilestoneRepository _plantcategorymilestoneRepository;

        public PlantCategoryMilestoneService(IPlantCategoryMilestoneRepository plantcategorymilestoneRepository)
        {
            _plantcategorymilestoneRepository = plantcategorymilestoneRepository;
        }

        public async Task<string> AddMilestoneAsync(PlantCategoryMilestone milestone)
        {
           return await _plantcategorymilestoneRepository.AddAsync(milestone);
        }

        public async Task<int> UpdateMilestoneAsync(PlantCategoryMilestone milestone)
        {
            var rowsaffected = await _plantcategorymilestoneRepository.UpdateAsync(milestone);

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

        public async Task<int> DeleteMilestoneAsync(string plantcategorymilestonesysid, string loggeduserid)
        {
       
            try
            {
                //GET PLANT INFO
                var obj = await _plantcategorymilestoneRepository.GetAsync(plantcategorymilestonesysid);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = loggeduserid;
                await _plantcategorymilestoneRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _plantcategorymilestoneRepository.DeleteAsync(plantcategorymilestonesysid);


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

 

        public async Task<PlantCategoryMilestone> GetByIdAsync(string plantcategorymilestonesysid)
        {
            return await _plantcategorymilestoneRepository.GetAsync(plantcategorymilestonesysid);
        }

        public async Task<IEnumerable<PlantCategoryMilestone>> GetAllMilestonesByPlantCategoryAsync(string plantcode, string categorycode)
        {
            return await _plantcategorymilestoneRepository.GetListAsync(plantcode, categorycode);
        }

        public async Task<IEnumerable<PlantCategoryMilestone>> GetAllMilestonesByPlantCategorySysIdAsync(string plantcategorysysid)
        {
            return await _plantcategorymilestoneRepository.GetAllAsync(plantcategorysysid);
        }

  
    }
}
