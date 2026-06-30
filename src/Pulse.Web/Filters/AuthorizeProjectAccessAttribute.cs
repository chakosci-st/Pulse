using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Filters
{
    public enum ProjectAccessMode
    {
        AnyMembership,
        PlantMemberOnly,
        PlantMemberOrProjectMember,
        ProjectMemberOnly
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeProjectAccessAttribute : AuthorizeAttribute
    {
        public ProjectAccessMode Mode { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                return false;
            }

            var user = httpContext.User as ClaimsPrincipal;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            var loggedUserId = user.Claims.FirstOrDefault(c => c.Type == "employeeid")?.Value;
            if (string.IsNullOrWhiteSpace(loggedUserId))
            {
                return false;
            }

            var projectRepository = DependencyResolver.Current.GetService<IProjectRepository>();
            var plantMemberRepository = DependencyResolver.Current.GetService<IPlantMemberRepository>();
            var projectMemberRepository = DependencyResolver.Current.GetService<IProjectMemberRepository>();
            if (projectRepository == null || plantMemberRepository == null || projectMemberRepository == null)
            {
                return false;
            }

            switch (Mode)
            {
                case ProjectAccessMode.AnyMembership:
                    return HasAnyMembership(plantMemberRepository, projectMemberRepository, loggedUserId);
                case ProjectAccessMode.PlantMemberOnly:
                    return HasActivePlantMembership(plantMemberRepository, loggedUserId);
                case ProjectAccessMode.PlantMemberOrProjectMember:
                    return HasProjectAccess(projectRepository, plantMemberRepository, projectMemberRepository, ResolveProjectNo(httpContext), loggedUserId, true);
                case ProjectAccessMode.ProjectMemberOnly:
                    return HasProjectAccess(projectRepository, plantMemberRepository, projectMemberRepository, ResolveProjectNo(httpContext), loggedUserId, false);
                default:
                    return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext != null
                && RequiresProjectNumber()
                && string.IsNullOrWhiteSpace(ResolveProjectNo(filterContext.HttpContext)))
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                {
                    ["area"] = "Projects",
                    ["controller"] = "Projects",
                    ["action"] = "Index"
                });
                return;
            }

            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You are not authorized for this project resource.");
        }

        private bool RequiresProjectNumber()
        {
            return Mode == ProjectAccessMode.PlantMemberOrProjectMember
                || Mode == ProjectAccessMode.ProjectMemberOnly;
        }

        private static string ResolveProjectNo(HttpContextBase httpContext)
        {
            var routeValue = httpContext.Request.RequestContext.RouteData.Values["projectno"] as string;
            if (!string.IsNullOrWhiteSpace(routeValue))
            {
                return routeValue;
            }

            var queryValue = httpContext.Request.QueryString["projectno"];
            if (!string.IsNullOrWhiteSpace(queryValue))
            {
                return queryValue;
            }

            return httpContext.Request["projectno"];
        }

        private static bool HasAnyMembership(IPlantMemberRepository plantMemberRepository, IProjectMemberRepository projectMemberRepository, string loggedUserId)
        {
            return HasActivePlantMembership(plantMemberRepository, loggedUserId)
                || projectMemberRepository.GetByMemberIdAsync(loggedUserId).GetAwaiter().GetResult().Any();
        }

        private static bool HasActivePlantMembership(IPlantMemberRepository plantMemberRepository, string loggedUserId, string plantCode = null)
        {
            return plantMemberRepository
                .GetListAsync(plantCode, loggedUserId)
                .GetAwaiter()
                .GetResult()
                .Any(member => member != null && member.IsActive == 1);
        }

        private static bool HasProjectAccess(
            IProjectRepository projectRepository,
            IPlantMemberRepository plantMemberRepository,
            IProjectMemberRepository projectMemberRepository,
            string projectNo,
            string loggedUserId,
            bool allowPlantMembers)
        {
            if (string.IsNullOrWhiteSpace(projectNo))
            {
                return false;
            }

            var project = projectRepository.GetAsync(projectNo).GetAwaiter().GetResult();
            if (project == null)
            {
                return false;
            }

            if (string.Equals(project.ProjectOwnerId, loggedUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var isProjectMember = projectMemberRepository
                .GetListAsync(projectNo)
                .GetAwaiter()
                .GetResult()
                .Any(member => member != null && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase));

            if (isProjectMember)
            {
                return true;
            }

            return allowPlantMembers && HasActivePlantMembership(plantMemberRepository, loggedUserId, project.PlantCode);
        }
    }
}