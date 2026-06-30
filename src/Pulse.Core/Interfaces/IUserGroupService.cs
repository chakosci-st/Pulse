using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for usergroup-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserGroupService
    {
        /// <summary>
        /// Retrieves all usergroups from the system.
        /// </summary>
        /// <returns>A list of all usergroups.</returns>
        Task<IEnumerable<UserGroup>> GetAllUserGroupsAsync();

        /// <summary>
        /// Retrieves a usergroup by its unique identifier.
        /// </summary>
        /// <param name="usergroupid">The unique identifier of the usergroup.</param>
        /// <returns>The usergroup with the specified USERGROUPCODE, or null if not found.</returns>
        Task<UserGroup> GetUserGroupByIdAsync(int usergroupid);

        /// <summary>
        /// Adds a new usergroup to the system.
        /// </summary>
        /// <param name="usergroup">The usergroup to add.</param>
        /// <returns>Rows affected.</returns>
        Task<int> AddUserGroupAsync(UserGroup usergroup);

        /// <summary>
        /// Updates an existing usergroup in the system.
        /// </summary>
        /// <param name="usergroup">The usergroup to update.</param>
        /// <returns>Rows affected.</returns>
        Task<int> UpdateUserGroupAsync(UserGroup usergroup);

        /// <summary>
        /// Deletes a usergroup from the system by its unique identifier.
        /// </summary>
        /// <param name="usergroupid">The unique identifier of the user to delete.</param>
        /// <param name="userid">The user who deleted the usergroup.</param>
        /// <returns>Rows affected.</returns>
        Task<int> DeleteUserGroupAsync(int usergroupid, string userid);


        /// <summary>
        /// Retrieves all usergroups from the system.
        /// </summary>
        /// <param name="searchValue">Search key.</param>
        /// <param name="sortBy">sortBy.</param>
        /// <param name="sortDirection">sortDirection</param>
        /// <param name="isActive">The plant status.</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>A list of all usergroups.</returns>
        Task<PagedResult<UserGroup>> GetPagedUserGroupsAsync(string searchValue, string sortBy, string sortDirection, bool? isActive, int pageNumber, int pageSize);

        /// <summary>
        /// Retrieves a usergroup by its unique identifier.
        /// </summary>
        /// <param name="usergroupid">The unique identifier of the plant.</param>
        /// <returns>The usergroup with the specified usergroupid, or null if not found.</returns>
        UserGroup GetByUserGroupId(int usergroupid);
        Task<UserGroupModule> GetModuleAsync(int usergroupid, string modulecode);
        Task<IEnumerable<UserGroupModule>> GetModulesAsync(int usergroupid);

        Task AuthorizeToModule(UserGroupModule module);
        Task RestrictToModule(UserGroupModule module);

    }
}
