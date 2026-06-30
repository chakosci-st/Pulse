using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> GetDetailsFromIEDBByProductCodePlantCodeAsync(string productcode, string plantcode)
        {
            return await _productRepository.GetDetailsFromIEDBByProductCodePlantCodeAsync(productcode, plantcode);
        }
         
    }
}
