using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Projects.Controllers
{
    public class ProjectMembersController : Controller
    {
        // GET: Projects/ProjectMembers
        public ActionResult Index()
        {
            return View();
        }
    }
}