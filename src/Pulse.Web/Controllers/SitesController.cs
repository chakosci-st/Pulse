using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Pulse.Web.Controllers
{
    public class SitesController : Controller
    {
        // GET: Sites
        public ActionResult Index()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                controller: "Sites",
                controllerTitle: "Sites",
                action: "Reports",
                actionTitle: "Reports",
                areaTitle: "",
                routeValues: new Dictionary<string, object> { }
            );

            return View();
        }
    }
}