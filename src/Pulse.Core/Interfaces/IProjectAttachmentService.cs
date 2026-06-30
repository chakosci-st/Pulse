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
    public interface IProjectAttachmentService
    {
        /// <summary>
        /// Adds a new attachment to the system.
        /// </summary>
        /// <param name="attachment">The attachment to add.</param>
        Task<string> AddAsync(ProjectAttachment obj);
 
        /// <summary>
        /// Retrieves all attachment from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <returns>A list of all attachments.</returns>
        Task<IEnumerable<ProjectAttachment>> GetByProjectAsync(string projectno);

        /// <summary>
        /// Retrieves all attachment from the system.
        /// </summary>
        /// <param name="projectno">projectno</param>
        /// <param name="entitytype">entitytype</param>
        /// <param name="entitysysid">entitysysid</param>
        /// <returns>A list of all attachments.</returns>
        Task<IEnumerable<ProjectAttachment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid);

        /// <summary>
        /// Retrieves an attachment by its identifier.
        /// </summary>
        /// <param name="attachmentSysId">Attachment id.</param>
        /// <returns>The attachment if found.</returns>
        Task<ProjectAttachment> GetByIdAsync(string attachmentSysId);

        /// <summary>
        /// Removes an attachment by its identifier.
        /// </summary>
        /// <param name="attachmentSysId">Attachment id.</param>
        /// <returns>Rows affected.</returns>
        Task<int> RemoveAsync(string attachmentSysId);


    }
}
