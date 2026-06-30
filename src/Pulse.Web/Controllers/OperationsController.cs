using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Controllers
{
    public class OperationsController : Controller
    {
        // GET: Operations
        public ActionResult Index()
        {
            return RedirectToAction(
                 "Index",
                 "Home",
                 new { area = "Operations" });
        }
    }
}