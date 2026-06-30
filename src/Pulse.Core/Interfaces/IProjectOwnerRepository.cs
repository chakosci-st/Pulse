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
    public interface IProjectOwnerRepository : IBaseRepository<ProjectOwner, string>
    {
        /// <summary>
        /// Retrieves project members asynchronously from the repository.
        /// </summary>
        /// <param name="parentsysid">The parentsysid</param> 
        /// <param name="parenttype">The parenttype (project, milestone, task)</param> 
        /// <returns>members by projectno</returns>
        Task<IEnumerable<ProjectOwner>> GetListAsync(string parentsysid, string parenttype);

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        Task<ProjectOwner> GetAsync(string projectno, string memberid, string parenttype, string parentsysid);
    }
}
