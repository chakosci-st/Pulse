using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class PlantCategoryService : IPlantCategoryService
    {
        private readonly IPlantCategoryRepository _plantcategoryRepository;

        public PlantCategoryService(IPlantCategoryRepository plantcategoryRepository)
        {
            _plantcategoryRepository = plantcategoryRepository;
        }

        public async Task<string> AddLinkAsync(PlantCategory entity)
        {
            return await _plantcategoryRepository.AddAsync(entity);
        }

        public async Task<int> UpdateLinkAsync(PlantCategory entity)
        {
            var rowsaffected = await _plantcategoryRepository.UpdateAsync(entity);

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

        public async Task<int> DeleteLinkAsync(string plantcategorysysid, string loggeduserid)
        {
          

            try
            {
                //GET PLANT INFO
                var obj = await _plantcategoryRepository.GetAsync(plantcategorysysid);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = loggeduserid;
                await _plantcategoryRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _plantcategoryRepository.DeleteAsync(plantcategorysysid);


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





        public async Task<PlantCategory> GetByIdAsync(string plantcategorysysid)
        {
            return await _plantcategoryRepository.GetAsync(plantcategorysysid);
        }

        public async Task<IEnumerable<PlantCategory>> GetAllCategoriesByPlantAsync(string plantcode)
        {
            return await _plantcategoryRepository.GetListAsync(plantcode: plantcode);
        }

        public async Task<IEnumerable<PlantCategory>> GetAllPlantsByCategoryAsync(string categorycode)
        {
            return await _plantcategoryRepository.GetListAsync(categorycode: categorycode);
        }

         
    }
}
