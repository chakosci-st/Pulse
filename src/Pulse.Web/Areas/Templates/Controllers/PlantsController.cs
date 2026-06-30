using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Templates.Controllers
{
    [RouteArea("Templates")]
    public class PlantsController : Controller
    {
        // GET: Templates/Plants  
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/Plant/Reports 
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/Plants/New 
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "New",
                actionTitle: "New Plant",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

 
        // GET: Templates/Plant/Overview/{code}  
        public ActionResult Overview(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Templates",
               areaTitle: "Templates",
               controller: "Plants",
               controllerTitle: "Plant",
               action: "Overview",
               actionTitle: "Plant Overview", 
               routeValues: new Dictionary<string, object> { { "code", code } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = code;
            return View();

        }
 

        public PartialViewResult Tab1()
        {
            // You can pass a model if needed
            return PartialView("_Tab1Partial");
        }

        public PartialViewResult Tab2()
        {
            return PartialView("_Tab2Partial");
        }


    }
}