using Pulse.Core.Entities;
using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Controllers
{
    public class PlantsController : Controller
    {
        // GET: Plants
        public ActionResult Index()
        {

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs("Plants", "Plants", "", "List");
            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Plants/New
        [Route("plant/new")]
        public ActionResult New()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs("Plants", "Plants", "New", "New");
            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        // GET: Plant/Edit/{code}
        [Route("plant/edit/{code}")]
        public ActionResult Edit(string code)
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs("Plants", "Plants", "Edit", "Edit");
            ViewBag.Breadcrumbs = breadcrumbs;
            return View();

        }

        // GET: Plant/Display/{code}
        [Route("plant/display/{code}")]
        public ActionResult Display(string code)
        {
            return View();
        }

        // GET: Plant/Edit
        public ActionResult Reports()
        {
            return View();
        }
    }
}