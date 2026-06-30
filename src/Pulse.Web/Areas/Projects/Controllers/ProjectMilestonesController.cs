using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Projects.Controllers
{
    public class ProjectMilestonesController : Controller
    {
        // GET: Projects/ProjectMilestones
        public ActionResult Index()
        {
            return View();
        }
    }
}