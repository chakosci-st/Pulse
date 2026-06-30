using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Api.Models;
using Pulse.SharedUtilities.Extensions;
using Pulse.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/notifications")]
    public class NotificationsController : ApiController
    {
        private readonly INotificationService _notificationService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;

        public class MarkNotificationsReadRequest
        {
            public List<string> NotificationIds { get; set; }
        }

        public NotificationsController(
            INotificationService notificationService,
            IProjectMemberRepository projectMemberRepository,
            IProjectTaskRepository projectTaskRepository,
            IProjectRepository projectRepository)
        {
            _notificationService = notificationService;
            _projectMemberRepository = projectMemberRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
        }

        [HttpPost]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFADD")]
        [Route("add")]
        public async Task<IHttpActionResult> Add([FromBody] NotificationSubmit model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Message))
            {
                return Ok(new
                {
                    Success = false,
                    Error = "Message text is required."
                });
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!await CanAddNotificationAsync(model))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var loggeduserFirstName = User.Identity.GetClaim("firstname");
            var loggeduserLastName = User.Identity.GetClaim("lastname");

            var obj = new Notification
            {
                Title = model.Title,
                Message = model.Message,
                Recipients = model.Recipients,
                NotificationDate = model.NotificationDate,
                ExpiryDate = model.ExpiryDate,
                CreatedBy = loggeduser,
                EntitySysId = model.EntitySysId,
                EntityType = model.EntityType,
                CreatedDate = DateTime.Now
            };

            var sysId = await _notificationService.AddAsync(obj);
            obj.NotificationSysId = sysId;

            var permissions = await GetNotificationPermissionsAsync(obj);

            return Ok(new
            {
                success = true,
                data = MapNotification(obj, permissions.canEdit, permissions.canDelete)
            });
        }

        [HttpPut]
        [Authorize]
        [Route("{id}")]
        public async Task<IHttpActionResult> Edit(string id, [FromBody] NotificationSubmit model)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Notification id is required.");
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest("Message text is required.");
            }

            var existing = await _notificationService.GetAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var existingPermissions = await GetNotificationPermissionsAsync(existing);
            if (!existingPermissions.canEdit)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            existing.Title = model.Title;
            existing.Message = model.Message;
            existing.Recipients = model.Recipients;
            existing.NotificationDate = model.NotificationDate;
            existing.ExpiryDate = model.ExpiryDate;
            existing.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _notificationService.EditAsync(existing);

            var updated = await _notificationService.GetAsync(id);
            var updatedPermissions = await GetNotificationPermissionsAsync(updated ?? existing);
            return Ok(new
            {
                success = true,
                data = updated == null ? null : MapNotification(updated, updatedPermissions.canEdit, updatedPermissions.canDelete)
            });
        }

        [HttpDelete]
        [Authorize]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Notification id is required.");
            }

            var existing = await _notificationService.GetAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var existingPermissions = await GetNotificationPermissionsAsync(existing);
            if (!existingPermissions.canDelete)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            await _notificationService.DeleteAsync(existing);

            return Ok(new
            {
                success = true,
                id = id
            });
        }

        [HttpGet]
        [Authorize]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFVIEW")]
        [Route("active/unread")]
        public async Task<IHttpActionResult> GetActiveUnread()
        {
            var loggeduser = User.Identity.GetClaim("employeeid");
            var notifications = (await _notificationService.GetActiveUnreadAsync(loggeduser) ?? Enumerable.Empty<Notification>()).ToList();
            var data = (await Task.WhenAll(notifications.Select(MapNotificationAsync)))
                .OrderByDescending(a => a.NotificationDate)
                .ThenByDescending(a => a.CreatedDate)
                .ToList();

            return Ok(new
            {
                unreadCount = data.Count,
                data = data
            });
        }

        [HttpGet]
        [Authorize]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFVIEW")]
        [Route("active/grouped")]
        public async Task<IHttpActionResult> GetActiveGrouped()
        {
            var loggeduser = User.Identity.GetClaim("employeeid");
            var notifications = (await _notificationService.GetActiveAsync(loggeduser) ?? Enumerable.Empty<Notification>()).ToList();
            var items = (await Task.WhenAll(notifications.Select(MapNotificationAsync)))
                .OrderByDescending(a => a.NotificationDate)
                .ThenByDescending(a => a.CreatedDate)
                .ToList();

            var sections = items
                .GroupBy(item => GetSectionKey(item.EntityType))
                .Select(group => new
                {
                    sectionKey = group.Key,
                    title = GetSectionTitle(group.Key),
                    description = GetSectionDescription(group.Key),
                    count = group.Count(),
                    groups = group
                        .GroupBy(item => string.IsNullOrWhiteSpace(item.ContextLabel) ? item.EntitySysId ?? "UNASSIGNED" : item.ContextLabel)
                        .Select(contextGroup => new
                        {
                            contextKey = contextGroup.Key,
                            contextLabel = contextGroup.First().ContextLabel,
                            count = contextGroup.Count(),
                            items = contextGroup.ToList()
                        })
                        .OrderByDescending(contextGroup => contextGroup.count)
                        .ThenBy(contextGroup => contextGroup.contextLabel)
                        .ToList()
                })
                .OrderBy(section => GetSectionOrder(section.sectionKey))
                .ToList();

            return Ok(new
            {
                totalCount = items.Count,
                unreadCount = items.Count(a => a.IsRead == false),
                archivedCount = items.Count(a => a.IsArchived),
                sections = sections
            });
        }

        [HttpPost]
        [Authorize]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFVIEW")]
        [Route("{id}/read")]
        public async Task<IHttpActionResult> MarkAsRead(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Notification id is required.");
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            await _notificationService.MarkAsReadAsync(loggeduser, id);

            return Ok(new
            {
                success = true,
                id = id
            });
        }

        [HttpPost]
        [Authorize]
        [Route("read-selected")]
        public async Task<IHttpActionResult> MarkSelectedAsRead([FromBody] MarkNotificationsReadRequest request)
        {
            var notificationIds = (request?.NotificationIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!notificationIds.Any())
            {
                return BadRequest("At least one notification id is required.");
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var updatedCount = await _notificationService.MarkAsReadAsync(loggeduser, notificationIds);

            return Ok(new
            {
                success = true,
                updatedCount = updatedCount,
                notificationIds = notificationIds
            });
        }

        [HttpPost]
        [Authorize]
        [Route("read-all")]
        public async Task<IHttpActionResult> MarkAllAsRead()
        {
            var loggeduser = User.Identity.GetClaim("employeeid");
            var unreadItems = (await _notificationService.GetActiveUnreadAsync(loggeduser) ?? Enumerable.Empty<Notification>())
                .Select(item => item.NotificationSysId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!unreadItems.Any())
            {
                return Ok(new
                {
                    success = true,
                    updatedCount = 0,
                    notificationIds = unreadItems
                });
            }

            var updatedCount = await _notificationService.MarkAsReadAsync(loggeduser, unreadItems);
            return Ok(new
            {
                success = true,
                updatedCount = updatedCount,
                notificationIds = unreadItems
            });
        }

        [HttpGet]
        [Authorize]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFVIEW")]
        [Route("item/{id}")]
        public async Task<IHttpActionResult> Get(string id)
        {

            var data = await _notificationService.GetAsync(id);
            var permissions = await GetNotificationPermissionsAsync(data);

            return Ok(new { data = data == null ? null : MapNotification(data, permissions.canEdit, permissions.canDelete) });
        }

        [HttpGet]
        [Authorize]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFVIEW")]
        [Route("project/{projectno}")]
        public async Task<IHttpActionResult> GetPerProject(string projectno)
        {

            var data = await _notificationService.GetByProjectAsync(projectno);
            var models = await Task.WhenAll((data ?? Enumerable.Empty<Notification>()).Select(MapNotificationAsync));

            return Ok(new { data = models });
        }

        [HttpGet]
        [Authorize]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "NOTIFVIEW")]
        [Route("entity/{entitytype}/{entitysysid}")]
        public async Task<IHttpActionResult> GetByEntities(string entitytype, string entitysysid)
        {

            var data = await _notificationService.GetByEntityAsync(entitytype, entitysysid);
            var models = await Task.WhenAll((data ?? Enumerable.Empty<Notification>()).Select(MapNotificationAsync));

            return Ok(new { data = models });
        }

        private async Task<NotificationItemModel> MapNotificationAsync(Notification notification)
        {
            var permissions = await GetNotificationPermissionsAsync(notification);
            var model = MapNotification(notification, permissions.canEdit, permissions.canDelete);

            if (model == null)
            {
                return null;
            }

            await TryEnrichTaskContextAsync(notification, model);
            return model;
        }

        private async Task TryEnrichTaskContextAsync(Notification notification, NotificationItemModel model)
        {
            if (notification == null || model == null)
            {
                return;
            }

            if (!string.Equals(model.EntityType, "TASK", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var meta = ParseMeta(notification.MetaJson);
            model.ProjectNo = FirstNonEmpty(
                model.ProjectNo,
                GetMetaValue(meta, "projectNo"),
                GetMetaValue(meta, "projectCode"),
                GetMetaValue(meta, "projectno"));
            model.ProjectName = FirstNonEmpty(
                model.ProjectName,
                GetMetaValue(meta, "projectName"),
                GetMetaValue(meta, "projectname"));
            model.ParentNodeName = FirstNonEmpty(
                model.ParentNodeName,
                GetMetaValue(meta, "parentNodeName"),
                GetMetaValue(meta, "parentName"),
                GetMetaValue(meta, "parentNode"));

            try
            {
                ProjectTask task = null;
                if (!string.IsNullOrWhiteSpace(notification.EntitySysId))
                {
                    task = await _projectTaskRepository.GetAsync(notification.EntitySysId);
                }

                if (task != null)
                {
                    model.ProjectNo = FirstNonEmpty(model.ProjectNo, task.ProjectNo);

                    if (string.IsNullOrWhiteSpace(model.ParentNodeName)
                        && !string.IsNullOrWhiteSpace(task.ProjectNo)
                        && !string.IsNullOrWhiteSpace(task.ParentType)
                        && !string.IsNullOrWhiteSpace(task.ParentSysId))
                    {
                        var parent = await _projectRepository.GetProjectNodeItemAsync(
                            task.ProjectNo,
                            NormalizeNodeType(task.ParentType),
                            task.ParentSysId);

                        model.ParentNodeName = FirstNonEmpty(model.ParentNodeName, parent?.NodeName);
                    }
                }

                if (string.IsNullOrWhiteSpace(model.ProjectName) && !string.IsNullOrWhiteSpace(model.ProjectNo))
                {
                    var project = await _projectRepository.GetAsync(model.ProjectNo);
                    model.ProjectName = FirstNonEmpty(model.ProjectName, project?.ProjectName);
                }
            }
            catch
            {
                // Keep notifications available even if optional task-context lookups fail.
            }
        }

        private async Task<(bool canEdit, bool canDelete)> GetNotificationPermissionsAsync(Notification notification)
        {
            if (notification == null)
            {
                return (false, false);
            }

            if (string.Equals((notification.EntityType ?? string.Empty).Trim(), "PROJECT", StringComparison.OrdinalIgnoreCase))
            {
                var isMember = await IsProjectMemberAsync(notification.EntitySysId);
                return (isMember, isMember);
            }

            return (CanEditNotifications(), CanDeleteNotifications());
        }

        private async Task<bool> CanAddNotificationAsync(NotificationSubmit model)
        {
            if (model == null)
            {
                return false;
            }

            if (string.Equals((model.EntityType ?? string.Empty).Trim(), "PROJECT", StringComparison.OrdinalIgnoreCase))
            {
                return await IsProjectMemberAsync(model.EntitySysId);
            }

            return CanAddNotifications();
        }

        private async Task<bool> IsProjectMemberAsync(string projectNo)
        {
            var loggedUserId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(loggedUserId) || string.IsNullOrWhiteSpace(projectNo))
            {
                return false;
            }

            var members = await _projectMemberRepository.GetListAsync(projectNo);
            return (members ?? Enumerable.Empty<ProjectMember>())
                .Any(member => member != null && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase));
        }

        private bool CanAddNotifications()
        {
            var moduleCodes = User.Identity.GetClaim("modulecodes") ?? string.Empty;
            return moduleCodes
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(code => string.Equals(code.Trim(), "NOTIFADD", StringComparison.OrdinalIgnoreCase));
        }

        private NotificationItemModel MapNotification(Notification notification, bool canEdit, bool canDelete)
        {
            var meta = ParseMeta(notification.MetaJson);
            var entityType = (notification.EntityType ?? string.Empty).Trim().ToUpperInvariant();
            var contextLabel = GetMetaValue(meta, "context");
            return new NotificationItemModel
            {
                NotificationSysId = notification.NotificationSysId,
                EntityType = entityType,
                EntitySysId = notification.EntitySysId,
                Title = string.IsNullOrWhiteSpace(notification.Title) ? "Notification" : notification.Title,
                Message = notification.Message,
                Recipients = notification.Recipients,
                ContextLabel = string.IsNullOrWhiteSpace(contextLabel) ? BuildFallbackContextLabel(entityType, notification.EntitySysId) : contextLabel,
                ProjectNo = GetMetaValue(meta, "projectNo"),
                ProjectName = GetMetaValue(meta, "projectName"),
                ParentNodeName = GetMetaValue(meta, "parentNodeName"),
                CreatedBy = notification.CreatedBy,
                CreatedByDisplayName = BuildDisplayName(GetMetaValue(meta, "createdFirstName"), GetMetaValue(meta, "createdLastName"), notification.CreatedBy),
                CreatedDate = notification.CreatedDate,
                NotificationDate = notification.NotificationDate,
                ExpiryDate = notification.ExpiryDate,
                IsRead = notification.IsViewed == 1,
                IsArchived = notification.Archived == 1,
                CanManage = canEdit || canDelete,
                CanEdit = canEdit,
                CanDelete = canDelete
            };
        }

        private bool CanEditNotifications()
        {
            var moduleCodes = User.Identity.GetClaim("modulecodes") ?? string.Empty;
            return moduleCodes
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(code => string.Equals(code.Trim(), "NOTIFEDIT", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(code.Trim(), "NOTIFADD", StringComparison.OrdinalIgnoreCase));
        }

        private bool CanDeleteNotifications()
        {
            var moduleCodes = User.Identity.GetClaim("modulecodes") ?? string.Empty;
            return moduleCodes
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(code => string.Equals(code.Trim(), "NOTIFDEL", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(code.Trim(), "NOTIFADD", StringComparison.OrdinalIgnoreCase));
        }

        private static JObject ParseMeta(string metaJson)
        {
            if (string.IsNullOrWhiteSpace(metaJson))
            {
                return new JObject();
            }

            try
            {
                var parsed = JObject.Parse(metaJson);
                return parsed["meta"] as JObject ?? parsed;
            }
            catch
            {
                return new JObject();
            }
        }

        private static string GetMetaValue(JObject meta, string propertyName)
        {
            return meta?[propertyName]?.ToString();
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        private static string NormalizeNodeType(string nodeType)
        {
            var normalized = (nodeType ?? string.Empty).Trim().ToUpperInvariant();
            switch (normalized)
            {
                case "MILESTONE": return "milestone";
                case "TASK":
                case "ACTIVITY": return "task";
                default: return string.IsNullOrWhiteSpace(nodeType) ? "task" : nodeType.Trim().ToLowerInvariant();
            }
        }

        private static string BuildDisplayName(string firstName, string lastName, string fallback)
        {
            var fullName = string.Join(" ", new[] { firstName, lastName }.Where(a => !string.IsNullOrWhiteSpace(a))).Trim();
            return string.IsNullOrWhiteSpace(fullName) ? (fallback ?? string.Empty) : fullName;
        }

        private static string BuildFallbackContextLabel(string entityType, string entitySysId)
        {
            if (string.Equals(entityType, "SYSTEM", StringComparison.OrdinalIgnoreCase))
            {
                return "Pulse";
            }

            if (string.IsNullOrWhiteSpace(entitySysId))
            {
                return string.IsNullOrWhiteSpace(entityType) ? "General" : entityType;
            }

            return entitySysId;
        }

        private static string GetSectionKey(string entityType)
        {
            var normalized = (entityType ?? string.Empty).Trim().ToUpperInvariant();
            switch (normalized)
            {
                case "PLANT": return "PLANT";
                case "PROJECT": return "PROJECT";
                case "MILESTONE": return "MILESTONE";
                case "TASK": return "TASK";
                case "SYSTEM": return "SYSTEM";
                default: return "OTHER";
            }
        }

        private static int GetSectionOrder(string sectionKey)
        {
            switch (sectionKey)
            {
                case "PLANT": return 1;
                case "PROJECT": return 2;
                case "MILESTONE": return 3;
                case "TASK": return 4;
                case "SYSTEM": return 5;
                default: return 6;
            }
        }

        private static string GetSectionTitle(string sectionKey)
        {
            switch (sectionKey)
            {
                case "PLANT": return "Plants";
                case "PROJECT": return "Projects";
                case "MILESTONE": return "Milestones";
                case "TASK": return "Tasks";
                case "SYSTEM": return "System";
                default: return "Other";
            }
        }

        private static string GetSectionDescription(string sectionKey)
        {
            switch (sectionKey)
            {
                case "PLANT": return "Notifications grouped by the plants you can access.";
                case "PROJECT": return "Notifications grouped by your allowed projects.";
                case "MILESTONE": return "Milestone-level updates currently active for you.";
                case "TASK": return "Task-level alerts and reminders across your work.";
                case "SYSTEM": return "System-wide notices visible to your account.";
                default: return "Other notification contexts available to you.";
            }
        }
    }
}
