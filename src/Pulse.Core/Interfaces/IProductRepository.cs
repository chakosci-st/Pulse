using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project-products data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProductRepository : IBaseRepository<Product, string>
    {
 
        Task<Product> GetDetailsFromIEDBByProductCodePlantCodeAsync(string productcode, string plantcode);

    }
}
