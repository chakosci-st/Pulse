using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.SharedUtilities.Extensions;
using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Areas.Settings.Models;

namespace Pulse.Web.Areas.Settings.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly INotificationService _notificationService;
        private readonly IProjectMilestoneRepository _projectMilestoneRepository;

        public DashboardController(
            IUserService userService,
            IProjectTaskService projectTaskService,
            INotificationService notificationService,
            IProjectMilestoneRepository projectMilestoneRepository)
        {
            _userService = userService;
            _projectTaskService = projectTaskService;
            _notificationService = notificationService;
            _projectMilestoneRepository = projectMilestoneRepository;
        }

        // GET: Settings/Dashboard
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            var userId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return HttpNotFound();
            }

            var userTask = _userService.GetUserByIdAsync(userId);
            var userGroupsTask = _userService.GetUserGroupsAsync(userId);
            var assignedTasksTask = _projectTaskService.GetItemListAsync(userId);
            var notificationsTask = _notificationService.GetActiveAsync(userId);

            await System.Threading.Tasks.Task.WhenAll(userTask, userGroupsTask, assignedTasksTask, notificationsTask);

            var user = await userTask;
            if (user == null)
            {
                return HttpNotFound();
            }

            var userGroups = (await userGroupsTask ?? Enumerable.Empty<UserGroupMember>())
                .Where(group => group != null)
                .Select(group => group.UserGroup != null && !string.IsNullOrWhiteSpace(group.UserGroup.UserGroupName)
                    ? group.UserGroup.UserGroupName
                    : (group.UserGroupId.HasValue ? $"User Group #{group.UserGroupId.Value}" : null))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            var assignedTasks = (await assignedTasksTask ?? Enumerable.Empty<ProjectTaskItem>())
                .Where(task => task != null)
                .ToList();

            var closedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "COMPLETED",
                "FAILED",
                "CANCEL",
                "CANCELED",
                "CANCELLED"
            };

            var pendingTasks = assignedTasks
                .Where(task => !closedStatuses.Contains((task.Status ?? string.Empty).Trim()))
                .ToList();

            var notifications = (await notificationsTask ?? Enumerable.Empty<Notification>())
                .Where(notification => notification != null)
                .OrderByDescending(notification => notification.NotificationDate == default(DateTime)
                    ? notification.CreatedDate
                    : notification.NotificationDate)
                .ToList();

            var recentActivities = await BuildRecentActivitiesAsync(notifications.Take(5).ToList());

            var dueSoonTasks = pendingTasks
                .OrderBy(task => task.TargetCompletionDate ?? DateTime.MaxValue)
                .ThenBy(task => task.ProjectName)
                .ThenBy(task => task.ActivityName)
                .Take(5)
                .Select(task => new DashboardTaskSummaryViewModel
                {
                    ProjectTaskSysId = task.ProjectTaskSysId,
                    ProjectNo = task.ProjectNo,
                    ProjectName = task.ProjectName,
                    ActivityName = task.ActivityName,
                    Status = string.IsNullOrWhiteSpace(task.Status) ? "Unknown" : task.Status,
                    TargetCompletionDate = task.TargetCompletionDate,
                    IsOverdue = task.TargetCompletionDate.HasValue && task.TargetCompletionDate.Value.Date < DateTime.Today
                })
                .ToList();

            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))).Trim();

            var initials = string.Join(string.Empty, new[] { user.FirstName, user.LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Substring(0, 1).ToUpperInvariant()));

            var viewModel = new DashboardViewModel
            {
                User = user,
                FullName = string.IsNullOrWhiteSpace(fullName) ? user.UserId : fullName,
                Initials = string.IsNullOrWhiteSpace(initials) ? "?" : initials,
                PendingTaskCount = pendingTasks.Count,
                CompletedTaskCount = assignedTasks.Count(task => string.Equals(task.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)),
                ActiveNotificationCount = notifications.Count,
                UnreadNotificationCount = notifications.Count(notification => notification.IsViewed == 0),
                ProjectCount = assignedTasks.Select(task => task.ProjectNo).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                UserGroupCount = userGroups.Count,
                UserGroups = userGroups,
                DueSoonTasks = dueSoonTasks,
                RecentActivities = recentActivities
            };

            ViewBag.Title = "Settings";
            ViewBag.TitleIcon = "bi bi-sliders";
            ViewBag.SubTitle = "Personal dashboard, quick links, and user-level activity";
            ViewBag.Breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Settings",
                areaTitle: "Settings",
                controller: "Dashboard",
                controllerTitle: "Dashboard",
                action: "Index",
                actionTitle: "Overview",
                routeValues: new Dictionary<string, object>());

            return View(viewModel);
        }

        private async System.Threading.Tasks.Task<List<ProfileRecentActivityViewModel>> BuildRecentActivitiesAsync(IEnumerable<Notification> notifications)
        {
            var notificationList = (notifications ?? Enumerable.Empty<Notification>())
                .Where(notification => notification != null)
                .ToList();

            var milestoneIds = notificationList
                .Where(notification => string.Equals(notification.EntityType, "milestone", StringComparison.OrdinalIgnoreCase))
                .Select(notification => notification.EntitySysId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var milestoneLookups = await LoadProjectMilestoneMapAsync(milestoneIds);

            return notificationList
                .Select(notification =>
                {
                    var target = BuildActivityTarget(notification, milestoneLookups);
                    return new ProfileRecentActivityViewModel
                    {
                        Title = string.IsNullOrWhiteSpace(notification.Title) ? "Pulse update" : notification.Title,
                        Message = notification.Message,
                        Context = ParseContextLabel(notification),
                        TargetUrl = target.url,
                        TargetLabel = target.label,
                        CreatedBy = notification.CreatedBy,
                        OccurredAt = notification.NotificationDate == default(DateTime)
                            ? notification.CreatedDate
                            : notification.NotificationDate,
                        IsUnread = notification.IsViewed == 0
                    };
                })
                .ToList();
        }

        private async System.Threading.Tasks.Task<Dictionary<string, ProjectMilestone>> LoadProjectMilestoneMapAsync(IEnumerable<string> milestoneIds)
        {
            var ids = (milestoneIds ?? Enumerable.Empty<string>()).ToList();
            var milestones = await System.Threading.Tasks.Task.WhenAll(ids.Select(id => _projectMilestoneRepository.GetAsync(id)));

            return milestones
                .Where(milestone => milestone != null && !string.IsNullOrWhiteSpace(milestone.MilestoneSysId))
                .GroupBy(milestone => milestone.MilestoneSysId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }

        private static (string url, string label) BuildActivityTarget(
            Notification notification,
            IReadOnlyDictionary<string, ProjectMilestone> milestones)
        {
            var entityType = (notification.EntityType ?? string.Empty).Trim();
            var entitySysId = (notification.EntitySysId ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entitySysId))
            {
                return ($"/Home/Notifications#notification-{HttpUtility.UrlEncode(notification.NotificationSysId ?? string.Empty)}", "Open notification");
            }

            if (string.Equals(entityType, "plant", StringComparison.OrdinalIgnoreCase))
            {
                return ($"/Templates/Plants/Display/{HttpUtility.UrlEncode(entitySysId)}", "Open plant");
            }

            if (string.Equals(entityType, "project", StringComparison.OrdinalIgnoreCase))
            {
                return ($"/Projects/Details/{HttpUtility.UrlEncode(entitySysId)}", "Open project");
            }

            if (string.Equals(entityType, "task", StringComparison.OrdinalIgnoreCase))
            {
                return ($"/Projects/ProjectTasks/Edit/{HttpUtility.UrlEncode(entitySysId)}", "Open task");
            }

            if (string.Equals(entityType, "milestone", StringComparison.OrdinalIgnoreCase) && milestones.TryGetValue(entitySysId, out var milestone) && milestone != null && !string.IsNullOrWhiteSpace(milestone.ProjectNo))
            {
                return ($"/Projects/Overview/{HttpUtility.UrlEncode(milestone.ProjectNo)}", "Open project");
            }

            return ($"/Home/Notifications#notification-{HttpUtility.UrlEncode(notification.NotificationSysId ?? string.Empty)}", "Open notification");
        }

        private static string ParseContextLabel(Notification notification)
        {
            if (!string.IsNullOrWhiteSpace(notification.MetaJson))
            {
                try
                {
                    var payload = JObject.Parse(notification.MetaJson);
                    var context = payload.SelectToken("meta.context")?.ToString();
                    if (!string.IsNullOrWhiteSpace(context))
                    {
                        return context;
                    }
                }
                catch
                {
                }
            }

            return ToDisplayLabel(notification.EntityType);
        }

        private static string ToDisplayLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "System";
            }

            var cleanedValue = value.Replace("_", " ").Replace("-", " ").Trim();
            return char.ToUpperInvariant(cleanedValue[0]) + cleanedValue.Substring(1);
        }
    }
}