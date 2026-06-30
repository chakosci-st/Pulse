using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project field data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectFieldRepository : IBaseRepository<ProjectField, string>
    {
        /// <summary>
        /// Retrieves project fields asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <param name="milestonesysid">The milestonesysid.</param> 
        /// <param name="tasksysid">The tasksysid.</param>  
        /// <returns>fields by projectno</returns>
        Task<IEnumerable<ProjectField>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null);

    }
}
