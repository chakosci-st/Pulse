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
    public interface IProjectNotificationService
    {
        Task NotifyProjectCreated(ProjectCreatedEventArgs eventMessage);
        Task NotifyProjectCompleted(ProjectCompletedEventArgs eventMessage);
        Task NotifyProjectCancelled(ProjectCanceledEventArgs eventMessage);
        Task NotifyProjectHold(ProjectHoldEventArgs eventMessage);
        Task NotifyProjectFailed(ProjectFailedEventArgs eventMessage);
        Task NotifyProjectStarted(ProjectStartedEventArgs eventMessage);


        /// <summary>
        /// Adds a new Notification to the system.
        /// </summary>
        /// <param name="attachment">The Notification to add.</param>
        Task<string> AddAsync(ProjectNotification obj);

        /// <summary>
        /// Retrieves all Notification from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <returns>A list of all Notification.</returns>
        Task<IEnumerable<ProjectNotification>> GetByProjectAsync(string projectno);

        /// <summary>
        /// Retrieves all Notification from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <param name="entitytype">entitytype</param>
        /// <param name="entitysysid">entitysysid</param>
        /// <returns>A list of all Notification.</returns>
        Task<IEnumerable<ProjectNotification>> GetByEntityAsync(string projectno, string entitytype, string entitysysid);



    }
}
