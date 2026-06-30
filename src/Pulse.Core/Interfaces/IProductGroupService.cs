using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for productgroup-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProductGroupService
    {
        /// <summary>
        /// Retrieves all productgroups from the system.
        /// </summary>
        /// <returns>A list of all productgroups.</returns>
        Task<IEnumerable<ProductGroup>> GetAllProductGroupsAsync();

        /// <summary>
        /// Retrieves a productgroup by its unique identifier.
        /// </summary>
        /// <param name="productgroupcode">The unique identifier of the productgroup.</param>
        /// <returns>The productgroup with the specified PLANTCODE, or null if not found.</returns>
        Task<ProductGroup> GetProductGroupByCodeAsync(string productgroupcode);

        /// <summary>
        /// Adds a new productgroup to the system.
        /// </summary>
        /// <param name="productgroup">The productgroup to add.</param>
        /// <returns>Rows affected.</returns>
        Task<string> AddProductGroupAsync(ProductGroup productgroup);

        /// <summary>
        /// Updates an existing productgroup in the system.
        /// </summary>
        /// <param name="productgroup">The productgroup to update.</param>
        /// <returns>Rows affected.</returns>
        Task<int> UpdateProductGroupAsync(ProductGroup productgroup);

        /// <summary>
        /// Deletes a productgroup from the system by its unique identifier.
        /// </summary>
        /// <param name="productgroupcode">The unique identifier of the product to delete.</param>
        /// <param name="userid">The user who deleted the productgroup.</param>
        /// <returns>Rows affected.</returns>
        Task<int> DeleteProductGroupAsync(string productgroupcode, string userid);


        /// <summary>
        /// Retrieves all productgroup from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The plant status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all productgroups.</returns>
        Task<PagedResult<ProductGroupWithStats>> GetPagedProductGroupsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a productgroup by its unique identifier.
        /// </summary>
        /// <param name="productgroupcode">The unique identifier of the productgroup.</param>
        /// <returns>The productgroup with the specified productgroupcode, or null if not found.</returns>
        ProductGroup GetProductGroupByCode(string productgroupcode);
    }
}
