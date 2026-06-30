using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant member link-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantMemberService
    {
        /// <summary>
        /// Retrieves all members per plant from the system.
        /// </summary>
        /// <param name="plantcode">plant</param>
        /// <returns>A list of all categories per plant.</returns>
        Task<IEnumerable<PlantMember>> GetAllMembersByPlantAsync(string plantcode);

        /// <summary>
        /// Retrieves all plants per user from the system.
        /// </summary>
        /// <param name="userid">userid</param>
        /// <returns>A list of all plants per user.</returns>
        Task<IEnumerable<PlantMember>> GetAllPlantsByUserIdAsync(string userid);

        /// <summary>
        /// Retrieves a plant - member by its unique identifier.
        /// </summary>
        /// <param name="plantmembersysid">The unique identifier of the member to retrieve.</param> 
        /// <returns>The plant - member with the specified key, or null if not found.</returns>
        Task<PlantMember> GetByIdAsync(string plantmembersysid);

        /// <summary>
        /// Adds a new plant - member link to the system.
        /// </summary>
        /// <param name="entity">The link to add.</param>
        Task<string> AddLinkAsync(PlantMember entity);

        /// <summary>
        /// Updates an existing plant - member in the system.
        /// </summary>
        /// <param name="entity">The plant - member to update.</param>
        Task<int> UpdateLinkAsync(PlantMember entity);
   
    }
}
