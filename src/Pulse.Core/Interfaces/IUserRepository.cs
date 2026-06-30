using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for managing user data in the repository.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IUserRepository : IBaseRepository<User, string>
    {
         /// <summary>
        /// Retrieves a user by its username.
        /// </summary>
        /// <param name="username">The unique identifier of the user.</param>
        /// <returns>The user with the specified USERNAME, or null if not found.</returns>
        Task<User> GetByUserNameAsync(string username);
         
    }
}
