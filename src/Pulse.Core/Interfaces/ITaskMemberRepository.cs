using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing task member data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ITaskMemberRepository : IBaseRepository<TaskMember, string>
    {
        /// <summary>
        /// Retrieves project members asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>members by task per project</returns>
        Task<IEnumerable<TaskMember>> GetListAsync(string projectno);

        /// <summary>
        /// Retrieves project members asynchronously from the repository.
        /// </summary>
        /// <param name="tasksysid">The task id.</param> 
        /// <returns>members by task</returns>
        Task<IEnumerable<TaskMember>> GetByTaskAsync(string tasksysid);


    }
}
