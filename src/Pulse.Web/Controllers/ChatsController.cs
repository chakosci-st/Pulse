using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Controllers
{
    public class ChatsController : Controller
    {
        // GET: Chats
        public ActionResult Index()
        {

            var breadcrumbs = SharedUtilities.Helpers.BreadcrumbHelper.GenerateBreadcrumbs(
area: "",
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