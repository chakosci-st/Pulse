using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class MaturityLevelService : IMaturityLevelService
    {
        private readonly IMaturityLevelRepository _maturitylevelRepository;

        public MaturityLevelService(IMaturityLevelRepository maturitylevelRepositorysitory)
        {
            _maturitylevelRepository = maturitylevelRepositorysitory;
        }

        public async Task<string> AddMaturityLevelAsync(MaturityLevel maturitylevel)
        {
          return await _maturitylevelRepository.AddAsync(maturitylevel);
        }

        public async Task<int> DeleteMaturityLevelAsync(string maturitycode, string userid)
        {
 

            try
            {
                //GET INFO
                var obj = await _maturitylevelRepository.GetAsync(maturitycode);


                //SET USER WHO DELETES THE MaturityLevel
                obj.ModifiedBy = userid;
                await _maturitylevelRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _maturitylevelRepository.DeleteAsync(maturitycode);


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

        public async Task<IEnumerable<MaturityLevel>> GetAllMaturityLevelsAsync()
        {
            return await _maturitylevelRepository.GetListAsync();
        }

        public async Task<MaturityLevel> GetMaturityLevelByCodeAsync(string maturitycode)
        {
            return await _maturitylevelRepository.GetAsync(maturitycode);
        }

        public async Task<int> UpdateMaturityLevelAsync(MaturityLevel maturitylevel)
        {
            var rowsaffected = await _maturitylevelRepository.UpdateAsync(maturitylevel);

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

        public MaturityLevel GetMaturityByCode(string maturitycode)
        {
            return _maturitylevelRepository.Get(maturitycode);
        }
        public async Task<PagedResult<MaturityLevelWithStats>> GetPagedMaturityLevelsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _maturitylevelRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
    }
}
