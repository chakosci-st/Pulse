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
    public class CategoriesController : Controller
    {

        // GET: Templates/Categories  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CATGRYVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Categories",
                controllerTitle: "Categories",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


        // GET: Templates/Categories/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CATGRYVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Categories",
                controllerTitle: "Categories",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/Categories/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CATGRYADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Categories",
                controllerTitle: "Categories",
                action: "New",
                actionTitle: "New Category",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/Categories/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CATGRYEDIT")]
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
               controller: "Categories",
               controllerTitle: "Categories",
               action: "Edit",
               actionTitle: "Edit Category",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Templates/Categories/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "CATGRYVIEW")]
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
               controller: "Categories",
               controllerTitle: "Categories",
               action: "Display",
               actionTitle: "Display Category",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }
    }
}