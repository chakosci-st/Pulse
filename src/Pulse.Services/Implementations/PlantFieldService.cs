using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class PlantFieldService : IPlantFieldService
    {
        private readonly IPlantFieldRepository _plantfieldRepository;

        public PlantFieldService(IPlantFieldRepository plantfieldRepository)
        {
            _plantfieldRepository = plantfieldRepository;
        }

        public async Task<string> AddLinkAsync(PlantField entity)
        {
            return await _plantfieldRepository.AddAsync(entity);
        }

        public async Task<int> UpdateLinkAsync(PlantField entity)
        {
            var rowsaffected = await _plantfieldRepository.UpdateAsync(entity);

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

        public async Task<int> DeleteLinkAsync(string plantfieldsysid, string loggeduserid)
        {
        
            try
            {
                //GET PLANT INFO
                var obj = await _plantfieldRepository.GetAsync(plantfieldsysid);


                //SET USER WHO DELETES THE Plant
                obj.ModifiedBy = loggeduserid;
                await _plantfieldRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _plantfieldRepository.DeleteAsync(plantfieldsysid);


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





        public async Task<PlantField> GetByIdAsync(string plantfieldsysid)
        {
            return await _plantfieldRepository.GetAsync(plantfieldsysid);
        }

        public async Task<IEnumerable<PlantField>> GetAllCategoriesByPlantAsync(string plantcode)
        {
            return await _plantfieldRepository.GetListAsync(plantcode: plantcode);
        }

        public async Task<IEnumerable<PlantField>> GetAllPlantsByFieldAsync(int fieldid)
        {
            return await _plantfieldRepository.GetListAsync(fieldid: fieldid);
        }

         
    }
}
