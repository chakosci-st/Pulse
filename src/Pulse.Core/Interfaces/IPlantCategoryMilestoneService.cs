using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant category milestone-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantCategoryMilestoneService
    {
        /// <summary>
        /// Retrieves all milestones per plant - category from the system.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param>
        /// <param name="categorycode">The unique identifier of the member to retrieve.</param>
        /// <returns>A list of all milestones per plant - category.</returns>
        Task<IEnumerable<PlantCategoryMilestone>> GetAllMilestonesByPlantCategoryAsync(string plantcode, string categorycode);

        /// <summary>
        /// Retrieves all milestones per plant - category from the system.
        /// </summary>
        /// <param name="plantcategorysysid">The unique identifier of the member to retrieve.</param>
        /// <returns>A list of all milestones per plant - category.</returns>
        Task<IEnumerable<PlantCategoryMilestone>> GetAllMilestonesByPlantCategorySysIdAsync(string plantcategorysysid);

        /// <summary>
        /// Retrieves a milestone by its unique identifier.
        /// </summary>
        /// <param name="plantcategorymilestonesysid">The unique identifier of the member to retrieve.</param> 
        /// <returns>The milestone with the specified PLANTCODE, categorycode and milestonecode, or null if not found.</returns>
        Task<PlantCategoryMilestone> GetByIdAsync(string plantcategorymilestonesysid);

        /// <summary>
        /// Adds a new plant - category milestone to the system.
        /// </summary>
        /// <param name="milestone">The milestone to add.</param>
        Task<string> AddMilestoneAsync(PlantCategoryMilestone milestone);

        /// <summary>
        /// Updates an existing plant - category in the system.
        /// </summary>
        /// <param name="milestone">The plant - category milestone to update.</param>
        Task<int> UpdateMilestoneAsync(PlantCategoryMilestone milestone);

        /// <summary>
        /// Deletes a plant-category milestone from the system by its unique identifier.
        /// </summary>
        /// <param name="plantcategorymilestonesysid">The unique identifier of the member to delete.</param> 
        /// <param name="loggeduserid">The user who deleted the member.</param>
        Task<int>DeleteMilestoneAsync(string plantcategorymilestonesysid, string loggeduserid);

         
    }
}
