using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for user-related business logic.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users from the system.
        /// </summary>
        /// <returns>A list of all users.</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves a user by its unique identifier.
        /// </summary>
        /// <param name="userid">The unique identifier of the user.</param>
        /// <returns>The user with the specified USERID, or null if not found.</returns>
        Task<User> GetUserByIdAsync(string userid);

        /// <summary>
        /// Retrieves a user by its usersname.
        /// </summary>
        /// <param name="username">The unique identifier of the user.</param>
        /// <returns>The user with the specified USERNAME, or null if not found.</returns>
        Task<User> GetUserByUserNameAsync(string username);

 


        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user">The user to add.</param>
        Task<string> AddUserAsync(User user);

        /// <summary>
        /// Updates an existing user in the system.
        /// </summary>
        /// <param name="user">The user to update.</param>
        Task<int> UpdateUserAsync(User user);

        /// <summary>
        /// Deletes a user from the system by its unique identifier.
        /// </summary>
        /// <param name="userid">The unique identifier of the user to delete.</param>
        /// <param name="loggeduser">The user who deleted the user.</param>
        Task<int> DeleteUserAsync(string userid, string loggeduser);


        /// <summary>
        /// Retrieves a user Groups by its unique identifier.
        /// </summary>
        /// <param name="userid">The unique identifier of the user.</param>
        /// <returns>The user group with the specified USERID, or null if not found.</returns>
        Task<IEnumerable<UserGroupMember>> GetUserGroupsAsync(string userid);
    }
}
