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
    public interface IProjectCommentService
    {
        /// <summary>
        /// Adds a new Comment to the system.
        /// </summary>
        /// <param name="Comment">The Comment to add.</param>
        Task<string> AddAsync(ProjectComment obj);

        /// <summary>
        /// Retrieves all Comment from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <returns>A list of all Comments.</returns>
        Task<IEnumerable<ProjectComment>> GetByProjectAsync(string projectno);

        /// <summary>
        /// Retrieves all Comments from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <param name="entitytype">entitytype</param>
        /// <param name="entitysysid">entitysysid</param>
        /// <returns>A list of all Comments.</returns>
        Task<IEnumerable<ProjectComment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid);


    }
}
