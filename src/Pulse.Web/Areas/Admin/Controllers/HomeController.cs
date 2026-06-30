using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [RoutePrefix("Admin/Home")]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            var breadcrumbs = SharedUtilities.Helpers.BreadcrumbHelper.GenerateBreadcrumbs(
area: "Admin",
areaTitle: "",
controller: "Home",
controllerTitle: "Administration",
action: "Index",
actionTitle: "Administration",

routeValues: new Dictionary<string, object> { }
);
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }
    }
}