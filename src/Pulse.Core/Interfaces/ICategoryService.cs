using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for category-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories from the system.
        /// </summary>
        /// <returns>A list of all categories.</returns>
       Task< IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Retrieves all categories from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="sortBy">sortBy</param>
        /// <param name="sortDirection">sortDirection</param>
        /// <param name="isActive">The category status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all categories.</returns>
        Task<PagedResult<CategoryWithStats>> GetPagedCategoriesAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);


        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="categorycode">The unique identifier of the category.</param>
        /// <returns>The category with the specified PLANTCODE, or null if not found.</returns>
        Task<Category> GetCategoryByCodeAsync(string categorycode);

        /// <summary>
        /// Adds a new category to the system.
        /// </summary>
        /// <param name="category">The category to add.</param>
       Task<string> AddCategoryAsync(Category category);

        /// <summary>
        /// Updates an existing category in the system.
        /// </summary>
        /// <param name="category">The category to update.</param>
       Task<int> UpdateCategoryAsync(Category category);

        /// <summary>
        /// Deletes a category from the system by its unique identifier.
        /// </summary>
        /// <param name="categorycode">The unique identifier of the product to delete.</param>
        /// <param name="userid">The user who deleted the category.</param>
       Task<int> DeleteCategoryAsync(string categorycode, string userid);
    }
}
