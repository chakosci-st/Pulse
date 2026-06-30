using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepositorysitory)
        {
            _categoryRepository = categoryRepositorysitory;
        }

        public async Task<string> AddCategoryAsync(Category category)
        {
            return await _categoryRepository.AddAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(string categorycode, string userid)
        {
 
            try
            {
                //GET INFO
                var obj = await _categoryRepository.GetAsync(categorycode);


                //SET USER WHO DELETES THE Category
                obj.ModifiedBy = userid;
                await _categoryRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _categoryRepository.DeleteAsync(categorycode);


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

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetListAsync();
        }

        public async Task<PagedResult<CategoryWithStats>> GetPagedCategoriesAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {

            return await _categoryRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
        public Category GetCategoryByCode(string categorycode)
        {
            return _categoryRepository.Get(categorycode);
        }
        public async Task<Category> GetCategoryByCodeAsync(string categorycode)
        {
            return await _categoryRepository.GetAsync(categorycode);
        }

        public async Task<int> UpdateCategoryAsync(Category category)
        {
            var rowsaffected = await _categoryRepository.UpdateAsync(category);

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
