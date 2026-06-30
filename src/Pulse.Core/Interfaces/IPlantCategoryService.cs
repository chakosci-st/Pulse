using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant category link-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantCategoryService
    {
        /// <summary>
        /// Retrieves all categories per plant from the system.
        /// </summary>
        /// <param name="plantcode">plant</param>
        /// <returns>A list of all categories per plant.</returns>
        Task<IEnumerable<PlantCategory>> GetAllCategoriesByPlantAsync(string plantcode);

        /// <summary>
        /// Retrieves all plants per category from the system.
        /// </summary>
        /// <param name="categorycode">Category code</param>
        /// <returns>A list of all plants per category.</returns>
        Task<IEnumerable<PlantCategory>> GetAllPlantsByCategoryAsync(string categorycode);

        /// <summary>
        /// Retrieves a plant - category by its unique identifier.
        /// </summary>
        /// <param name="plantcategorysysid">The unique identifier of the member to retrieve.</param> 
        /// <returns>The plant - category with the specified PLANTCODE and category, or null if not found.</returns>
        Task<PlantCategory> GetByIdAsync(string plantcategorysysid);

        /// <summary>
        /// Adds a new plant - category link to the system.
        /// </summary>
        /// <param name="entity">The link to add.</param>
        Task<string> AddLinkAsync(PlantCategory entity);

        /// <summary>
        /// Updates an existing plant - category in the system.
        /// </summary>
        /// <param name="entity">The plant - category to update.</param>
        Task<int> UpdateLinkAsync(PlantCategory entity);

        /// <summary>
        /// Deletes a plant-category link from the system by its unique identifier.
        /// </summary>
        /// <param name="plantcategorysysid">The unique identifier of the member to delete.</param> 
        /// <param name="loggeduserid">The user who deleted the member.</param>
        Task<int>DeleteLinkAsync(string plantcategorysysid, string loggeduserid);

         
    }
}
