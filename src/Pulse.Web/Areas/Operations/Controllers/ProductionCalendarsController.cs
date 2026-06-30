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
    public class ProductionCalendarsController : Controller
    {
        // GET: Operations/ProductionCalendars  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CALNDRVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Operations",
                controller: "ProductionCalendars",
                controllerTitle: "Production Calendars",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Operations",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Operations/ProductionCalendars/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CALNDRVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Operations",
                controller: "ProductionCalendars",
                controllerTitle: "Production Calendars",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Operations",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Operations/ProductionCalendars/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CALNDRGEN")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductionCalendars",
                controllerTitle: "Production Calendars",
                action: "New",
                actionTitle: "New Calendar",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Operations/ProductionCalendars/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CALNDRGEN")]
        public ActionResult Edit(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Operations",
               controller: "ProductionCalendars",
               controllerTitle: "Production Calendars",
               action: "Edit",
               actionTitle: "Edit Calendar",
               areaTitle: "Operations",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Operations/ProductionCalendars/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CALNDRVIEW")]
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
            controller: "ProductionCalendars",
            controllerTitle: "Production Calendars",
            action: "Display",
            actionTitle: resolvedCode,
            areaTitle: "Operations",
            routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
        );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }
    }
}