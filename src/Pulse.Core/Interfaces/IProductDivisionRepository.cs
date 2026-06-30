using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing productdivision data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProductDivisionRepository : IBaseRepository<ProductDivision, string>
    {
        Task<PagedResult<ProductDivisionWithStats>> GetPagedListAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        ProductDivision Get(string id);
    }
}
