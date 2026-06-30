using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for workitem prerequisite-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IWorkItemPrerequisiteService
    {
 
        /// <summary>
        /// Retrieves all prerequisite work items from the system.
        /// </summary>
        /// <param name="workitemsysid">The unique identifier of the member to retrieve.</param>  
        /// <returns>A list of all prerequisite work items.</returns>
        Task<IEnumerable<WorkItemPrerequisite>> GetAllPrerequisitesAsync(string workitemsysid);

        /// <summary>
        /// Retrieves all work items per prerequisite from the system.
        /// </summary>
        /// <param name="prerequisiteworkitemsysid">The unique identifier of the member to retrieve.</param>  
        /// <returns>A list of all work items.</returns>
        Task<IEnumerable<WorkItemPrerequisite>> GetAllWorkItemsAsync(string prerequisiteworkitemsysid);

        /// <summary>
        /// Retrieves a workitem-prerequisite link by its unique identifier.
        /// </summary>
        /// <param name="workitemprerequisitesysid">The unique identifier of the workitem-prerequisite link.</param>
        /// <returns>The prerequisite with the specified workitemprerequisitesysid, or null if not found.</returns>
        Task<WorkItemPrerequisite> GetByIdAsync(string workitemprerequisitesysid);

        /// <summary>
        /// Adds a new workitem-prerequisite to the system.
        /// </summary>
        /// <param name="prerequisite">The prerequisite to add.</param>
        Task<string> AddWorkItemPrerequisiteAsync(WorkItemPrerequisite prerequisite);

        /// <summary>
        /// Updates an existing workitem-prerequisite in the system.
        /// </summary>
        /// <param name="prerequisite">The prerequisite to update.</param>
        Task<int> UpdateWorkItemPrerequisiteAsync(WorkItemPrerequisite prerequisite);

        /// <summary>
        /// Deletes a workitem-prerequisite from the system by its unique identifier.
        /// </summary>
        /// <param name="workitemprerequisitesysid">The unique identifier of the workitem-prerequisite to delete.</param> 
        Task<int> DeleteWorkItemPrerequisiteAsync(string workitemprerequisitesysid);
    }
}
