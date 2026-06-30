using System.Web.Mvc;

namespace Pulse.Web.Areas.Sites
{
    public class SitesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Sites";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute(
            //        "Sites_short",
            //        "Sites/{action}/{id}",
            //        new
            //        {
            //            controller = "Sites",
            //            action = "Index",
            //            id = UrlParameter.Optional
            //        }
            //    );

            context.MapRoute(
                name: "Sites_Create_Short",
                url: "Sites/Create",
                defaults: new
                {
                    controller = "Plants",
                    action = "Create"
                }
            );

            context.MapRoute(
                name: "Sites_Update_Short",
                url: "Sites/Create",
                defaults: new
                {
                    controller = "Plants",
                    action = "Create"
                }
            );


            context.MapRoute(
                "Sites_default",
                "Sites/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}