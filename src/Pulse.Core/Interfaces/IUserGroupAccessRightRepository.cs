using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing user group access right data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserGroupAccessRightRepository : IBaseRepository<UserGroupAccessRight, string>
    {
        /// <summary>
        /// Retrieves all the user group access rights from the repository.
        /// </summary>
        /// <param name="usergroupid">The unique identifier of the access right to retrieve.</param>
        /// <param name="modulecode">The unique identifier of the access right to retrieve.</param>
        /// <returns>A list of all user group access rights.</returns>
        Task<IEnumerable<UserGroupAccessRight>> GetListAsync(int? usergroupid = null, string modulecode = null);

        Task<UserGroupAccessRight> GetAsync(int usergroupid, string modulecode);

        Task<IEnumerable<UserGroupModule>> GetModulesAsync(int usergroupid);
        
    }
}
