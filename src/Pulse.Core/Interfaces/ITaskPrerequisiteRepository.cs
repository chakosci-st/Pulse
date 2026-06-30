using Entity = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project-task prerequisites data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ITaskPrerequisiteRepository : IBaseRepository<Entity.TaskPrerequisite, string>
    {
        /// <summary>
        /// Retrieves task prerequisites per project asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>task prerequisites by projectno</returns>
        Task<IEnumerable<Entity.TaskPrerequisite>> GetListAsync(string projectno);

 

    }
}
