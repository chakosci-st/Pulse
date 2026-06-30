using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing work item template data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IWorkItemRepository : IBaseRepository<WorkItem, string>
    {
        /// <summary>
        /// Retrieves all work items from the repository.
        /// </summary>
        /// <param name="plantcode">The unique identifier of the member to retrieve.</param> 
        /// <param name="categorycode">The unique identifier of the member to retrieve.</param> 
        /// <param name="maturitycode">The unique identifier of the member to retrieve (Optional).</param> 
        /// <returns>A list of all work items.</returns>
        Task<IEnumerable<WorkItem>> GetListAsync(string plantcode, string categorycode, string maturitycode = null);

 
    }
}
