using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    public class ModulesController : Controller
    {
        // GET: Admin/Modules  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "MODULEVIEW")]
        public ActionResult Index()
        {
 
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "Modules",
                controllerTitle: "Modules",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Admin/Modules/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "MODULEVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "Modules",
                controllerTitle: "Modules",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Admin/Modules/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "MODULEADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "Modules",
                controllerTitle: "Modules",
                action: "New",
                actionTitle: "New Category",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Admin/Modules/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "MODULEEDIT")]
        public ActionResult Edit(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "Modules",
               controllerTitle: "Modules",
               action: "Edit",
               actionTitle: "Edit Category",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Admin/Modules/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "MODULEVIEW")]
        public ActionResult Display(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "Modules",
               controllerTitle: "Modules",
               action: "Display",
               actionTitle: "Display Module",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }
    }
}