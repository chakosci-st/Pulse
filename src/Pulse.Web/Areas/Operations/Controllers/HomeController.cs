using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Operations.Controllers
{
    [RouteArea("Operations")]
    [RoutePrefix("Operations/Home")]
    public class HomeController : Controller
    {
        // GET: Operations/Home
        public ActionResult Index()
        {
            var breadcrumbs = SharedUtilities.Helpers.BreadcrumbHelper.GenerateBreadcrumbs(
            area: "Operations",
            areaTitle: "",
            controller: "Home",
            controllerTitle: "Operations",
            action: "Index",
            actionTitle: "Operations",

            routeValues: new Dictionary<string, object> { }
            );
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }
    }
}