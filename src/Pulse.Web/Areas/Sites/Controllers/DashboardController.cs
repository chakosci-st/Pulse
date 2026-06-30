using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Sites.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Site/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}