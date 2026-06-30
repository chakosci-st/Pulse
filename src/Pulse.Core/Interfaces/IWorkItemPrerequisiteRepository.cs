using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing Work Item Prerequisite data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IWorkItemPrerequisiteRepository : IBaseRepository<WorkItemPrerequisite, string>
    {
        /// <summary>
        /// Retrieves all prerequisite work items from the repository.
        /// </summary>
        /// <param name="workitemsysid">The unique identifier of the member to retrieve.</param>  
        /// <param name="prerequisiteworkitemsysid">The unique identifier of the member to retrieve.</param>  
        /// <returns>A list of all prerequisite work items.</returns>
        Task<IEnumerable<WorkItemPrerequisite>> GetListAsync(string workitemsysid = null, string prerequisiteworkitemsysid = null);

    }
}
