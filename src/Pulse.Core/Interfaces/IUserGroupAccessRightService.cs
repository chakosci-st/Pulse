using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for user group access rights business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserGroupAccessRightService
    {
        /// <summary>
        /// Retrieves all user group access rights from the system.
        /// </summary>
        /// <returns>A list of all user group access rights.</returns>
        Task<IEnumerable<UserGroupAccessRight>> GetAllAccessRightsAsync(int? usergroupid = null, string modulecode = null);

        /// <summary>
        /// Retrieves an access right by its unique identifier.
        /// </summary>
        /// <param name="usergroupaccessrightsysid">The unique identifier of the access right.</param> 
        /// <returns>The access right with the specified module code and usergroupid, or null if not found.</returns>
        Task<UserGroupAccessRight> GetAccessRightByIdAsync(string usergroupaccessrightsysid);

        /// <summary>
        /// Adds a new user access to the system.
        /// </summary>
        /// <param name="useraccess">The user access to add.</param>
        Task<string> AddAccessRightAsync(UserGroupAccessRight useraccess);

        /// <summary>
        /// Updates an existing user access in the system.
        /// </summary>
        /// <param name="useraccess">The user access to update.</param>
        Task<int> UpdateAccessRightAsync(UserGroupAccessRight useraccess);

        /// <summary>
        /// Deletes a user access from the system by its unique identifier.
        /// </summary>
        /// <param name="usergroupaccessrightsysid">The unique identifier of the member to delete.</param> 
        /// <param name="loggeduserid">The user who deleted the member.</param>
        Task<int> DeleteAccessRightAsync(string usergroupaccessrightsysid, string loggeduserid);

         
    }
}
