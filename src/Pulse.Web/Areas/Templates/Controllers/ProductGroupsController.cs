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
    public class ProductGroupsController : Controller
    {
        // GET: Templates/ProductGroups  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PGRPVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductGroups",
                controllerTitle: "Product Groups",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/ProductDivision/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PGRPVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductGroups",
                controllerTitle: "Product Groups",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/ProductGroups/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PGRPADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductGroups",
                controllerTitle: "Product Groups",
                action: "New",
                actionTitle: "New Product Division",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/ProductGroups/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PGRPEDIT")]
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
               controller: "ProductGroups",
               controllerTitle: "Product Groups",
               action: "Edit",
               actionTitle: "Edit Product Division",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Templates/ProductGroups/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PGRPVIEW")]
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
               controller: "ProductGroups",
               controllerTitle: "Product Groups",
               action: "Display",
               actionTitle: "Display Product Group",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


    }
}