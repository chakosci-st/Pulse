using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for UserGroupMember-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserGroupMemberService
    {
        /// <summary>
        /// Retrieves all usergroupmembers from the system.
        /// </summary>
        /// <returns>A list of all usergroupmembers.</returns>
        Task<IEnumerable<UserGroupMember>> GetAllUserGroupMembersAsync();

        /// <summary>
        /// Retrieves all usergroupmembers from the system.
        /// </summary>
        /// <returns>A list of all usergroupmembers.</returns>
        Task<IEnumerable<UserGroupMember>> GetAllUserGroupMembersAsync(int usergroupid, string userid);


        /// <summary>
        /// Retrieves a usergroupmember by its unique identifier.
        /// </summary>
        /// <param name="usergroupmemberid">The unique identifier of the usergroupmember.</param>
        /// <returns>The usergroupmember with the specified USERGROUPCODE, or null if not found.</returns>
        Task<UserGroupMember> GetUserGroupMemberByIdAsync(string usergroupmemberid);

        /// <summary>
        /// Adds a new usergroupmember to the system.
        /// </summary>
        /// <param name="usergroupmember">The usergroupmember to add.</param>
        /// <returns>Rows affected.</returns>
        Task<string> AddUserGroupMemberAsync(UserGroupMember usergroupmember);

        /// <summary>
        /// Updates an existing usergroupmember in the system.
        /// </summary>
        /// <param name="usergroupmember">The usergroupmember to update.</param>
        /// <returns>Rows affected.</returns>
        Task<int> UpdateUserGroupMemberAsync(UserGroupMember usergroupmember);

        /// <summary>
        /// Deletes a usergroupmember from the system by its unique identifier.
        /// </summary>
        /// <param name="usergroupmemberid">The unique identifier of the user to delete.</param>
        /// <param name="userid">The user who deleted the usergroupmember.</param>
        /// <returns>Rows affected.</returns>
        Task<int> DeleteUserGroupMemberAsync(string usergroupmemberid, string userid);
    }
}
