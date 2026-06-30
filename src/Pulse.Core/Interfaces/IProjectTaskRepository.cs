using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project-tasks data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectTaskRepository : IBaseRepository<ProjectTask, string>
    {
        /// <summary>
        /// Retrieves milestones per project asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>members by projectno</returns>
        Task<IEnumerable<ProjectTask>> GetListAsync(string projectno, string parenttype = null, string parentsysid = null);

        Task<int> UpdateTargetDateAsync(ProjectTask task);

        Task<IEnumerable<ProjectTaskItem>> GetTaskItemListAsync(string projecttasksysid = null, string userid = null);
    }
}
