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
    public interface INotificationRepository : IBaseRepository<Notification, string>
    {
 

        /// <summary>
        /// Retrieves notifications per project asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">projectno</param>  
        /// <returns>Notifications</returns>
        Task<IEnumerable<Notification>> GetListAsync(string projectno);

        /// <summary>
        /// Retrieves notifications per entity asynchronously from the repository.
        /// </summary> 
        /// <param name="entitytype">entitytype</param> 
        /// <param name="entitysysid">entitysysid</param> 
        /// <returns>Notifications</returns>
        Task<IEnumerable<Notification>> GetListAsync(string entitytype, string entitysysid);

        Task<int> MarkAsReadAsync(string userid, string notificationsysid);

        Task<int> MarkAsReadAsync(string userid, IEnumerable<string> notificationsysids);

        Task<IEnumerable<Notification>> GetActiveAsync(string userid);

        Task<IEnumerable<Notification>> GetActiveUnreadAsync(string userid);

    }
}
