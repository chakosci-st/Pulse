using Pulse.Core.Entities; 
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class ProductDivisionService : IProductDivisionService
    {
        private readonly IProductDivisionRepository _productdivisionRepository;

        public ProductDivisionService(IProductDivisionRepository productdivisionRepositorysitory)
        {
            _productdivisionRepository = productdivisionRepositorysitory;
        }

        public async Task<string> AddProductDivisionAsync(ProductDivision productdivision)
        {
            return await _productdivisionRepository.AddAsync(productdivision);
        }

        public async Task<int> DeleteProductDivisionAsync(string productdivisioncode, string userid)
        {
         

            try
            {
                //GET PLANT INFO
                var obj = await _productdivisionRepository.GetAsync(productdivisioncode);


                //SET USER WHO DELETES THE ProductDivision
                obj.ModifiedBy = userid;
                await _productdivisionRepository.UpdateAsync(obj);

                //DELETE RECORD
                var rowsaffected = await _productdivisionRepository.DeleteAsync(productdivisioncode);


                // Publish the ProductUpdatedEvent
                //var productdivisionDeletedEvent = new ProductDivisionDeletedEvent
                //{
                //    ProductDivisionCode = productdivision.ProductDivisionCode,
                //    ActionBy = productdivision.CreatedBy
                //};
 

                if (rowsaffected > 0)
                {
                    //_eventPublisher.Publish(productdivisionDeletedEvent);
                    return rowsaffected;
                }
            }
            catch (Exception ex)
            {
      
                throw new Exception(ex.Message);

            }



            return 0;
        }

        public async Task<IEnumerable<ProductDivision>> GetAllProductDivisionsAsync()
        {
            return await _productdivisionRepository.GetListAsync();
        }

        public async Task<ProductDivision> GetProductDivisionByCodeAsync(string productdivisioncode)
        {
            return await _productdivisionRepository.GetAsync(productdivisioncode);
        }

        public async Task<int> UpdateProductDivisionAsync(ProductDivision productdivision)
        {
            var rowsaffected = await _productdivisionRepository.UpdateAsync(productdivision);

            // Publish the ProductUpdatedEvent
            //var productdivisionUpdatedEvent = new ProductDivisionUpdatedEvent
            //{
            //    ProductDivisionCode = productdivision.ProductDivisionCode,
            //    ActionBy = productdivision.CreatedBy
            //};

            if (rowsaffected > 0)
            {
                //_eventPublisher.Publish(productdivisionUpdatedEvent);
                return rowsaffected;
            }

            return 0;
        }

        public ProductDivision GetProductDivisionByCode(string productdivisioncode)
        {
            return _productdivisionRepository.Get(productdivisioncode);
        }
        public async Task<PagedResult<ProductDivisionWithStats>> GetPagedProductDivisionsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize)
        {
            return await _productdivisionRepository.GetPagedListAsync(searchValue, sortBy, sortDirection, isActive, pageNumber, pageSize);
        }
    }
}
