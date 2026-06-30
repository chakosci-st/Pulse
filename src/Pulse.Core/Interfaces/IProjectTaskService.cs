using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Pulse.Core.Entities;
/// <summary>
/// Interface for projectmilestone-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectTaskService : IBaseChangeStatusService<ProjectTask>
    {
        /// <summary>
        /// Add an existing projectmilestone in the system.
        /// </summary>
        /// <param name="task">The task to add.</param>
        /// <param name="loggeduser">loggeduser.</param>
        Task<string> AddTaskAsync(ProjectTask task, string loggeduser);


        /// <summary>
        /// Updates an existing projectmilestone in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="loggeduser">loggeduser.</param>
        Task UpdateTaskAsync(ProjectTask task, string loggeduser);

        /// <summary>
        /// Deletes an existing projectmilestone in the system.
        /// </summary>
        /// <param name="task">The task to delete.</param>
        /// <param name="loggeduser">loggeduser.</param>
        Task DeleteTaskAsync(ProjectTask task, string loggeduser);

        /// <summary>
        /// Retrieves a task by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the id.</param>
        /// <returns>The project task with the specified id, or null if not found.</returns>
        Task<ProjectTask> GetTaskByIdAsync(string id);


        /// <summary>
        /// Updates an existing task in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// /// <param name="reason">reason.</param>
        /// <param name="loggeduser">loggeduser.</param>
        Task SetTargetAsync(ProjectTask task, string reason, string loggeduser);

 

       Task<ProjectTaskItem> GetItemDetailsAsync(string projecttasksysid, string userid);

        Task<ProjectTaskItem> GetItemDetailsReadOnlyAsync(string projecttasksysid, string userid);

        Task<IEnumerable<ProjectTaskItem>> GetItemListAsync(string userid);


        /////// <summary>
        /////// Retrieves all projectmilestones from the system.
        /////// </summary>
        /////// <returns>A list of all projectmilestones.</returns>
        ////Task<IEnumerable<ProjectMilestone>> GetAllProjectMilestonesAsync();

        /////// <summary>
        /////// Retrieves all projectmilestones from the system.
        /////// </summary>
        /////// <returns>A list of all projectmilestones.</returns>
        ////Task<IEnumerable<ProjectMilestone>> GetAllProjectMilestonesAsync(string projectno);

        /////// <summary>
        /////// Retrieves all projectmilestones from the system.
        /////// </summary>
        /////// <returns>A list of all projectmilestones.</returns>
        ////Task<IEnumerable<TargetRevision>> GetAllTargetRevisionsAsync(string projectno, string milestonesysid);



    }
}
