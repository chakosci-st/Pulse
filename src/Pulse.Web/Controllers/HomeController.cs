using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Pulse.SharedUtilities.Helpers;

namespace Pulse.Web.Controllers
{ 
    public class HomeController : Controller
    {
        private sealed class SearchModuleEntry
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string Keywords { get; set; }
        }

        public ActionResult Index()
        {
            return View();
        }

        private IReadOnlyList<SearchModuleEntry> BuildSearchModules()
        {
            var moduleCodes = ((User as ClaimsPrincipal)?.Claims.FirstOrDefault(c => c.Type == "modulecodes")?.Value ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool HasModule(params string[] requiredCodes)
            {
                return requiredCodes.Any(code => moduleCodes.Contains(code));
            }

            var modules = new List<SearchModuleEntry>
            {
                new SearchModuleEntry
                {
                    Title = "Help Center",
                    Description = "FAQ, user guide, and common workflows.",
                    Url = Url.Action("Guide", "Home", new { area = "" }),
                    Keywords = "help faq guide support documentation user guide"
                },
                new SearchModuleEntry
                {
                    Title = "Recent Changes",
                    Description = "Curated release highlights for the latest Pulse updates.",
                    Url = Url.Action("RecentChanges", "Home", new { area = "" }),
                    Keywords = "recent changes release notes updates whats new changelog"
                },
                new SearchModuleEntry
                {
                    Title = "Project View",
                    Description = "Project cards with task health snapshots.",
                    Url = Url.Action("ViewProjects", "Home", new { area = "" }),
                    Keywords = "project cards overview dashboard tasks project view"
                },
                new SearchModuleEntry
                {
                    Title = "Project Export",
                    Description = "Export project records to an Excel-ready file.",
                    Url = Url.Action("Report1", "Home", new { area = "" }),
                    Keywords = "project export excel records extraction download report"
                },
                new SearchModuleEntry
                {
                    Title = "Monitoring Matrix",
                    Description = "Milestone and task tracking matrix with DMS values.",
                    Url = Url.Action("Report2", "Home", new { area = "" }),
                    Keywords = "monitoring matrix milestones tasks dms tracker report"
                },
                new SearchModuleEntry
                {
                    Title = "Project Comparison",
                    Description = "Compare project timelines, milestones, tasks, and form values side by side.",
                    Url = Url.Action("Report3", "Home", new { area = "" }),
                    Keywords = "project comparison timeline compare milestones tasks forms report"
                }
            };

            if (HasModule("CALNDRVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Calendar View",
                    Description = "Browse generated production calendars in calendar view.",
                    Url = Url.Action("Reports", "ProductionCalendars", new { area = "Templates" }),
                    Keywords = "calendar production calendar template templates year workweek calndrview"
                });
            }

            if (HasModule("CATGRYVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Category List",
                    Description = "Browse template categories available in Pulse.",
                    Url = Url.Action("Index", "Categories", new { area = "Templates" }),
                    Keywords = "category categories template templates list catgryview"
                });
            }

            if (HasModule("FORMVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Form List",
                    Description = "Browse reusable forms for project workflows.",
                    Url = Url.Action("Index", "Forms", new { area = "Templates" }),
                    Keywords = "form forms template templates list formview"
                });
            }

            if (HasModule("MATVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Maturity List",
                    Description = "Browse project maturity templates and levels.",
                    Url = Url.Action("Index", "MaturityLevels", new { area = "Templates" }),
                    Keywords = "maturity template templates list matview"
                });
            }

            if (HasModule("MODULEVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Module List",
                    Description = "Browse admin modules and access definitions.",
                    Url = Url.Action("Index", "Modules", new { area = "Admin" }),
                    Keywords = "module modules admin user access list moduleview users"
                });
            }

            if (HasModule("NOTIFVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Notification View",
                    Description = "Review notifications available to your plants and projects.",
                    Url = Url.Action("Notifications", "Home", new { area = "" }),
                    Keywords = "notifications notification view alerts notifview"
                });
            }

            if (HasModule("PDIVVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Product Division List",
                    Description = "Browse product division templates.",
                    Url = Url.Action("Index", "ProductDivisions", new { area = "Templates" }),
                    Keywords = "product division divisions template templates pdivview"
                });
            }

            if (HasModule("PGRPVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Product Group List",
                    Description = "Browse product group templates.",
                    Url = Url.Action("Index", "ProductGroups", new { area = "Templates" }),
                    Keywords = "product group groups template templates pgrpview"
                });
            }

            if (HasModule("PLANTVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Plant List",
                    Description = "Browse plants and plant-level template configuration.",
                    Url = Url.Action("Index", "Plants", new { area = "Templates" }),
                    Keywords = "plant plants template templates plantview"
                });
            }

            if (HasModule("RMAPVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "Roadmap List",
                    Description = "Browse roadmap templates and milestones.",
                    Url = Url.Action("Index", "Roadmaps", new { area = "Templates" }),
                    Keywords = "roadmap roadmaps template templates rmapview"
                });
            }

            if (HasModule("USRGRPVIEW"))
            {
                modules.Add(new SearchModuleEntry
                {
                    Title = "User Group List",
                    Description = "Browse user groups and admin access assignments.",
                    Url = Url.Action("Index", "UserGroups", new { area = "Admin" }),
                    Keywords = "user users user group user groups admin usrgrpview"
                });
            }

            return modules;
        }
         
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        } 
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Guide()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Help Center",
                action: "Guide",
                actionTitle: "FAQ",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Pulse FAQ";
            ViewBag.TitleIcon = "bi bi-question-circle-fill";
            ViewBag.SubTitle = "Quick answers for common Pulse questions, with links to the full user guide.";
            ViewBag.IsActiveFAQ = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult UserGuide()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Help Center",
                action: "UserGuide",
                actionTitle: "User Guide",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Pulse User Guide";
            ViewBag.TitleIcon = "bi bi-journal-bookmark-fill";
            ViewBag.SubTitle = "A full walkthrough of Pulse features, user responsibilities, and proper ways of working.";
            ViewBag.IsActiveUserGuide = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult RecentChanges()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Recent Changes",
                action: "RecentChanges",
                actionTitle: "Release Highlights",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Recent Changes";
            ViewBag.TitleIcon = "bi bi-clock-history";
            ViewBag.SubTitle = "Curated highlights for the latest Pulse updates across search, reporting, project access, and production calendars.";
            ViewBag.IsActiveFAQ = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult ViewProjects()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "View",
                action: "ViewProjects",
                actionTitle: "Project Cards",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Project View";
            ViewBag.SubTitle = "Project cards with task health snapshots.";
            ViewBag.IsActiveView = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult Report1()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Reports",
                action: "Report1",
                actionTitle: "Project Export",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Project Export";
            ViewBag.SubTitle = "Export project records into an Excel-ready file.";
            ViewBag.IsActiveReports = "menu-open";
            ViewBag.IsActiveReport1 = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult Report2()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Reports",
                action: "Report2",
                actionTitle: "Monitoring Matrix",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Monitoring Matrix";
            ViewBag.SubTitle = "Project monitoring matrix with milestone and task DMS tracking.";
            ViewBag.IsActiveReports = "menu-open";
            ViewBag.IsActiveReport2 = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult Report3()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Reports",
                action: "Report3",
                actionTitle: "Project Comparison",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Project Comparison";
            ViewBag.SubTitle = "Compare multiple project timelines, milestones, tasks, and form values side by side.";
            ViewBag.IsActiveReports = "menu-open";
            ViewBag.IsActiveReport3 = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        public ActionResult Search(string q)
        {
            var query = (q ?? string.Empty).Trim();
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Search",
                action: "Search",
                actionTitle: "Global Search",
                routeValues: new Dictionary<string, object> { { "q", query } }
            );

            ViewBag.Title = "Search";
            ViewBag.SubTitle = string.IsNullOrWhiteSpace(query)
                ? "Search across projects, milestones, tasks, and only the modules you can view."
                : $"Results for \"{query}\".";
            ViewBag.SearchQuery = query;
            ViewBag.SearchModules = BuildSearchModules();
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }

        //[Filters.AuthorizeUserGroup(Groups = "", Modules = "NOTIFVIEW")]
        public ActionResult Notifications()
        {
            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "",
                areaTitle: "",
                controller: "Home",
                controllerTitle: "Notifications",
                action: "Notifications",
                actionTitle: "Notification Center",
                routeValues: new Dictionary<string, object> { }
            );

            ViewBag.Title = "Notifications";
            ViewBag.TitleIcon = "bi bi-bell-fill";
            ViewBag.SubTitle = "Active notifications grouped by the plants, projects, milestones, and tasks available to you.";
            ViewBag.IsActiveNotifications = "active";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View();
        }
    }
}