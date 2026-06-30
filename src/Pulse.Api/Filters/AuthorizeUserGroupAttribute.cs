using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Pulse.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeUserGroupAttribute : AuthorizeAttribute
    {
        public string Groups { get; set; }   // comma-separated
        public string Modules { get; set; }  // comma-separated

        public AuthorizeUserGroupAttribute()
        {
            System.Diagnostics.Debug.WriteLine(">>> AuthorizeUserGroupAttribute ctor HIT <<<");
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {

            System.Diagnostics.Debug.WriteLine(">>> AuthorizeUserGroupAttribute IsAuthorized.Get HIT <<<");

            var principal = actionContext.ControllerContext.RequestContext.Principal as ClaimsPrincipal;
            if (principal == null || !principal.Identity.IsAuthenticated)
                return false;

            // Parse required
            var requiredGroups = (Groups ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .ToList();

            var requiredModuleCodes = (Modules ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .ToList();

            var anyGroupRequired = requiredGroups.Any();
            var anyModuleRequired = requiredModuleCodes.Any();

            // If nothing is required, treat as no restriction (or change to "return false" if you prefer)
            if (!anyGroupRequired && !anyModuleRequired)
                return true;

            var userGroups = new List<string>();
            var moduleCodes = new List<string>();

            if (anyGroupRequired)
            {
                var userGroupsClaim = principal.Claims
                    .FirstOrDefault(c => c.Type == "usergroups");

                if (userGroupsClaim != null && !string.IsNullOrWhiteSpace(userGroupsClaim.Value))
                {
                    userGroups = userGroupsClaim.Value
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .ToList();
                }
            }

            if (anyModuleRequired)
            {
                var moduleCodesClaim = principal.Claims
                    .FirstOrDefault(c => c.Type == "modulecodes");

                if (moduleCodesClaim != null && !string.IsNullOrWhiteSpace(moduleCodesClaim.Value))
                {
                    moduleCodes = moduleCodesClaim.Value
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .ToList();
                }
            }

            // If groups are specified, user must match at least one group to be group-authorized
            var groupAuthorized = anyGroupRequired &&
                                  requiredGroups.Any(rg =>
                                      userGroups.Any(ug =>
                                          string.Equals(ug, rg, StringComparison.OrdinalIgnoreCase)));

            // If modules are specified, user must match at least one module to be module-authorized
            var moduleAuthorized = anyModuleRequired &&
                                   requiredModuleCodes.Any(rm =>
                                       moduleCodes.Any(um =>
                                           string.Equals(um, rm, StringComparison.OrdinalIgnoreCase)));

            // Final rule: group OR module
            return groupAuthorized || moduleAuthorized;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request
                .CreateResponse(HttpStatusCode.Forbidden, "You are not authorized for this resource.");
        }
    }
}