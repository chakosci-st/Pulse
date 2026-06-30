using Pulse.Core.Entities;
using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for Roadmap-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IRoadmapService
    {
        /// <summary>
        /// Retrieves all roadmaps from the system.
        /// </summary>
        /// <returns>A list of all roadmaps.</returns>
        Task<IEnumerable<Roadmap>> GetAllRoadmapsAsync();

        /// <summary>
        /// Retrieves a roadmap by its unique identifier.
        /// </summary>
        /// <param name="roadmapsysid">The unique identifier of the roadmap.</param>
        /// <returns>The roadmap with the specified roadmapsysid, or null if not found.</returns>
        Task<Roadmap> GetRoadmapByIdAsync(string roadmapsysid);

        /// <summary>
        /// Retrieves a roadmap by its unique identifier.
        /// </summary>
        /// <param name="roadmapsysid">The unique identifier of the roadmap.</param>
        /// <returns>The roadmap with the specified roadmapsysid, or null if not found.</returns>
        Roadmap GetRoadmapById(string roadmapsysid);


        /// <summary>
        /// Retrieves a roadmap by its unique identifier.
        /// </summary>
        /// <param name="roadmapsysid">The unique identifier of the roadmap.</param>
        /// <returns>The roadmap with the specified roadmapsysid, or null if not found.</returns>
        Task<RoadmapExtended> GetCompleteInfoRoadmapByIdAsync(string roadmapsysid);


        /// <summary>
        /// Retrieves a roadmap by its unique identifier.
        /// </summary>
        /// <param name="roadmapsysid">The unique identifier of the roadmap.</param>
        /// <returns>The roadmap with the specified roadmapsysid, or null if not found.</returns>
        Task<dtoTreeResponse> GetTreeResponseAsync(string roadmapsysid);


        /// <summary>
        /// Retrieves all roadmap from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="isActive">The roadmap status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all productdivisions.</returns>
        Task<PagedResult<RoadmapExtended>> GetPagedRoadmapsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);





        /// <summary>
        /// Adds a new roadmap to the system.
        /// </summary>
        /// <param name="roadmap">The roadmap to add.</param>
        /// <returns>Rows affected.</returns>
        Task<string> BuildRoadmapAsync(Roadmap roadmap, string loggeduser);

        /// <summary>
        /// Updates an existing roadmap in the system.
        /// </summary>
        /// <param name="roadmap">The roadmap to update.</param> 
        System.Threading.Tasks.Task RebuildRoadmapAsync(Roadmap roadmap, string transactionkey, string loggeduser);


        /// <summary>
        /// Updates an existing roadmap (basic info only) in the system.
        /// </summary>
        /// <param name="roadmap">The roadmap to update.</param> 
        System.Threading.Tasks.Task UpdateAsync(Roadmap roadmap, string transactionkey, string loggeduser);


        /// <summary>
        /// Change status of an existing roadmap (basic info only) in the system.
        /// </summary>
        /// <param name="roadmap">The roadmap to update.</param> 
        System.Threading.Tasks.Task ChangeStatusAsync(Roadmap roadmap, string transactionkey, string loggeduser);

        /// <summary>
        /// Delete an existing roadmap (basic info only) in the system.
        /// </summary>
        /// <param name="roadmapsysid">The roadmap to delete.</param> 
        /// <param name="userid">The user deletes the roadmap.</param> 
        Task<int> DeleteRoadmapAsync(string roadmapsysid, string userid);


 
    }
}
