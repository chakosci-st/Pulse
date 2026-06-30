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
    public interface IWorkItemService
    {
        /// <summary>
        /// Retrieves all work items per plant - category / maturity code from the system.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param> 
        /// <param name="categorycode">The unique identifier of the member to retrieve.</param> 
        /// <param name="maturitycode">The unique identifier of the member to retrieve (Optional).</param> 
        /// <returns>A list of all work items per plant - category / maturity.</returns>
        Task<IEnumerable<WorkItem>> GetWorkItemListAsync(string plantcode, string categorycode, string maturitycode = null);

        /// <summary>
        /// Retrieves a work item by its unique identifier.
        /// </summary>
        /// <param name="workitemsysid">The unique identifier of the member to retrieve.</param> 
        /// <returns>The milestone with the specified PLANTCODE, categorycode and milestonecode, or null if not found.</returns>
        Task<WorkItem> GetByIdAsync(string workitemsysid);

        /// <summary>
        /// Adds a new work item to the system.
        /// </summary>
        /// <param name="workitem">The milestone to add.</param>
        Task<string> AddWorkItemAsync(WorkItem workitem);

        /// <summary>
        /// Updates an existing work item in the system.
        /// </summary>
        /// <param name="workitem">The plant - category milestone to update.</param>
        Task<int> UpdateWorkItemAsync(WorkItem workitem);

        /// <summary>
        /// Deletes a work item from the system by its unique identifier.
        /// </summary>
        /// <param name="workitemsysid">The unique identifier of the member to delete.</param> 
        /// <param name="loggeduserid">The user who deleted the member.</param>
        Task<int> DeleteWorkItemAsync(string workitemsysid, string loggeduserid);


    }
}
