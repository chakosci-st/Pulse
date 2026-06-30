using Pulse.Auth.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Auth.Identity.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static CustomClaimsIdentity GetCustomIdentity(this ClaimsPrincipal principal)
        {
            return principal.Identity as CustomClaimsIdentity;
        }
    }
}
