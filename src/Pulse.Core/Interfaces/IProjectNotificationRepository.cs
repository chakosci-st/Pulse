using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing project member data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectNotificationRepository : IBaseRepository<ProjectNotification, string>
    {
        /// <summary>
        /// Retrieves project Notifications asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>Notifications</returns>
        Task<IEnumerable<ProjectNotification>> GetListAsync(string projectno);

        /// <summary>
        /// Retrieves projects per Notifications asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">projectno</param> 
        /// <param name="entitytype">entitytype</param> 
        /// <param name="entitysysid">entitysysid</param> 
        /// <returns>Notifications</returns>
        Task<IEnumerable<ProjectNotification>> GetByEntityAsync(string projectno, string entitytype, string entitysysid);

    }
}
