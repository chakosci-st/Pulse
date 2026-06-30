using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;

        public ModuleService(IModuleRepository moduleRepositorysitory)
        {
            _moduleRepository = moduleRepositorysitory;
        }

        public async Task<IEnumerable<Module>> GetAllModulesAsync()
        {
            return await _moduleRepository.GetListAsync();
        }

        public async Task<Module> GetModuleByCodeAsync(string modulecode)
        {
            return await _moduleRepository.GetAsync(modulecode);
        }
        
        public async  Task<string> AddModuleAsync(Module module)
        {
            return await _moduleRepository.AddAsync(module);
        }

        public async Task<int> UpdateModuleAsync(Module module)
        {

            var rowsaffected = await _moduleRepository.UpdateAsync(module);

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

        public async Task<int> DeleteModuleAsync(string modulecode, string userid)
        {
    
            try
            {
                //GET PLANT INFO
                var obj = await _moduleRepository.GetAsync(modulecode); 


                //SET USER WHO DELETES THE Module
                obj.ModifiedBy = userid;
                await _moduleRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _moduleRepository.DeleteAsync(modulecode);


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

        public async Task<PagedResult<Module>> GetPagedModulesAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {

            return await _moduleRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
        public Module GetModuleByCode(string modulecode)
        {
            return _moduleRepository.Get(modulecode);
        }

 
    }
}
