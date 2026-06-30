using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Sites.Controllers
{
    [RouteArea("Sites")]
    public class PlantsController : Controller
    {
        // GET: Templates/Plants  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Sites",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Sites",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/Plant/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Sites",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Sites",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/Plants/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Sites",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "New",
                actionTitle: "New Plant",
                areaTitle: "Sites",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/Plant/Overview/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTEDIT")]
        public ActionResult Overview(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Sites",
               controller: "Plants",
               controllerTitle: "Plants",
               action: "Overview",
               actionTitle: "Plant Overview",
               areaTitle: "Sites",
               routeValues: new Dictionary<string, object> { { "code", code } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = code;
            return View();

        }

        // GET: Templates/Plants/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTVIEW")]
        public ActionResult Display(string code)
        {
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