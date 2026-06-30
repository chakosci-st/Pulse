using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Projects.Controllers
{
    public class ProjectChatsController : Controller
    {
        // GET: Projects/ProjectChats
        public ActionResult Index()
        {

            var breadcrumbs = SharedUtilities.Helpers.BreadcrumbHelper.GenerateBreadcrumbs(
  area: "Projects",
  areaTitle: "",
  controller: "ProjectChats",
  controllerTitle: "Chats",
  action: "Index",
  actionTitle: "Index",
  routeValues: new Dictionary<string, object> { }
);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }
    }
}