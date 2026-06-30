using Pulse.SharedUtilities.Helpers;
using Pulse.Core.Interfaces;
using Pulse.Web.Helpers;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Areas.Projects.Controllers
{ 
 
    public class ProjectsController : Controller
    { 
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectRepository _projectRepository;

        public ProjectsController(IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository)
        {
            _projectMemberRepository = projectMemberRepository;
            _projectRepository = projectRepository;
        }

        private string GetLoggedUserId()
        {
            return (User as ClaimsPrincipal)?.Claims.FirstOrDefault(c => c.Type == "employeeid")?.Value;
        }

        private async System.Threading.Tasks.Task ApplyProjectAccessViewBagsAsync(string projectno)
        {
            var loggedUserId = GetLoggedUserId();
            var members = string.IsNullOrWhiteSpace(projectno)
                ? Enumerable.Empty<Pulse.Core.Entities.ProjectMember>()
                : await _projectMemberRepository.GetListAsync(projectno);
            var project = string.IsNullOrWhiteSpace(projectno) ? null : await _projectRepository.GetAsync(projectno);
            var isOwnerMember = members.Any(member => member != null
                && member.IsOwner == 1
                && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase));

            ViewBag.CanManageProjectNotifications = members.Any(member => member != null
                && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase));
            ViewBag.IsProjectOwnerMember = isOwnerMember || (project != null && string.Equals(project.ProjectOwnerId, loggedUserId, StringComparison.OrdinalIgnoreCase));
            ViewBag.IsPrimaryProjectOwner = project != null && string.Equals(project.ProjectOwnerId, loggedUserId, StringComparison.OrdinalIgnoreCase);
        }

        // GET: Projects/Projects
        [Filters.AuthorizeProjectAccess(Mode = Filters.ProjectAccessMode.AnyMembership)]
        public ActionResult Index()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
    area: "Projects",
    areaTitle: "",
    controller: "Projects",
    controllerTitle: "Projects",
    action: "Index",
    actionTitle: "Index",
    routeValues: new Dictionary<string, object> { }
);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeProjectAccess(Mode = Filters.ProjectAccessMode.AnyMembership)]
        public ActionResult StatusBoard()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Projects",
                areaTitle: "",
                controller: "Projects",
                controllerTitle: "Projects",
                action: "StatusBoard",
                actionTitle: "Status Board",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }
         
        [Filters.AuthorizeProjectAccess(Mode = Filters.ProjectAccessMode.PlantMemberOnly)]
        public ActionResult Create()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Projects",
                areaTitle: "",
                controller: "Projects",
                controllerTitle: "Projects",
                action: "Create",
                actionTitle: "Register",
                routeValues: new Dictionary<string, object> { }
            );


            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

          
        [Filters.AuthorizeProjectAccess(Mode = Filters.ProjectAccessMode.ProjectMemberOnly)]
        public async Task<ActionResult> Review(string projectno)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, projectno, "projectno");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedProjectNo = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Projects",
                areaTitle: "",
                controller: "Projects",
                controllerTitle: "Projects",
                action: "Review",
                actionTitle: "Review",
                routeValues: new Dictionary<string, object> { { "projectno", resolvedProjectNo } }
            );


            ViewBag.ProjectNo = resolvedProjectNo;
            await ApplyProjectAccessViewBagsAsync(resolvedProjectNo);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }

        [Filters.AuthorizeProjectAccess(Mode = Filters.ProjectAccessMode.PlantMemberOrProjectMember)]
        public async Task<ActionResult> Details(string projectno)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, projectno, "projectno");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedProjectNo = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Projects",
                areaTitle: "",
                controller: "Projects",
                controllerTitle: "Projects",
                action: "Details",
                actionTitle: "Details",
                routeValues: new Dictionary<string, object> { { "projectno", resolvedProjectNo } }
            );

            ViewBag.ProjectNo = resolvedProjectNo;
            await ApplyProjectAccessViewBagsAsync(resolvedProjectNo);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


        [Filters.AuthorizeProjectAccess(Mode = Filters.ProjectAccessMode.ProjectMemberOnly)]
        public async Task<ActionResult> Configure(string projectno)
        {
            var redirectResult = PageRouteValueHelper.ResolveStringRouteValue(this, projectno, "projectno");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var resolvedProjectNo = ViewBag.Id as string;
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Projects",
                areaTitle: "",
                controller: "Projects",
                controllerTitle: "Projects",
                action: "Configure",
                actionTitle: "Configure",
                routeValues: new Dictionary<string, object> { { "projectno", resolvedProjectNo } }
            );

            ViewBag.ProjectNo = resolvedProjectNo;
            await ApplyProjectAccessViewBagsAsync(resolvedProjectNo);

            ViewBag.Breadcrumbs = breadcrumbs;
            return View();
        }


        [HttpGet]
        public ActionResult DrawerUpdateProjectPartial()
        { 
            return PartialView("_DrawerUpdateProjectPartial");
        }
    }
}