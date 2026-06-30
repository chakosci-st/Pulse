using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
namespace Pulse.Core.Interfaces
{
    public interface INotificationService
    {
  
        /// <summary>
        /// Adds a new Notification to the system.
        /// </summary>
        /// <param name="attachment">The Notification to add.</param>
        Task<string> AddAsync(Notification obj);

        /// <summary>
        /// Update a new Notification to the system.
        /// </summary>
        /// <param name="attachment">The Notification to add.</param>
        Task<int> EditAsync(Notification obj);

        /// <summary>
        /// Delete a new Notification to the system.
        /// </summary>
        /// <param name="attachment">The Notification to add.</param>
        Task<int> DeleteAsync(Notification obj);


        /// <summary>
        /// Retrieves all Notification from the system.
        /// </summary> 
        /// <returns>A list of all Notification.</returns>
        Task<Notification> GetAsync(string id);


        /// <summary>
        /// Retrieves all Notification from the system.
        /// </summary> 
        /// <returns>A list of all Notification.</returns>
        Task<IEnumerable<Notification>> GetAllAsync();


        /// <summary>
        /// Retrieves all Notification from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <returns>A list of all Notification.</returns>
        Task<IEnumerable<Notification>> GetByProjectAsync(string projectno);

        /// <summary>
        /// Retrieves all Notification from the system.
        /// </summary>
        /// <param name="entitytype">entitytype</param>
        /// <param name="entitysysid">entitysysid</param>
        /// <returns>A list of all Notification.</returns>
        Task<IEnumerable<Notification>> GetByEntityAsync(string entitytype, string entitysysid);

        Task<int> MarkAsReadAsync(string userid, string notificationsysid);

        Task<int> MarkAsReadAsync(string userid, IEnumerable<string> notificationsysids);

        Task<IEnumerable<Notification>> GetActiveAsync(string userid);

        Task<IEnumerable<Notification>> GetActiveUnreadAsync(string userid);


    }
}
