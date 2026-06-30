using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Templates.Controllers
{
    [RouteArea("Templates")]
    public class ProductionCalendarsController : Controller
    {
        // GET: Templates/ProductionCalendars  
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductionCalendars",
                controllerTitle: "Production Calendars",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/ProductionCalendars/Reports 
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductionCalendars",
                controllerTitle: "Production Calendars",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/ProductionCalendars/New 
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

        // GET: Templates/ProductionCalendars/Edit/{code}  
        public ActionResult Edit(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Templates",
               controller: "ProductionCalendars",
               controllerTitle: "Production Calendars",
               action: "Edit",
               actionTitle: "Edit Calendar",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", code } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = code;
            return View();

        }

        // GET: Templates/ProductionCalendars/Display/{code} 
        public ActionResult Display(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
            area: "Templates",
            controller: "ProductionCalendars",
            controllerTitle: "Production Calendars",
            action: "Display",
            actionTitle: code,
            areaTitle: "Templates",
            routeValues: new Dictionary<string, object> { { "code", code } }
        );


            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = code;
            return View();
        }
    }
}