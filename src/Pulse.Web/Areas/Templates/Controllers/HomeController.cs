using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Templates.Controllers
{
    [RouteArea("Templates")]
    [RoutePrefix("Templates/Home")]
    public class HomeController : Controller
    {
        // GET: Templates/Home
        public ActionResult Index()
        {
            var breadcrumbs = SharedUtilities.Helpers.BreadcrumbHelper.GenerateBreadcrumbs(
    area: "Templates",
        areaTitle: "",
    controller: "Home",
    controllerTitle: "Templates",
    action: "Index",
    actionTitle: "Templates",

    routeValues: new Dictionary<string, object> { }
);
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }
    }
}