using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Operations.Controllers
{
    [RouteArea("Operations")]
    public class PlantsController : Controller
    {
        // GET: Templates/Plants  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Operations",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Operations",
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
                area: "Operations",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Operations",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/Plants/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Operations",
                controller: "Plants",
                controllerTitle: "Plants",
                action: "New",
                actionTitle: "New Plant",
                areaTitle: "Operations",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


        // GET: Templates/Plant/Configuration/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTEDIT")]
        public ActionResult Configuration(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var requestedCode = ViewBag.Id as string;

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Operations",
               areaTitle: "Operations",
               controller: "Plants",
               controllerTitle: "Plant",
               action: "Configuration",
               actionTitle: "Plant Configuration", 
               routeValues: new Dictionary<string, object> { { "code", requestedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = requestedCode;
            return View();

        }

        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PLANTVIEW")]
        public ActionResult Display(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Operations",
               areaTitle: "Operations",
               controller: "Plants",
               controllerTitle: "Plants",
               action: "Display",
               actionTitle: "Display Plant",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
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