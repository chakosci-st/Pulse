using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
/// <summary>
/// Interface for projectmember-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectMemberService
    {
        /// <summary>
        /// Adds a new projectmember to the system.
        /// </summary>
        /// <param name="projectmember">The projectmember to add.</param>
        Task<string> EnrollMemberAsync(Entities.ProjectMember projectmembers);

        /// <summary>
        /// Adds a new projectmember to the system.
        /// </summary>
        /// <param name="projectmembers">The projectmembers to add.</param>
        Task EnrollMembersAsync(IEnumerable<Entities.ProjectMember> projectmembers);


        /// <summary>
        /// Updates an existing projectmember in the system.
        /// </summary>
        /// <param name="projectmember">The projectmember to update.</param>
        Task<int> UpdateMemberAsync(Entities.ProjectMember projectmembers);

        /// <summary>
        /// Updates an existing projectmember in the system.
        /// </summary>
        /// <param name="projectmember">The projectmember to update.</param>
        Task UpdateMembersAsync(IEnumerable<Entities.ProjectMember> projectmembers);


        /// <summary>
        /// Deletes a projectmember from the system by its unique identifier.
        /// </summary>
        /// <param name="projectmemberno">The unique identifier of the projectmember to delete.</param>
        /// <param name="loggedprojectmember">The projectmember who deleted the projectmember.</param>
        Task<int> RemoveMemberAsync(string projectmemberno, string loggeduserid);

        /// <summary>
        /// Retrieves a projectmember by its unique identifier.
        /// </summary>
        /// <param name="projectmemberno">The unique identifier of the projectmember.</param>
        /// <returns>The projectmember with the specified USERID, or null if not found.</returns>
        Task<Entities.ProjectMember> GetProjectMemberByIdAsync(string id);



        /// <summary>
        /// Retrieves all projectmembers from the system.
        /// </summary>
        /// <returns>A list of all projectmembers.</returns>
        Task<IEnumerable<Entities.ProjectMember>> GetAllProjectMembersAsync();

        /// <summary>
        /// Retrieves all projectmembers from the system.
        /// </summary>
        /// <returns>A list of all projectmembers.</returns>
        Task<IEnumerable<Entities.ProjectMember>> GetAllProjectMembersAsync(string projectno);


    }
}
