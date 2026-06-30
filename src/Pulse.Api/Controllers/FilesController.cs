using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.SharedUtilities.Extensions;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace Pulse.Api.Controllers
{

    [RoutePrefix("api/files")]
    public class FilesController : ApiController
    {
        private const string DefaultAttachmentAllowedExtensions = ".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.jpg,.jpeg,.png,.gif,.zip,.7z";
        private const int DefaultAttachmentMaxFileSizeMb = 25;

        private readonly IProjectAttachmentService _projectattachmentService;
        private readonly IProjectMemberService _projectMemberService;

        public FilesController(IProjectAttachmentService projectattachmentService, IProjectMemberService projectMemberService)
        {
            _projectattachmentService = projectattachmentService;
            _projectMemberService = projectMemberService;
        }


        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var loggeduserFirstName = User.Identity.GetClaim("firstname");
            var loggeduserLastName = User.Identity.GetClaim("lastname");

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type. Use 'multipart/form-data'.");

            var root = EnsureUploadsRootPath();

            var provider = new MultipartFormDataStreamProvider(root);

            // Parse multipart form (files + fields)
            await Request.Content.ReadAsMultipartAsync(provider);

            // ---- Read form fields ----
            var projectno = provider.FormData["projectno"];
            var entitytype = provider.FormData["entitytype"];
            var entitysysid = provider.FormData["entitysysid"];

            // Optional: validate required fields
            if (string.IsNullOrWhiteSpace(projectno))
            {
                return BadRequest("Missing required fields.");
            }

            // Folder per project
            var folderPath = Path.Combine(root, projectno);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var savedFiles = new List<string>();
            var failedFiles = new List<string>();
            var transactionDate = DateTime.Now;
            var files = new List<ProjectAttachment>();
            var allowedExtensions = GetConfiguredAllowedExtensions("attachments:allowedExtensions", DefaultAttachmentAllowedExtensions);
            var maxFileSizeBytes = GetConfiguredMaxFileSizeBytes("attachments:maxFileSizeMB", DefaultAttachmentMaxFileSizeMb);
            // ---- Process uploaded files ----
            foreach (var fileData in provider.FileData)
            {
                // original file name
                var originalFileName =
                    fileData.Headers.ContentDisposition.FileName?.Trim('"') ?? "unknown";

                var safeFileName = Path.GetFileName(originalFileName); // sanitize
                var tempFileInfo = new FileInfo(fileData.LocalFileName);

                if (!IsUploadAllowed(safeFileName, tempFileInfo.Length, allowedExtensions, maxFileSizeBytes, out var _))
                {
                    failedFiles.Add(safeFileName);
                    DeleteFileQuietly(fileData.LocalFileName);
                    continue;
                }

                var attachmentid = Guid.NewGuid().ToString();
                var targetPath = Path.Combine(folderPath, attachmentid + safeFileName);

                // Move from temp file to final location
                File.Move(fileData.LocalFileName, targetPath);


                // MIME type (e.g. "image/png", "application/pdf")
                var contentType = fileData.Headers.ContentType?.MediaType;

                // Extension (e.g. ".png", ".pdf")
                var extension = Path.GetExtension(safeFileName);

                // Size in bytes
                var fileInfo = new FileInfo(targetPath);
                long fileSizeBytes = fileInfo.Length;

                // Save metadata to DB
                var fileinfo = new ProjectAttachment
                {
                    AttachmentSysId = attachmentid,
                    ProjectNo = projectno,
                    EntityType = entitytype,
                    EntitySysId = entitysysid,
                    CreatedBy = loggeduser,
                    FileName = originalFileName,
                    AltFileName = safeFileName,
                    FileSize = int.Parse(fileSizeBytes.ToString()),
                    FileType = contentType,
                    FileExtension = extension,
                    CreatedDate = transactionDate,
                    CanManageAttachment = true,
                    CreatedByMeta = new User {
                        FirstName = loggeduserFirstName,
                        LastName = loggeduserLastName
                    }
                };
                files.Add(fileinfo);
                try
                {
                    await _projectattachmentService.AddAsync(fileinfo);
                    savedFiles.Add(safeFileName);

                }
                catch (Exception e)
                {
                    failedFiles.Add(safeFileName);
                }



            }

            return Ok(new
            {
                ProjectNo = projectno,
                EntityType = entitytype,
                EntitySysId = entitysysid,
                filesMeta = files,
                Files = savedFiles,
                Failed = failedFiles
            });
        }

        [HttpDelete]
        [Route("{attachmentSysId}")]
        public async Task<IHttpActionResult> Delete(string attachmentSysId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var attachment = await _projectattachmentService.GetByIdAsync(attachmentSysId);
            if (attachment == null)
            {
                return NotFound();
            }

            var currentUserId = User.Identity.GetClaim("employeeid");
            if (!await CanManageAttachmentAsync(attachment, currentUserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var rowsAffected = await _projectattachmentService.RemoveAsync(attachmentSysId);
            if (rowsAffected <= 0)
            {
                return BadRequest("Unable to delete attachment.");
            }

            DeletePhysicalFile(attachment);

            return Ok(new { attachmentSysId = attachmentSysId });
        }

        [HttpPost]
        [Route("{attachmentSysId}/replace")]
        public async Task<IHttpActionResult> Replace(string attachmentSysId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var existingAttachment = await _projectattachmentService.GetByIdAsync(attachmentSysId);
            if (existingAttachment == null)
            {
                return NotFound();
            }

            var currentUserId = User.Identity.GetClaim("employeeid");
            var currentFirstName = User.Identity.GetClaim("firstname");
            var currentLastName = User.Identity.GetClaim("lastname");

            if (!await CanManageAttachmentAsync(existingAttachment, currentUserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type. Use 'multipart/form-data'.");
            }

            var root = EnsureUploadsRootPath();
            var provider = new MultipartFormDataStreamProvider(root);
            await Request.Content.ReadAsMultipartAsync(provider);

            var replacementFile = provider.FileData.FirstOrDefault();
            if (replacementFile == null)
            {
                return BadRequest("No file uploaded.");
            }

            var replacementOriginalFileName = replacementFile.Headers.ContentDisposition.FileName?.Trim('"') ?? "unknown";
            var replacementSafeFileName = Path.GetFileName(replacementOriginalFileName);
            var replacementSizeBytes = new FileInfo(replacementFile.LocalFileName).Length;
            var allowedExtensions = GetConfiguredAllowedExtensions("attachments:allowedExtensions", DefaultAttachmentAllowedExtensions);
            var maxFileSizeBytes = GetConfiguredMaxFileSizeBytes("attachments:maxFileSizeMB", DefaultAttachmentMaxFileSizeMb);

            if (!IsUploadAllowed(replacementSafeFileName, replacementSizeBytes, allowedExtensions, maxFileSizeBytes, out var replacementValidationError))
            {
                DeleteFileQuietly(replacementFile.LocalFileName);
                return BadRequest(replacementValidationError);
            }

            var folderPath = EnsureProjectFolderPath(existingAttachment.ProjectNo);
            var replacementAttachment = BuildAttachmentMetadata(
                replacementFile,
                folderPath,
                existingAttachment.ProjectNo,
                existingAttachment.EntityType,
                existingAttachment.EntitySysId,
                currentUserId,
                currentFirstName,
                currentLastName,
                DateTime.Now);

            await _projectattachmentService.AddAsync(replacementAttachment);
            await _projectattachmentService.RemoveAsync(existingAttachment.AttachmentSysId);

            DeletePhysicalFile(existingAttachment);

            return Ok(new { data = replacementAttachment });
        }

        private string EnsureUploadsRootPath()
        {
            var configuredVirtualFolder = ConfigurationManager.AppSettings["attachments:virtualFolder"];
            var root = ResolveStorageRootPath(configuredVirtualFolder, "~/files");

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            return root;
        }

        private string EnsureProjectFolderPath(string projectNo)
        {
            var folderPath = Path.Combine(EnsureUploadsRootPath(), projectNo);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
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
                mappedPath = HttpContext.Current?.Server.MapPath(resolvedPath);
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

        private static bool IsUploadAllowed(
            string fileName,
            long fileSizeBytes,
            HashSet<string> allowedExtensions,
            long maxFileSizeBytes,
            out string validationError)
        {
            var extension = NormalizeExtension(Path.GetExtension(fileName));
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            {
                validationError = "File type is not allowed.";
                return false;
            }

            if (fileSizeBytes > maxFileSizeBytes)
            {
                var maxMb = Math.Max(1, maxFileSizeBytes / (1024L * 1024L));
                validationError = $"File size exceeds the {maxMb} MB limit.";
                return false;
            }

            validationError = null;
            return true;
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

        private static void DeleteFileQuietly(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
            }
        }

        private ProjectAttachment BuildAttachmentMetadata(
            MultipartFileData fileData,
            string folderPath,
            string projectNo,
            string entityType,
            string entitySysId,
            string createdBy,
            string createdFirstName,
            string createdLastName,
            DateTime createdDate)
        {
            var attachmentId = Guid.NewGuid().ToString();
            var originalFileName = fileData.Headers.ContentDisposition.FileName?.Trim('"') ?? "unknown";
            var safeFileName = Path.GetFileName(originalFileName);
            var targetPath = Path.Combine(folderPath, attachmentId + safeFileName);
            File.Move(fileData.LocalFileName, targetPath);

            var contentType = fileData.Headers.ContentType?.MediaType;
            var extension = Path.GetExtension(safeFileName);
            var fileInfo = new FileInfo(targetPath);

            return new ProjectAttachment
            {
                AttachmentSysId = attachmentId,
                ProjectNo = projectNo,
                EntityType = entityType,
                EntitySysId = entitySysId,
                CreatedBy = createdBy,
                FileName = originalFileName,
                AltFileName = safeFileName,
                FileSize = int.Parse(fileInfo.Length.ToString()),
                FileType = contentType,
                FileExtension = extension,
                CreatedDate = createdDate,
                CanManageAttachment = true,
                CreatedByMeta = new User
                {
                    FirstName = createdFirstName,
                    LastName = createdLastName
                }
            };
        }

        private async Task<bool> CanManageAttachmentAsync(ProjectAttachment attachment, string userId)
        {
            if (attachment == null || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            if (string.Equals(attachment.CreatedBy, userId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var members = await _projectMemberService.GetAllProjectMembersAsync(attachment.ProjectNo);
            return (members ?? Enumerable.Empty<ProjectMember>())
                .Any(member => member != null && string.Equals(member.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        private void DeletePhysicalFile(ProjectAttachment attachment)
        {
            if (attachment == null || string.IsNullOrWhiteSpace(attachment.ProjectNo) || string.IsNullOrWhiteSpace(attachment.AttachmentSysId))
            {
                return;
            }

            var filePath = Path.Combine(EnsureUploadsRootPath(), attachment.ProjectNo, attachment.AttachmentSysId + (attachment.AltFileName ?? string.Empty));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}
