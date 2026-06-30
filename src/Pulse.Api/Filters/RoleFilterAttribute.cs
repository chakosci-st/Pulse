using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;
using Pulse.Core.Interfaces;
using System.Security.Principal;
using System.Security.Claims;
using System.Linq;

namespace Pulse.Api.Filters
{
    public class RoleFilterAttribute : ActionFilterAttribute
    {
        public string[] Roles { get; }

        public RoleFilterAttribute(params string[] roles)
        {
            Roles = roles ?? new string[0];
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var user = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized");
                return;
            }

            var hasAnyRole = Roles.Length == 0 || Roles.Any(user.IsInRole);

            if (!hasAnyRole)
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.Forbidden, "Forbidden");
                return;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}