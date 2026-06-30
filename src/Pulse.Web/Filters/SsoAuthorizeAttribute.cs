using System.Web.Mvc;

namespace Pulse.Web.Filters
{
    public class SsoAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var requestedUrl = request.RawUrl;

            filterContext.HttpContext.Session["ReturnUrl"] = requestedUrl;

            var urlHelper = new UrlHelper(request.RequestContext);
            var ssoLoginUrl = urlHelper.Action("Relogin", "Auth");

            filterContext.Result = new RedirectResult(ssoLoginUrl);
        }
    }
}