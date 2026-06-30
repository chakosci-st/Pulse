using System.Web.Mvc;

namespace Pulse.Web.Areas.Templates
{
    public class TemplatesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Templates";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {

            context.MapRoute(
                name: "Templates_default_code",
                url: "Templates/{controller}/{action}/{code}",
                defaults: new { action = "Index", code = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Templates.Controllers" }
            );

            context.MapRoute(
                name: "Templates_default",
                url: "Templates/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Templates.Controllers" }
            );
        }
    }
}