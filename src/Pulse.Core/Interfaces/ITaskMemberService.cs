using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
/// <summary>
/// Interface for taskmember-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ITaskMemberService
    {
        /// <summary>
        /// Adds a new taskmember to the system.
        /// </summary>
        /// <param name="taskmember">The taskmember to add.</param>
        Task<string> EnrollMemberAsync(Entities.ProjectMember taskmembers);

        /// <summary>
        /// Adds a new taskmember to the system.
        /// </summary>
        /// <param name="taskmembers">The taskmembers to add.</param>
        Task EnrollMembersAsync(IEnumerable<Entities.ProjectMember> taskmembers);


        /// <summary>
        /// Updates an existing taskmember in the system.
        /// </summary>
        /// <param name="taskmember">The taskmember to update.</param>
        Task<int> UpdateMemberAsync(Entities.ProjectMember taskmembers);

        /// <summary>
        /// Updates an existing taskmember in the system.
        /// </summary>
        /// <param name="taskmember">The taskmember to update.</param>
        Task UpdateMembersAsync(IEnumerable<Entities.ProjectMember> taskmembers);


        /// <summary>
        /// Deletes a taskmember from the system by its unique identifier.
        /// </summary>
        /// <param name="taskmemberno">The unique identifier of the taskmember to delete.</param>
        /// <param name="loggedtaskmember">The taskmember who deleted the taskmember.</param>
        Task<int> RemoveMemberAsync(string taskmemberno, string loggeduserid);

        /// <summary>
        /// Retrieves a taskmember by its unique identifier.
        /// </summary>
        /// <param name="taskmemberno">The unique identifier of the taskmember.</param>
        /// <returns>The taskmember with the specified USERID, or null if not found.</returns>
        Task<Entities.ProjectMember> GetProjectMemberByIdAsync(string id);



        /// <summary>
        /// Retrieves all taskmembers from the system.
        /// </summary>
        /// <returns>A list of all taskmembers.</returns>
        Task<IEnumerable<Entities.ProjectMember>> GetAllProjectMembersAsync();

        /// <summary>
        /// Retrieves all taskmembers from the system.
        /// </summary>
        /// <returns>A list of all taskmembers.</returns>
        Task<IEnumerable<Entities.ProjectMember>> GetAllProjectMembersAsync(string projectno);


    }
}
