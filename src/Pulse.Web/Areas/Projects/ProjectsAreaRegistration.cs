using System.Web.Mvc;

namespace Pulse.Web.Areas.Projects
{
    public class ProjectsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Projects";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {


            context.MapRoute(
                name: "Projects_Index_Short",
                url: "Projects/Index",
                defaults: new
                {
                    controller = "Projects",
                    action = "Index"
                }
            );

            context.MapRoute(
                name: "Projects_StatusBoard_Short",
                url: "Projects/StatusBoard",
                defaults: new
                {
                    controller = "Projects",
                    action = "StatusBoard"
                }
            );


            context.MapRoute(
                name: "Projects_Create_Short",
                url: "Projects/Create",
                defaults: new
                {
                    controller = "Projects",
                    action = "Create"
                }
            );

            context.MapRoute(
                name: "Projects_Configure_Short",
                url: "Projects/Configure/{projectno}",
                defaults: new
                {
                    controller = "Projects",
                    action = "Configure",
                    projectno = UrlParameter.Optional
                }
            );

            context.MapRoute(
                name: "Projects_Configure_Short_2",
                url: "Projects/{projectno}/Configure",
                defaults: new
                {
                    controller = "Projects",
                    action = "Configure",
                    projectno = UrlParameter.Optional
                }
            );

            context.MapRoute(
                name: "Projects_Review_Short",
                url: "Projects/Review/{projectno}",
                defaults: new
                {
                    controller = "Projects",
                    action = "Review",
                    projectno = UrlParameter.Optional
                }
            );

            context.MapRoute(
                name: "Projects_Review_Short_2",
                url: "Projects/{projectno}/Review",
                defaults: new
                {
                    controller = "Projects",
                    action = "Review",
                    projectno = UrlParameter.Optional
                }
            );



            context.MapRoute(
                name: "Projects_Details_Short",
                url: "Projects/Details/{projectno}",
                defaults: new
                {
                    controller = "Projects",
                    action = "Details",
                    projectno = UrlParameter.Optional
                }
            );

            context.MapRoute(
                name: "Projects_Details_Short_2",
                url: "Projects/{projectno}/Details",
                defaults: new
                {
                    controller = "Projects",
                    action = "Details",
                    projectno = UrlParameter.Optional
                }
            );


            //context.MapRoute(
            //        "Projects_short",
            //        "Projects/{action}/{id}",
            //        new
            //        {
            //            controller = "Projects",  // your ProjectsController
            //            action = "Index",
            //            id = UrlParameter.Optional
            //        }
            //    );

            context.MapRoute(
                "Projects_default",
                "Projects/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}