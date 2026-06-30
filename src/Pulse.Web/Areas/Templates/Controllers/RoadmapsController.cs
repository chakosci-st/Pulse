using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Templates.Controllers
{
    [RouteArea("Templates")]
    public class RoadmapsController : Controller
    {
        // GET: Templates/Roadmaps  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "RMAPVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Roadmaps",
                controllerTitle: "Roadmaps",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/Roadmap/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "RMAPVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Roadmaps",
                controllerTitle: "Roadmaps",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/Roadmaps/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "RMAPADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Roadmaps",
                controllerTitle: "Roadmaps",
                action: "New",
                actionTitle: "New Roadmap",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


        // GET: Templates/Roadmaps/Copy 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "RMAPADD")]
        public ActionResult Copy(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Roadmaps",
                controllerTitle: "Roadmaps",
                action: "New",
                actionTitle: "New Roadmap",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = code;
            return View();
        }

        // GET: Templates/Roadmaps/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "RMAPEDIT")]
        public ActionResult Edit(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Templates",
               controller: "Roadmaps",
               controllerTitle: "Roadmaps",
               action: "Edit",
               actionTitle: "Edit Roadmap",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Templates/Roadmaps/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "RMAPVIEW")]
        public ActionResult Display(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Templates",
               controller: "Roadmaps",
               controllerTitle: "Roadmaps",
               action: "Display",
               actionTitle: "Display Roadmap",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


    }
}