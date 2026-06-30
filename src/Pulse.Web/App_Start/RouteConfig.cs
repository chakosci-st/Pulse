using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pulse.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "ProjectsOverview",
                url: "projects/overview/{code}",
                defaults: new
                {
                    controller = "Projects",
                    action = "Overview",
                    code = UrlParameter.Optional
                },
                namespaces: new[] { "Pulse.Web.Controllers" }
            );


            routes.MapRoute(
                name: "Default_projectno",
                url: "{controller}/{action}/{projectno}",
                defaults: new { controller = "Home", action = "Index", code = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Controllers" }
            );


            routes.MapRoute(
                name: "Default_code",
                url: "{controller}/{action}/{code}",
                defaults: new { controller = "Home", action = "Index", code = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Controllers" }
            );
        }
    }
}
