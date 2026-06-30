using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for plant usergroup member-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IPlantUserGroupMemberService
    {
        /// <summary>
        /// Retrieves all plant-usergroup members from the system.
        /// </summary>
        /// <returns>A list of all plant-usergroup members.</returns>
        Task<IEnumerable<PlantUserGroupMember>> GetAlMembersByPlantUserGroupAsync(string plantcode, int usergroupid);

        /// <summary>
        /// Retrieves a plant by its unique identifier.
        /// </summary>
        /// <param name="plantusergroupmembersysid">The unique identifier of the member.</param> 
        /// <returns>The plant with the specified PLANTCODE, or null if not found.</returns>
        Task<PlantUserGroupMember> GetMemberByIdAsync(string plantusergroupmembersysid);

        /// <summary>
        /// Adds a new plant to the system.
        /// </summary>
        /// <param name="member">The member to add.</param>
        /// <returns>Primary Key.</returns>
        Task<string> AddMemberAsync(PlantUserGroupMember member);

        /// <summary>
        /// Updates an existing plant in the system.
        /// </summary>
        /// <param name="member">The member to update.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int> UpdateMemberAsync(PlantUserGroupMember member);

        /// <summary>
        /// Deletes a plant from the system by its unique identifier.
        /// </summary>
        /// <param name="plantusergroupmembersysid">The unique identifier of the member to delete.</param>
        /// <param name="loggeduserid">The user who deleted the member.</param>
        /// <returns>Number of rows affected.</returns>
        Task<int>DeleteMemberAsync(string plantusergroupmembersysid, string loggeduserid);

         
    }
}
