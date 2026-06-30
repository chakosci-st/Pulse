using System.Security.Claims;
using System.Security.Principal;

namespace Pulse.SharedUtilities.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        public static string GetClaim(this IIdentity identity, string claimType)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity?.FindFirst(claimType)?.Value;
        }


    }

}

 