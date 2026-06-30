using System;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Pulse.Web.Helpers
{
    public static class AuthorizationHelper
    {
        public static bool IsInGroupsOrModules(string groupsCsv, string modulesCsv)
        {
            var user = HttpContext.Current?.User as ClaimsPrincipal;
            if (user == null || !user.Identity.IsAuthenticated)
                return false;

            var requiredGroups = (groupsCsv ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .ToList();

            var requiredModuleCodes = (modulesCsv ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .ToList();

            var anyGroupRequired = requiredGroups.Any();
            var anyModuleRequired = requiredModuleCodes.Any();

            if (!anyGroupRequired && !anyModuleRequired)
                return true;

            var userGroups = new string[0];
            var moduleCodes = new string[0];

            if (anyGroupRequired)
            {
                var groupsClaim = user.Claims.FirstOrDefault(c => c.Type == "usergroups");
                if (groupsClaim != null)
                    userGroups = groupsClaim.Value
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray();
            }

            if (anyModuleRequired)
            {
                var modulesClaim = user.Claims.FirstOrDefault(c => c.Type == "modulecodes");
                if (modulesClaim != null)
                    moduleCodes = modulesClaim.Value
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray();
            }

            var groupAuthorized = anyGroupRequired &&
                requiredGroups.Any(rg =>
                    userGroups.Any(ug =>
                        string.Equals(ug, rg, StringComparison.OrdinalIgnoreCase)));

            var moduleAuthorized = anyModuleRequired &&
                requiredModuleCodes.Any(rm =>
                    moduleCodes.Any(um =>
                        string.Equals(um, rm, StringComparison.OrdinalIgnoreCase)));

            return groupAuthorized || moduleAuthorized;
        }
    }
}