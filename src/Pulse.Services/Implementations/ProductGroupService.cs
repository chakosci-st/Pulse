using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class ProductGroupService : IProductGroupService
    {
        private readonly IProductGroupRepository _productgroupRepository;

        public ProductGroupService(IProductGroupRepository productgroupRepositorysitory)
        {
            _productgroupRepository = productgroupRepositorysitory;
        }

        public async Task<IEnumerable<ProductGroup>> GetAllProductGroupsAsync()
        {
            return await _productgroupRepository.GetListAsync();
        }

        public async Task<ProductGroup> GetProductGroupByCodeAsync(string productgroupcode)
        {
            return await _productgroupRepository.GetAsync(productgroupcode);
        }
        
        public async Task<string> AddProductGroupAsync(ProductGroup productgroup)
        {
 

            return await _productgroupRepository.AddAsync(productgroup);
        }

        public async Task<int> UpdateProductGroupAsync(ProductGroup productgroup)
        {

            var rowsaffected = await _productgroupRepository.UpdateAsync(productgroup);

            // Publish the ProductUpdatedEvent
            //var productgroupUpdatedEvent = new ProductGroupUpdatedEvent
            //{
            //    ProductGroupCode = productgroup.ProductGroupCode,
            //    ActionBy = productgroup.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(productgroupUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public async Task<int> DeleteProductGroupAsync(string productgroupcode, string userid)
        {
          
            try
            {
                //GET PLANT INFO
                var obj = await _productgroupRepository.GetAsync(productgroupcode); 


                //SET USER WHO DELETES THE ProductGroup
                obj.ModifiedBy = userid;
                await _productgroupRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _productgroupRepository.DeleteAsync(productgroupcode);


                // Publish the ProductUpdatedEvent
                //var productgroupDeletedEvent = new ProductGroupDeletedEvent
                //{
                //    ProductGroupCode = productgroup.ProductGroupCode,
                //    ActionBy = productgroup.CreatedBy
                //};
 

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(productgroupDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            { 
                throw new Exception(ex.Message);

            }



            return 0;
        }


        public ProductGroup GetProductGroupByCode(string productgroupcode)
        {
            return _productgroupRepository.Get(productgroupcode);
        }
        public async Task<PagedResult<ProductGroupWithStats>> GetPagedProductGroupsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _productgroupRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
    }
}
