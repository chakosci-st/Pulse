using System.Web.Mvc;

namespace Pulse.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
                name: "Admin_UserGroups_Configure",
                url: "Admin/UserGroups/Configure/{id}",
                defaults: new { controller = "UserGroups", action = "Configure", id = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_UserGroups_Edit",
                url: "Admin/UserGroups/Edit/{id}",
                defaults: new { controller = "UserGroups", action = "Edit", id = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_UserGroups_Display",
                url: "Admin/UserGroups/Display/{code}",
                defaults: new { controller = "UserGroups", action = "Display", code = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_default_code",
                url: "Admin/{controller}/{action}/{code}",
                defaults: new { action = "Index", code = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Pulse.Web.Areas.Admin.Controllers" }
            ); 
        }
    }
}