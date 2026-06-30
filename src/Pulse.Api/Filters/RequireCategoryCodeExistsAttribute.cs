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
    public class RequireCategoryCodeExistsAttribute : ActionFilterAttribute
    {
        // Property injection
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // Get the "code" parameter from the action
            if (actionContext.ActionArguments.ContainsKey("code"))
            {
                var code = actionContext.ActionArguments["code"]?.ToString();

                // Resolve IPlantService from the current request lifetime scope
                var autofacScope = actionContext.Request.GetDependencyScope() as AutofacWebApiDependencyScope;
                var requestScope = autofacScope?.GetRequestLifetimeScope();

                var categoryService = requestScope?.Resolve<ICategoryService>();

                var category = categoryService?.GetCategoryByCodeAsync(code);
                if (category == null)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.NotFound, "Category code does not exist.");
                    return;
                }
            }

            base.OnActionExecuting(actionContext);
        }
    }
}