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
    public class UserGroupsController : Controller
    {
        // GET: Admin/UserGroups  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USRGRPVIEW")]
        public ActionResult Index()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "UserGroups",
                controllerTitle: "User Groups",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Admin/UserGroups/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USRGRPVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "UserGroups",
                controllerTitle: "User Groups",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Admin/UserGroups/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USRGRPADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Admin",
                controller: "UserGroups",
                controllerTitle: "User Groups",
                action: "New",
                actionTitle: "New User Group",
                areaTitle: "Adminitration",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Admin/UserGroups/Configure/{id}   
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USRGRPEDIT")]
        public ActionResult Configure(int id)
        {
            var redirectResult = PageRouteValueHelper.ResolveIntRouteValue(this, id, "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedId = Convert.ToInt32(ViewBag.Id);
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "UserGroups",
               controllerTitle: "User Groups",
               action: "Configure",
               actionTitle: "Configure User Group",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "id", resolvedId } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }


        // GET: Admin/UserGroups/Edit/{id}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USRGRPEDIT")]
        public ActionResult Edit(int id)
        {
            var redirectResult = PageRouteValueHelper.ResolveIntRouteValue(this, id, "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedId = Convert.ToInt32(ViewBag.Id);
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
               area: "Admin",
               controller: "UserGroups",
               controllerTitle: "User Groups",
               action: "Edit",
               actionTitle: "Edit User Group",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "id", resolvedId } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Admin/UserGroups/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "USRGRPVIEW")]
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
               controller: "UserGroups",
               controllerTitle: "User Groups",
               action: "Display",
               actionTitle: "Display User Group",
               areaTitle: "Adminitration",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }
    }
}