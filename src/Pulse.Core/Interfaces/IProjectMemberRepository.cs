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
    public interface IProjectMemberRepository : IBaseRepository<ProjectMember, string>
    {
        /// <summary>
        /// Retrieves project members asynchronously from the repository.
        /// </summary>
        /// <param name="projectno">The project no.</param> 
        /// <returns>members by projectno</returns>
        Task<IEnumerable<ProjectMember>> GetListAsync(string projectno);

        /// <summary>
        /// Retrieves projects per member asynchronously from the repository.
        /// </summary>
        /// <param name="userid">User Id</param> 
        /// <returns>members by projectno</returns>
        Task<IEnumerable<ProjectMember>> GetByMemberIdAsync(string memberid);

    }
}
