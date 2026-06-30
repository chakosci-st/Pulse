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
    public interface IProjectAttachmentRepository : IBaseRepository<ProjectAttachment, string>
    {
        /// <summary>
        /// Retrieves project Attachment asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>Attachments</returns>
        Task<IEnumerable<ProjectAttachment>> GetListAsync(string projectno);

        /// <summary>
        /// Retrieves projects per Attachment asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">projectno</param> 
        /// <param name="entitytype">entitytype</param> 
        /// <param name="entitysysid">entitysysid</param> 
        /// <returns>Attachments</returns>
        Task<IEnumerable<ProjectAttachment>> GetByEntityAsync(string projectno, string entitytype, string entitysysid);

    }
}
