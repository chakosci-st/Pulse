using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Pulse.Core.Entities;
/// <summary>
/// Interface for projectmember-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectFieldService
    {
        /// <summary>
        /// Retrieves all usergroupmembers from the system.
        /// </summary>
        /// <param name="projectno">The unique identifier of the projectno (OPTIONAL).</param>
        /// <param name="milestonesysid">The unique identifier of the milestonesysid (OPTIONAL).</param>
        /// <param name="tasksysid">The unique identifier of the tasksysid (OPTIONAL).</param>
        /// <returns>A list of all projectfields.</returns>
        Task<IEnumerable<ProjectField>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null);

        /// <summary>
        /// Retrieves a usergroupmember by its unique identifier.
        /// </summary>
        /// <param name="projectfieldsysid">The unique identifier of the projectfieldsysid.</param>
        /// <returns>The project fields with the specified projectfieldsysid, or null if not found.</returns>
        Task<ProjectField> GetByIdAsync(string projectfieldsysid);

        /// <summary>
        /// Adds a new usergroupmember to the system.
        /// </summary>
        /// <param name="usergroupmember">The usergroupmember to add.</param>
        /// <returns>key of project field.</returns>
        Task AddProjectFieldAsync(IEnumerable<ProjectField> projectfield, string loggeduserid);

        /// <summary>
        /// Updates an existing usergroupmember in the system.
        /// </summary>
        /// <param name="projectfield">The projectfield to update.</param> 
        Task UpdateProjectFieldAsync(IEnumerable<ProjectField> projectfield, string loggeduserid);

        /// <summary>
        /// Deletes a projectfield from the system by its unique identifier.
        /// </summary>
        /// <param name="projectfieldsysid">The unique identifier of the field to delete.</param>
        /// <param name="userid">The user who deleted the field.</param> 
        Task DeleteProjectFieldAsync(IEnumerable<string> projectfieldsysid, string loggeduserid);

    }
}
