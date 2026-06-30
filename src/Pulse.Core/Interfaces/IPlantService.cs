using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantService
    {
        /// <summary>
        /// Retrieves all plants from the system.
        /// </summary>
        /// <returns>A list of all plants.</returns>
        Task<IEnumerable<Plant>> GetAllPlantsAsync();

        /// <summary>
        /// Retrieves all plants from the system.
        /// </summary>
        /// <returns>A list of all plants.</returns>
        Task<IEnumerable<Plant>> GetAllPlantsByUserAsync(string userid);


        /// <summary>
        /// Retrieves all plants from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The plant status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all plants.</returns>
        Task<PagedResult<PlantWithStats>> GetPagedPlantsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a plant by its unique identifier.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the plant.</param>
        /// <returns>The plant with the specified PLANTCODE, or null if not found.</returns>
        Plant GetPlantByCode(string plantcode);



        /// <summary>
        /// Retrieves a plant by its unique identifier.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the plant.</param>
        /// <returns>The plant with the specified PLANTCODE, or null if not found.</returns>
        Task<Plant> GetPlantByCodeAsync(string plantcode);

        /// <summary>
        /// Adds a new plant to the system.
        /// </summary>
        /// <param name="plant">The plant to add.</param>
        Task<string> AddPlantAsync(Plant plant);
        Task<string> AddPlantAsync(Plant plant, byte[] fileBytes, string fileName);

        /// <summary>
        /// Updates an existing plant in the system.
        /// </summary>
        /// <param name="plant">The plant to update.</param>
        Task<int> UpdatePlantAsync(Plant plant);

        Task<int> UpdatePlantAsync(Plant plant, byte[] fileBytes, string fileName);

        /// <summary>
        /// Deletes a plant from the system by its unique identifier.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the plant to delete.</param>
        /// <param name="userid">The user who deleted the plant.</param>
        Task<int> DeletePlantAsync(string plantcode, string userid);




        #region "ROADMAP LINK"
        /// <summary>
        /// Adds/set a new roadmap link to the system.
        /// </summary>
        /// <param name="link">The link to add.</param>
        Task<string> SelectRoadmapAsync(PlantRoadmapLink link);

        /// <summary>
        /// Updates existing roadmap link in the system.
        /// </summary>
        /// <param name="link">The link to update.</param>
        Task<int> UnselectRoadmapAsync(PlantRoadmapLink link);

        /// <summary>
        /// Retrieves roadmaps by its unique identifier.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the members.</param>
        /// <returns>The members with the specified PLANTCODE, or null if not found.</returns>  
        Task<IEnumerable<PlantRoadmapLinkExtended>> GetRoadmapListAsync(string plantcode = null, string roadmapsysid = null);

        #endregion



        #region "MEMBERS"
        /// <summary>
        /// Adds a new plant member to the system.
        /// </summary>
        /// <param name="plantmember">The plant member to add.</param>
        Task<string> AddMemberAsync(PlantMember plantmember);

        /// <summary>
        /// Updates existing plant member in the system.
        /// </summary>
        /// <param name="plantmember">The plant member to update.</param>
        Task<int> UpdateMemberAsync(PlantMember plantmember);

        /// <summary>
        /// Retrieves members by its unique identifier.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the members.</param>
        /// <returns>The members with the specified PLANTCODE, or null if not found.</returns>
        Task<IEnumerable<PlantMember>> GetMembersByCode(string plantcode);
        #endregion

    }
}
