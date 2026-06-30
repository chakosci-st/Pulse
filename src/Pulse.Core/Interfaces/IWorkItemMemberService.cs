using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for WorkItemOwner-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IWorkItemMemberService
    {
        /// <summary>
        /// Retrieves all workitemmembers from the system.
        /// </summary>
        /// <returns>A list of all workitemmembers.</returns>
        Task<IEnumerable<WorkItemMember>> GetAllWorkItemMembersAsync();

        /// <summary>
        /// Retrieves all workitemmembers from the system.
        /// </summary>
        /// <returns>A list of all workitemmembers.</returns>
        Task<IEnumerable<WorkItemMember>> GetAllWorkItemMembersAsync(string workitemsysid);


        /// <summary>
        /// Retrieves a workitemmember by its unique identifier.
        /// </summary>
        /// <param name="workitemmemberid">The unique identifier of the workitemmember.</param>
        /// <returns>The workitemmember with the specified USERGROUPCODE, or null if not found.</returns>
        Task<WorkItemMember> GetWorkItemMemberByIdAsync(string workitemmemberid);

        /// <summary>
        /// Adds a new workitemmember to the system.
        /// </summary>
        /// <param name="workitemmember">The workitemmember to add.</param>
        /// <returns>Rows affected.</returns>
        Task<string> AddWorkItemMemberAsync(WorkItemMember workitemmember);

        /// <summary>
        /// Updates an existing workitemmember in the system.
        /// </summary>
        /// <param name="workitemmember">The workitemmember to update.</param>
        /// <returns>Rows affected.</returns>
        Task<int> UpdateWorkItemMemberAsync(WorkItemMember workitemmember);

        /// <summary>
        /// Deletes a workitemmember from the system by its unique identifier.
        /// </summary>
        /// <param name="workitemmemberid">The unique identifier of the user to delete.</param>
        /// <param name="userid">The user who deleted the workitemmember.</param>
        /// <returns>Rows affected.</returns>
        Task<int> DeleteWorkItemMemberAsync(string workitemmemberid, string userid);
    }
}
