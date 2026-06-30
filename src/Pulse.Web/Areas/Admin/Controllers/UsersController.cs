using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Admin.Controllers
{
    [RouteArea("Admin")] 
    public class UsersController : Controller
    {
        // GET: Admin/Users
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USERVIEW")]
        public ActionResult Index()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "Users",
                controllerTitle: "Users",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USERVIEW")]
        public ActionResult Reports()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "Users",
                controllerTitle: "Users",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USERADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "Users",
                controllerTitle: "Users",
                action: "New",
                actionTitle: "New User",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USEREDIT")] 
        public ActionResult Configure(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedId = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "Users",
               controllerTitle: "Users",
               action: "Configure",
               actionTitle: "Configure User",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "id", resolvedId } }
           );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USEREDIT")] 
        public ActionResult Edit(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedId = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "Users",
               controllerTitle: "Users",
               action: "Edit",
               actionTitle: "Edit User",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "id", resolvedId } }
           );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USERVIEW")]
        public ActionResult Display(string code)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, code, "code", "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedCode = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "Users",
               controllerTitle: "Users",
               action: "Display",
               actionTitle: "Display User",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );

            ViewBag.Breadcrumbs = breadcrumbs;

            return View("/Settings/Profile/Index");
        }
    }
}