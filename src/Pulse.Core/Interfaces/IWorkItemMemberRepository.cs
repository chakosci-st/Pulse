using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing field data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IWorkItemMemberRepository : IBaseRepository<WorkItemMember, string>
    {
        /// <summary>
        /// Retrieves all work items from the repository.
        /// </summary>
        /// <param name="workitemsysid">The unique identifier of the member to retrieve.</param>  
        /// <returns>A list of all work item owners.</returns>
        Task<IEnumerable<WorkItemMember>> GetListAsync(string workitemsysid = null);
    }
}
