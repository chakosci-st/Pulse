using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Pulse.Web.Helpers;

namespace Pulse.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeUserGroupAttribute : AuthorizeAttribute
    {
        public string Groups { get; set; }   // comma-separated
        public string Modules { get; set; }  // comma-separated

        public AuthorizeUserGroupAttribute()
        {
            System.Diagnostics.Debug.WriteLine(">>> MVC AuthorizeUserGroupAttribute ctor HIT <<<");
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            System.Diagnostics.Debug.WriteLine(">>> MVC AuthorizeUserGroupAttribute AuthorizeCore HIT <<<");

            if (httpContext == null)
                return false;

            var user = httpContext.User as ClaimsPrincipal;
            if (user == null || !user.Identity.IsAuthenticated)
                return false;

            if (AuthorizationHelper.HasSuperUserModule(user))
                return true;

            // Parse required groups/modules from attribute
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

            // If nothing is required, treat as no restriction
            if (!anyGroupRequired && !anyModuleRequired)
                return true;

            var userGroups = new List<string>();
            var moduleCodes = new List<string>();

            if (anyGroupRequired)
            {
                var userGroupsClaim = user.Claims.FirstOrDefault(c => c.Type == "usergroups");
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
                var moduleCodesClaim = user.Claims.FirstOrDefault(c => c.Type == "modulecodes");
                if (moduleCodesClaim != null && !string.IsNullOrWhiteSpace(moduleCodesClaim.Value))
                {
                    moduleCodes = moduleCodesClaim.Value
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .ToList();
                }
            }

            // Groups: at least one match
            var groupAuthorized = anyGroupRequired &&
                                  requiredGroups.Any(rg =>
                                      userGroups.Any(ug =>
                                          string.Equals(ug, rg, StringComparison.OrdinalIgnoreCase)));

            // Modules: at least one match
            var moduleAuthorized = anyModuleRequired &&
                                   requiredModuleCodes.Any(rm =>
                                       moduleCodes.Any(um =>
                                           string.Equals(um, rm, StringComparison.OrdinalIgnoreCase)));

            // Final rule: group OR module
            return groupAuthorized || moduleAuthorized;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // If you want HTTP 403 instead of redirect to Login
            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                "You are not authorized for this resource.");

            // redirect to login, comment above and use:
            // base.HandleUnauthorizedRequest(filterContext);
        }
    }
}