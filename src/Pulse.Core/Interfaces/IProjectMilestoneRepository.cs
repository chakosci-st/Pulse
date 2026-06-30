using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project-milestones data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectMilestoneRepository : IBaseRepository<ProjectMilestone, string>
    {
        /// <summary>
        /// Retrieves milestones per project asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>members by projectno</returns>
        Task<IEnumerable<ProjectMilestone>> GetListAsync(string projectno);

        Task<int> UpdateTargetDateAsync(ProjectMilestone task);

    }
}
