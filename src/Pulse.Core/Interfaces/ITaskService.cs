using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for task-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface ITaskService
    {
        /// <summary>
        /// Retrieves all tasks from the system.
        /// </summary>
        /// <returns>A list of all tasks.</returns>
        Task<IEnumerable<Entities.Task>> GetAllTasksAsync();
        /// <summary>
        /// Retrieves all tasks from the system.
        /// </summary>
        /// <param name="projectno">The projectno of the task.</param>
        /// <returns>A list of all tasks.</returns>
        Task<IEnumerable<Entities.Task>> GetAllTasksPerProjectAsync(string projectno);
        /// <summary>
        /// Retrieves all tasks from the system.
        /// </summary>
        /// <param name="userid">The userid of the task.</param>
        /// <returns>A list of all tasks (includes in usergroup and activedirectorygroup).</returns>
        Task<IEnumerable<Entities.Task>> GetAllTasksPerUserAsync(string userid);
        /// <summary>
        /// Retrieves all tasks from the system.
        /// </summary>
        /// <param name="userid">The usergroupid of the task.</param>
        /// <returns>A list of all tasks.</returns>
        Task<IEnumerable<Entities.Task>> GetAllTasksPerUserGroupAsync(int usergroupid);
        /// <summary>
        /// Retrieves all tasks from the system.
        /// </summary>
        /// <param name="activedirectorygroup">The activedirectorygroup of the task.</param>
        /// <returns>A list of all tasks.</returns>
        Task<IEnumerable<Entities.Task>> GetAllTasksPerActiveDirectoryGroupAsync(string activedirectorygroup);
        /// <summary>
        /// Retrieves a task by its unique identifier.
        /// </summary>
        /// <param name="taskid">The unique identifier of the task.</param>
        /// <returns>The task with the specified USERID, or null if not found.</returns>
        Task<Entities.Task> GetTaskByIdAsync(string taskid);

        /// <summary>
        /// Adds a new task to the system.
        /// </summary>
        /// <param name="task">The task to add.</param>
        Task<string> AddTaskAsync(Entities.Task task);

        /// <summary>
        /// Updates an existing task in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        Task<int> UpdateTaskAsync(Entities.Task task);

        /// <summary>
        /// Add statuschange to NotStarted in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToNotStartedAsync(Entities.Task task, string reason);

        /// <summary>
        /// Add statuschange to Ongoing in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToOngoingAsync(Entities.Task task, string reason);

        /// <summary>
        /// Add statuschange to Completed in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToCompletedAsync(Entities.Task task, string reason);

        /// <summary>
        /// Add statuschange to Cancelled in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToCancelledAsync(Entities.Task task, string reason);

        /// <summary>
        /// Add statuschange to Hold in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToHoldAsync(Entities.Task task, string reason);

        /// <summary>
        /// Add statuschange to Failed in the system.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <param name="reason">reason.</param>
        Task ChangeStatusToFailedAsync(Entities.Task task, string reason);


        /// <summary>
        /// Deletes a task from the system by its unique identifier.
        /// </summary>
        /// <param name="taskid">The unique identifier of the task to delete.</param>
        /// <param name="loggeduser">The loggeduser who deleted the task.</param>
        Task<int> DeleteTaskAsync(string taskid, string loggeduser);


        Task<IEnumerable<Entities.TargetRevision>> GetAllTargetRevisionsAsync(string tasksysid);
    }
}
