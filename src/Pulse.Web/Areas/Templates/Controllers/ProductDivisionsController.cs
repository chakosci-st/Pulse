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
    public class ProductDivisionsController : Controller
    {
        // GET: Templates/ProductDivisions  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PDIVVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductDivisions",
                controllerTitle: "Product Divisions",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/ProductDivision/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PDIVVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductDivisions",
                controllerTitle: "Product Divisions",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/ProductDivisions/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PDIVADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "ProductDivisions",
                controllerTitle: "Product Divisions",
                action: "New",
                actionTitle: "New Product Division",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/ProductDivisions/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PDIVEDIT")]
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
               controller: "ProductDivisions",
               controllerTitle: "Product Divisions",
               action: "Edit",
               actionTitle: "Edit Product Division",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Templates/ProductDivisions/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "PDIVVIEW")]
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
               controller: "ProductDivisions",
               controllerTitle: "Product Divisions",
               action: "Display",
               actionTitle: "Display Product Division",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


    }
}