using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;
using Pulse.Core.Interfaces;
using Autofac.Integration.WebApi;
using Autofac;

namespace Pulse.Api.Filters
{
    public class RequireRoadmapExistsAttribute : ActionFilterAttribute
    {
 

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // Get the "code" parameter from the action
            if (actionContext.ActionArguments.ContainsKey("code"))
            {
                var code = actionContext.ActionArguments["code"]?.ToString();
                var autofacScope = actionContext.Request.GetDependencyScope() as AutofacWebApiDependencyScope;
                var requestScope = autofacScope?.GetRequestLifetimeScope();

                var RoadmapService = requestScope?.Resolve<IRoadmapService>();
                // Synchronously check if the plant exists (for demo; use async if possible)
                var plant = RoadmapService?.GetRoadmapById(code);

                if (plant == null)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.NotFound, "Roadmap does not exist.");
                    return;
                }
            }

            base.OnActionExecuting(actionContext);
        }
    }
}