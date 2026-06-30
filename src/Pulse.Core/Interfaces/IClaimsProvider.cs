/// <summary>
/// Interface for providing custom claims for a user.
/// </summary>
using System.Collections.Generic;
using System.Security.Claims;

namespace Pulse.Core.Interfaces
{
    public interface IClaimsProvider
    {
 
        /// <summary>
        /// Generates claims for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of claims for the user.</returns>
        List<Claim> GetClaimsForUser(int userId);
    }
}
