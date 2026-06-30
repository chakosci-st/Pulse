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
    public class FormsController : Controller
    {
        // GET: Templates/Forms  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "FORMVIEW")]
        public ActionResult Index()
        {


            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Forms",
                controllerTitle: "Forms",
                action: "Index",
                actionTitle: "List",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Templates/Form/Reports 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "FORMVIEW")]
        public ActionResult Reports()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Forms",
                controllerTitle: "Forms",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }

        // GET: Templates/Forms/New 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "FORMADD")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Forms",
                controllerTitle: "Forms",
                action: "New",
                actionTitle: "New Form",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


        // GET: Templates/Forms/Copy 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "FORMADD")]
        public ActionResult Copy(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Templates",
                controller: "Forms",
                controllerTitle: "Forms",
                action: "New",
                actionTitle: "New Form",
                areaTitle: "Templates",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.Id = code;
            return View();
        }

        // GET: Templates/Forms/Edit/{code}  
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "FORMEDIT")]
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
               controller: "Forms",
               controllerTitle: "Forms",
               action: "Edit",
               actionTitle: "Edit Form",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Templates/Forms/Display/{code} 
        [Filters.AuthorizeUserGroup(Groups = "", Modules = "FORMVIEW")]
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
               controller: "Forms",
               controllerTitle: "Forms",
               action: "Display",
               actionTitle: "Display Form",
               areaTitle: "Templates",
               routeValues: new Dictionary<string, object> { { "code", resolvedCode } }
           );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


    }
}