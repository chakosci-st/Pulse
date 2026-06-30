using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for productdivision-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProductDivisionService
    {
        /// <summary>
        /// Retrieves all productdivisions from the system.
        /// </summary>
        /// <returns>A list of all productdivisions.</returns>
        Task<IEnumerable<ProductDivision>> GetAllProductDivisionsAsync();

        /// <summary>
        /// Retrieves a productdivision by its unique identifier.
        /// </summary>
        /// <param name="productdivisioncode">The unique identifier of the productdivision.</param>
        /// <returns>The productdivision with the specified PLANTCODE, or null if not found.</returns>
        Task<ProductDivision> GetProductDivisionByCodeAsync(string productdivisioncode);

        /// <summary>
        /// Adds a new productdivision to the system.
        /// </summary>
        /// <param name="productdivision">The productdivision to add.</param>
        Task<string> AddProductDivisionAsync(ProductDivision productdivision);

        /// <summary>
        /// Updates an existing productdivision in the system.
        /// </summary>
        /// <param name="productdivision">The productdivision to update.</param>
        Task<int> UpdateProductDivisionAsync(ProductDivision productdivision);

        /// <summary>
        /// Deletes a productdivision from the system by its unique identifier.
        /// </summary>
        /// <param name="productdivisioncode">The unique identifier of the product to delete.</param>
        /// <param name="userid">The user who deleted the productdivision.</param>
        Task<int> DeleteProductDivisionAsync(string productdivisioncode, string userid);

        /// <summary>
        /// Retrieves all productdivision from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The plant status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all productdivisions.</returns>
        Task<PagedResult<ProductDivisionWithStats>> GetPagedProductDivisionsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a productdivision by its unique identifier.
        /// </summary>
        /// <param name="productdivisioncode">The unique identifier of the productdivision.</param>
        /// <returns>The productdivision with the specified productdivisioncode, or null if not found.</returns>
        ProductDivision GetProductDivisionByCode(string productdivisioncode);
    }
}
