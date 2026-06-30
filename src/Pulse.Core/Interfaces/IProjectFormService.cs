using Entities = Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Pulse.Core.Entities;
/// <summary>
/// Interface for projectmember-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IProjectFormService
    {
 
        /// <summary>
        /// Adds a new form submissions to the system.
        /// </summary>
        /// <param name="usergroupmember">The usergroupmember to add.</param>
        /// <returns>key of project field.</returns>
        Task SubmitFormAsync(ProjectFormSubmission form, string loggeduserid);

        /// <summary>
        /// Update form submissions to the system.
        /// </summary>
        /// <param name="usergroupmember">The usergroupmember to add.</param>
        /// <returns>key of project field.</returns>
        Task UpdateFormAsync(ProjectFormSubmission form, string loggeduserid);

        Task<List<ProjectFormSubmissionValue>> GetFormValuesBySubmissionAsync(string id);

        Task<ProjectFormSubmissionValue> GetFormValueAsync(string id);
    }
}
