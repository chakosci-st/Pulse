using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Auth.Identity.Models
{
    public static class CustomClaimsPrincipal
    {
        /// <summary>
        /// Creates a ClaimsPrincipal with all custom claims.
        /// </summary>
        /// <param name="claimsDictionary">A dictionary of claim type to value.</param>
        /// <param name="roles">A list of AD group names (roles).</param>
        /// <returns>A ClaimsPrincipal with a CustomClaimsIdentity.</returns>
        public static ClaimsPrincipal Create(
            IDictionary<string, string> claimsDictionary,
            IList<string> roles)
        {
            var claims = new List<Claim>();

            // Add custom claims from the dictionary
            foreach (var kvp in claimsDictionary)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value));
                }
            }

            // Add role claims
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            // Create the custom identity and principal
            var identity = new CustomClaimsIdentity(new ClaimsIdentity(claims, "CustomAD"));
            return new ClaimsPrincipal(identity);
        }
    }
}
