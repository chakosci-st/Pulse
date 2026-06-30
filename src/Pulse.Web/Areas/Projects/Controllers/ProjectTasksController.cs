using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Projects.Controllers
{
    public class ProjectTasksController : Controller
    {
        // GET: Projects/ProjectTasks
        public ActionResult Index()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
    area: "Projects",
    areaTitle: "",
    controller: "ProjectTasks",
    controllerTitle: "Tasks",
    action: "Index",
    actionTitle: "Index",
    routeValues: new Dictionary<string, object> { }
);

            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult Edit(string id)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, id, "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            ViewBag.IsTaskReadOnly = false;

            var resolvedId = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
area: "Projects",
areaTitle: "",
controller: "ProjectTasks",
controllerTitle: "Tasks",
action: "Edit",
actionTitle: "Edit",
routeValues: new Dictionary<string, object> { { "id", resolvedId } }
);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        public ActionResult ReadOnly(string id)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, id, "id");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            ViewBag.IsTaskReadOnly = true;

            var resolvedId = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
area: "Projects",
areaTitle: "",
controller: "ProjectTasks",
controllerTitle: "Tasks",
action: "ReadOnly",
actionTitle: "View",
routeValues: new Dictionary<string, object> { { "id", resolvedId } }
);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View("Edit");
        }
    }
}