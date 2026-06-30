using System.Web.Mvc;

namespace Pulse.Web.Areas.Operations
{
    public class OperationsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Operations";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {

            context.MapRoute(
                name: "Operations_default_code",
                url: "Operations/{controller}/{action}/{code}",
                defaults: new { action = "Index", code = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Operations.Controllers" }
            );

            context.MapRoute(
                name: "Operations_default",
                url: "Operations/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Operations.Controllers" }
            );
        }
    }
}