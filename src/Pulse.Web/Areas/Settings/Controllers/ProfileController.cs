using System;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.SharedUtilities.Extensions;
using Pulse.SharedUtilities.Helpers;
using Pulse.Web.Areas.Settings.Models;
using Newtonsoft.Json.Linq;

namespace Pulse.Web.Areas.Settings.Controllers
{
    public class ProfileController : Controller
    {
        private const int ProfilePhotoSize = 256;
        private const string DefaultProfilePhotoAllowedExtensions = ".jpg,.jpeg,.png,.gif,.bmp,.webp";
        private const int DefaultProfilePhotoMaxFileSizeMb = 5;

        private readonly IUserService _userService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly IProjectMilestoneRepository _projectMilestoneRepository;

        public ProfileController(
            IUserService userService,
            IProjectTaskService projectTaskService,
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            INotificationService notificationService,
            IProjectMilestoneRepository projectMilestoneRepository)
        {
            _userService = userService;
            _projectTaskService = projectTaskService;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
            _projectMilestoneRepository = projectMilestoneRepository;
        }

        // GET: Settings/Profile
        public async System.Threading.Tasks.Task<ActionResult> Index(string id)
        {
            var userId = string.IsNullOrWhiteSpace(id)
                ? User.Identity.GetClaim("employeeid")
                : id;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return HttpNotFound();
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userGroupsTask = _userService.GetUserGroupsAsync(user.UserId);
            var assignedTasksTask = _projectTaskService.GetItemListAsync(user.UserId);
            var projectsTask = _projectRepository.GetListAsync();
            var projectMembershipsTask = _projectMemberRepository.GetByMemberIdAsync(user.UserId);
            var notificationsTask = _notificationService.GetActiveAsync(user.UserId);

            await System.Threading.Tasks.Task.WhenAll(userGroupsTask, assignedTasksTask, projectsTask, projectMembershipsTask, notificationsTask);

            var userGroups = await userGroupsTask;
            var userGroupNames = (userGroups ?? Enumerable.Empty<UserGroupMember>())
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

            var pendingTaskCount = assignedTasks.Count(task => !closedStatuses.Contains((task.Status ?? string.Empty).Trim()));
            var completedTaskCount = assignedTasks.Count(task => string.Equals(task.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase));

            var ownedProjectCount = (await projectsTask ?? Enumerable.Empty<Project>())
                .Count(project => project != null && string.Equals(project.ProjectOwnerId, user.UserId, StringComparison.OrdinalIgnoreCase));

            var memberProjectCount = (await projectMembershipsTask ?? Enumerable.Empty<ProjectMember>())
                .Where(member => member != null && !string.IsNullOrWhiteSpace(member.ProjectNo))
                .Select(member => member.ProjectNo)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var notifications = (await notificationsTask ?? Enumerable.Empty<Notification>())
                .Where(notification => notification != null)
                .OrderByDescending(notification => notification.NotificationDate == default(DateTime)
                    ? notification.CreatedDate
                    : notification.NotificationDate)
                .ToList();

            var activityNotifications = notifications.Take(6).ToList();
            var recentActivities = await BuildRecentActivitiesAsync(activityNotifications);

            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))).Trim();

            var initials = string.Join(string.Empty, new[] { user.FirstName, user.LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Substring(0, 1).ToUpperInvariant()));

            var viewModel = new ProfileViewModel
            {
                User = user,
                FullName = string.IsNullOrWhiteSpace(fullName) ? user.UserId : fullName,
                Initials = string.IsNullOrWhiteSpace(initials) ? "?" : initials,
                PhotoUrl = Url.Action("Photo", "Profile", new { area = "Settings", id = user.UserId }),
                HasPhoto = UserPhotoExists(user.UserId),
                CanEditPhoto = string.Equals(user.UserId, User.Identity.GetClaim("employeeid"), StringComparison.OrdinalIgnoreCase),
                PhotoStatusMessage = TempData["ProfilePhotoStatus"] as string,
                PhotoErrorMessage = TempData["ProfilePhotoError"] as string,
                UserGroups = userGroupNames,
                PendingTaskCount = pendingTaskCount,
                CompletedTaskCount = completedTaskCount,
                OwnedProjectCount = ownedProjectCount,
                MemberProjectCount = memberProjectCount,
                ActiveNotificationCount = notifications.Count,
                UnreadNotificationCount = notifications.Count(notification => notification.IsViewed == 0),
                RecentActivities = recentActivities
            };

            var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
                area: "Settings",
                areaTitle: "Settings",
                controller: "Profile",
                controllerTitle: "Profile",
                action: "Index",
                actionTitle: viewModel.FullName,
                routeValues: new Dictionary<string, object> { { "id", user.UserId } }
            );

            ViewBag.Title = viewModel.FullName;
            ViewBag.TitleIcon = "bi bi-person-badge-fill";
            ViewBag.SubTitle = "User profile, workload snapshot, and recent activity";
            ViewBag.Breadcrumbs = breadcrumbs;

            return View(viewModel);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult> DashboardScopePreference()
        {
            var userId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new HttpStatusCodeResult(401);
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            return Json(new { showAllUsers = user.DashboardShowAllUsers == 1 }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> DashboardScopePreference(bool showAllUsers)
        {
            var userId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new HttpStatusCodeResult(401);
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.DashboardShowAllUsers = showAllUsers ? 1 : 0;
            user.ModifiedBy = userId;

            var rowsAffected = await _userService.UpdateUserAsync(user);
            if (rowsAffected <= 0)
            {
                return new HttpStatusCodeResult(409);
            }

            return Json(new { showAllUsers });
        }

        [HttpGet]
        public ActionResult Photo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            var photoPath = GetUserPhotoFilePath(id);
            if (!System.IO.File.Exists(photoPath))
            {
                return HttpNotFound();
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetMaxAge(TimeSpan.Zero);

            return File(photoPath, "image/jpeg");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadPhoto(HttpPostedFileBase photo)
        {
            var currentUserId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return HttpNotFound();
            }

            if (photo == null || photo.ContentLength <= 0)
            {
                TempData["ProfilePhotoError"] = "Select an image to upload.";
                return RedirectToAction("Index", new { id = currentUserId });
            }

            var maxPhotoBytes = GetConfiguredMaxFileSizeBytes("profilePhoto:maxFileSizeMB", DefaultProfilePhotoMaxFileSizeMb);
            var allowedPhotoExtensions = GetConfiguredAllowedExtensions("profilePhoto:allowedExtensions", DefaultProfilePhotoAllowedExtensions);
            var extension = NormalizeExtension(Path.GetExtension(photo.FileName));

            if (string.IsNullOrWhiteSpace(extension) || !allowedPhotoExtensions.Contains(extension))
            {
                TempData["ProfilePhotoError"] = "File type is not allowed for profile photos.";
                return RedirectToAction("Index", new { id = currentUserId });
            }

            if (photo.ContentLength > maxPhotoBytes)
            {
                var maxMb = Math.Max(1, maxPhotoBytes / (1024L * 1024L));
                TempData["ProfilePhotoError"] = $"Profile photo must be {maxMb} MB or smaller.";
                return RedirectToAction("Index", new { id = currentUserId });
            }

            try
            {
                SaveProfilePhotoAsJpeg(currentUserId, photo);
                TempData["ProfilePhotoStatus"] = "Profile photo updated.";
            }
            catch (ArgumentException)
            {
                TempData["ProfilePhotoError"] = "Upload a valid image file.";
            }
            catch
            {
                TempData["ProfilePhotoError"] = "Unable to save the profile photo right now.";
            }

            return RedirectToAction("Index", new { id = currentUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePhoto()
        {
            var currentUserId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return HttpNotFound();
            }

            var photoPath = GetUserPhotoFilePath(currentUserId);
            if (System.IO.File.Exists(photoPath))
            {
                System.IO.File.Delete(photoPath);
                TempData["ProfilePhotoStatus"] = "Profile photo removed.";
            }
            else
            {
                TempData["ProfilePhotoError"] = "No profile photo is currently saved.";
            }

            return RedirectToAction("Index", new { id = currentUserId });
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

        private static void SaveProfilePhotoAsJpeg(string userId, HttpPostedFileBase photo)
        {
            var photoDirectory = GetUserPhotoDirectory();
            Directory.CreateDirectory(photoDirectory);

            using (var image = Image.FromStream(photo.InputStream, useEmbeddedColorManagement: true, validateImageData: true))
            using (var avatar = CreateSquareAvatar(image, ProfilePhotoSize))
            {
                var photoPath = GetUserPhotoFilePath(userId);
                var encoder = GetJpegEncoder();
                if (encoder == null)
                {
                    avatar.Save(photoPath, ImageFormat.Jpeg);
                    return;
                }

                using (var encoderParameters = new EncoderParameters(1))
                {
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L);
                    avatar.Save(photoPath, encoder, encoderParameters);
                }
            }
        }

        private static Bitmap CreateSquareAvatar(Image sourceImage, int size)
        {
            if (sourceImage == null)
            {
                throw new ArgumentException("Invalid source image.");
            }

            var cropSize = Math.Min(sourceImage.Width, sourceImage.Height);
            if (cropSize <= 0)
            {
                throw new ArgumentException("Invalid source image dimensions.");
            }

            var cropX = (sourceImage.Width - cropSize) / 2;
            var cropY = (sourceImage.Height - cropSize) / 2;

            var avatar = new Bitmap(size, size);
            avatar.SetResolution(sourceImage.HorizontalResolution > 0 ? sourceImage.HorizontalResolution : 96f,
                sourceImage.VerticalResolution > 0 ? sourceImage.VerticalResolution : 96f);

            using (var graphics = Graphics.FromImage(avatar))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.Clear(Color.White);

                graphics.DrawImage(
                    sourceImage,
                    new Rectangle(0, 0, size, size),
                    new Rectangle(cropX, cropY, cropSize, cropSize),
                    GraphicsUnit.Pixel);
            }

            return avatar;
        }

        private static ImageCodecInfo GetJpegEncoder()
        {
            return ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(codec => string.Equals(codec.MimeType, "image/jpeg", StringComparison.OrdinalIgnoreCase));
        }

        private static bool UserPhotoExists(string userId)
        {
            return !string.IsNullOrWhiteSpace(userId) && System.IO.File.Exists(GetUserPhotoFilePath(userId));
        }
        private static string GetUserPhotoFilePath(string userId)
        {
            return Path.Combine(GetUserPhotoDirectory(), SanitizeFileName(userId) + ".jpg");
        }

        private static string GetUserPhotoDirectory()
        {
            var configuredVirtualFolder = ConfigurationManager.AppSettings["profilePhoto:virtualFolder"];
            return ResolveStorageRootPath(configuredVirtualFolder, "~/files/UserProfile");
        }

        private static string ResolveStorageRootPath(string configuredPath, string defaultVirtualPath)
        {
            var resolvedPath = string.IsNullOrWhiteSpace(configuredPath)
                ? defaultVirtualPath
                : configuredPath.Trim();

            resolvedPath = Environment.ExpandEnvironmentVariables(resolvedPath);

            if (IsUncPath(resolvedPath))
            {
                return NormalizeUncPath(resolvedPath);
            }

            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var fileUri) && fileUri.IsFile)
            {
                return fileUri.LocalPath;
            }

            if (Path.IsPathRooted(resolvedPath))
            {
                return resolvedPath;
            }

            if (resolvedPath.StartsWith("/") || resolvedPath.StartsWith("\\"))
            {
                resolvedPath = "~" + resolvedPath;
            }
            else if (!resolvedPath.StartsWith("~"))
            {
                resolvedPath = "~/" + resolvedPath.TrimStart('/', '\\');
            }

            string mappedPath = null;
            try
            {
                mappedPath = HostingEnvironment.MapPath(resolvedPath);
            }
            catch (HttpException)
            {
                // Fallback to a relative physical path when a virtual folder is missing or inaccessible.
            }

            if (!string.IsNullOrWhiteSpace(mappedPath))
            {
                return mappedPath;
            }

            var relativePath = resolvedPath.TrimStart('~', '/', '\\')
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        private static bool IsUncPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && (path.StartsWith("\\\\", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal));
        }

        private static string NormalizeUncPath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('/', '\\');
        }

        private static HashSet<string> GetConfiguredAllowedExtensions(string key, string defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[key];
            var source = string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;

            return new HashSet<string>(
                source
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(extension => NormalizeExtension(extension))
                    .Where(extension => !string.IsNullOrWhiteSpace(extension)),
                StringComparer.OrdinalIgnoreCase);
        }

        private static long GetConfiguredMaxFileSizeBytes(string key, int defaultMb)
        {
            var raw = ConfigurationManager.AppSettings[key];
            if (!int.TryParse(raw, out var parsedMb) || parsedMb <= 0)
            {
                parsedMb = defaultMb;
            }

            return (long)parsedMb * 1024L * 1024L;
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }

            var normalized = extension.Trim().ToLowerInvariant();
            return normalized.StartsWith(".") ? normalized : "." + normalized;
        }

        private static string SanitizeFileName(string userId)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            return new string((userId ?? string.Empty)
                .Select(character => invalidCharacters.Contains(character) ? '_' : character)
                .ToArray());
        }
    }
}