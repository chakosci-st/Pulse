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
    public interface IProjectMilestoneService:IBaseChangeStatusService<ProjectMilestone>
    {
        /// <summary>
        /// Updates an existing projectmilestone in the system.
        /// </summary>
        /// <param name="projectmilestone">The projectmilestone to update.</param>
        /// <param name="loggeduser">loggeduser.</param>
        Task UpdateMilestoneAsync(ProjectMilestone projectmilestone , string loggeduser);


        /// <summary>
        /// Updates an existing projectmilestone in the system.
        /// </summary>
        /// <param name="projectmilestone">The projectmilestone to update.</param>
        Task ChangeMilestoneTargetAsync(string milestonesysid, string targetstart, string targetcompletion, string transactionkey, string loggeduser, string reason);
           

        /// <summary>
        /// Retrieves a projectmilestone by its unique identifier.
        /// </summary>
        /// <param name="projectmilestoneno">The unique identifier of the projectmilestone.</param>
        /// <returns>The projectmilestone with the specified USERID, or null if not found.</returns>
        Task<ProjectMilestone> GetProjectMilestoneByIdAsync(string id);
 
        /// <summary>
        /// Retrieves all projectmilestones from the system.
        /// </summary>
        /// <returns>A list of all projectmilestones.</returns>
        Task<IEnumerable<ProjectMilestone>> GetAllProjectMilestonesAsync();

        /// <summary>
        /// Retrieves all projectmilestones from the system.
        /// </summary>
        /// <returns>A list of all projectmilestones.</returns>
        Task<IEnumerable<ProjectMilestone>> GetAllProjectMilestonesAsync(string projectno);

        /// <summary>
        /// Retrieves all projectmilestones from the system.
        /// </summary>
        /// <returns>A list of all projectmilestones.</returns>
        Task<IEnumerable<TargetRevision>> GetAllTargetRevisionsAsync(string projectno, string milestonesysid);

        /// <summary>
        /// Updates an existing projectmilestone in the system.
        /// </summary>
        /// <param name="projectmilestone">The projectmilestone to update.</param> 
        /// <param name="reason">reason</param> 
        /// <param name="loggeduser">loggeduser</param> 
        Task SetTargetAsync(ProjectMilestone milestone, string reason, string loggeduser);
    }
}
