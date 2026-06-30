using Entity = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core.Entities;
/// <summary>
/// Interface for managing project-tasks data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ITaskRepository : IBaseRepository<Entity.Task, string>
    {
        /// <summary>
        /// Retrieves tasks per project asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>tasks by projectno</returns>
        Task<IEnumerable<TaskSearchQuery>> GetListAsync(string projectno = null, string milestonesysid = null, string tasksysid = null, string maturitycode = null, string userid = null, int? usergroupid = null, string adgroup = null, DateTime? displayrangefrom = null, DateTime? displayrangeto = null, string status = null);



    }
}
