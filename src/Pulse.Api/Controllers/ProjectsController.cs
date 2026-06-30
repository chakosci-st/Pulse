using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using Pulse.SharedUtilities.Extensions;
using Pulse.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/Projects")]
    public class ProjectsController : ApiController
    {
        private const string SuperUserModuleCode = "SUPERUSER";

        private readonly IProjectService _projectService;
        private readonly IRoadmapService _roadmapService;
        private readonly IProjectMemberService _projectmemberService;
        private readonly IProjectOwnerService _projectownerService;
        private readonly IProjectRepository _projectRepository;
        private readonly IPlantMemberRepository _plantMemberRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectOwnerRepository _projectOwnerRepository;
        private readonly IProjectCommentService _projectCommentService;
        private readonly IProjectMilestoneService _projectMilestoneService;
        private readonly IProjectTaskService _projectTaskService;
        ////private readonly IEventSubscriber<ProjectStartedEventArgs> _projectStartedSubscriber;
        ////private readonly IEventSubscriber<ProjectCreatedEventArgs> _projectCreatedSubscriber;
        ////private readonly IEventSubscriber<ProjectNotStartedEventArgs> _projectNotStartedSubscriber;


        public ProjectsController(IProjectService projectService, IRoadmapService roadmapService, IProjectMemberService projectmemberService, IProjectOwnerService projectownerService,
            IProjectRepository projectRepository, IPlantMemberRepository plantMemberRepository, IProjectMemberRepository projectMemberRepository,
            IProjectTaskRepository projectTaskRepository, IProjectOwnerRepository projectOwnerRepository,
            IProjectCommentService projectCommentService, IProjectMilestoneService projectMilestoneService,
            IProjectTaskService projectTaskService
            ////IEventSubscriber<ProjectStartedEventArgs> projectStartedSubscriber,
            ////IEventSubscriber<ProjectCreatedEventArgs> projectCreatedSubscriber,
            ////IEventSubscriber<ProjectNotStartedEventArgs> projectNotStartedSubscriber
            )
        {
            _projectService = projectService;
            _roadmapService = roadmapService;
            _projectmemberService = projectmemberService;
            _projectownerService = projectownerService;
            _projectRepository = projectRepository;
            _plantMemberRepository = plantMemberRepository;
            _projectMemberRepository = projectMemberRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectOwnerRepository = projectOwnerRepository;
            _projectCommentService = projectCommentService;
            _projectMilestoneService = projectMilestoneService;
            _projectTaskService = projectTaskService;
            ////_projectStartedSubscriber = projectStartedSubscriber;
            ////_projectCreatedSubscriber = projectCreatedSubscriber;
            ////_projectNotStartedSubscriber = projectNotStartedSubscriber;

            ////_eventBus.Subscribe(_projectStartedSubscriber);
            ////_eventBus.Subscribe(_projectCreatedSubscriber);
            ////_eventBus.Subscribe(_projectNotStartedSubscriber);
        }

        private IHttpActionResult ForbiddenProjectAccess(string message)
        {
            return Content(HttpStatusCode.Forbidden, new { message });
        }

        private bool HasSuperUserModule()
        {
            return ParseCsvToSet(User.Identity.GetClaim("modulecodes")).Contains(SuperUserModuleCode);
        }

        private async Task<bool> HasActivePlantMembershipAsync(string loggedUserId, string plantCode = null)
        {
            if (HasSuperUserModule())
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(loggedUserId))
            {
                return false;
            }

            var memberships = await _plantMemberRepository.GetListAsync(plantCode, loggedUserId);
            return memberships.Any(member => member != null && member.IsActive == 1);
        }

        private async Task<Project> GetProjectAsync(string projectNo)
        {
            if (string.IsNullOrWhiteSpace(projectNo))
            {
                return null;
            }

            return await _projectRepository.GetAsync(projectNo);
        }

        private async Task<bool> CanManageProjectAsync(string loggedUserId, string projectNo)
        {
            if (HasSuperUserModule())
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(loggedUserId) || string.IsNullOrWhiteSpace(projectNo))
            {
                return false;
            }

            var project = await GetProjectAsync(projectNo);
            if (project == null)
            {
                return false;
            }

            if (string.Equals(project.ProjectOwnerId, loggedUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var projectMembers = await _projectMemberRepository.GetListAsync(projectNo);
            return projectMembers.Any(member => member != null && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> IsProjectOwnerMemberAsync(string loggedUserId, string projectNo)
        {
            if (HasSuperUserModule())
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(loggedUserId) || string.IsNullOrWhiteSpace(projectNo))
            {
                return false;
            }

            var projectMembers = await _projectMemberRepository.GetListAsync(projectNo);
            if (projectMembers.Any(member => member != null
                && member.IsOwner == 1
                && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var project = await GetProjectAsync(projectNo);
            return project != null && string.Equals(project.ProjectOwnerId, loggedUserId, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> CanViewProjectDetailsAsync(string loggedUserId, string projectNo)
        {
            if (HasSuperUserModule())
            {
                return true;
            }

            if (await CanManageProjectAsync(loggedUserId, projectNo))
            {
                return true;
            }

            var project = await GetProjectAsync(projectNo);
            if (project == null)
            {
                return false;
            }

            return await HasActivePlantMembershipAsync(loggedUserId, project.PlantCode);
        }

        private async Task<ProjectTask> GetCustomProjectTaskAsync(string projectNo, string projectTaskSysId)
        {
            if (string.IsNullOrWhiteSpace(projectNo) || string.IsNullOrWhiteSpace(projectTaskSysId))
            {
                return null;
            }

            var task = await _projectTaskRepository.GetAsync(projectTaskSysId);
            if (task == null || !string.Equals(task.ProjectNo, projectNo, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return task;
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string NormalizeCustomMilestoneSourceMode(string sourceMode)
        {
            var normalized = (sourceMode ?? string.Empty).Trim().ToUpperInvariant();
            return normalized == "TEMPLATE" ? "TEMPLATE" : "MANUAL";
        }

        private static List<string> NormalizeTemplateMilestoneIds(IEnumerable<string> templateMilestoneSysIds, string templateMilestoneSysId = null)
        {
            var normalizedIds = (templateMilestoneSysIds ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!string.IsNullOrWhiteSpace(templateMilestoneSysId)
                && !normalizedIds.Contains(templateMilestoneSysId.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                normalizedIds.Insert(0, templateMilestoneSysId.Trim());
            }

            return normalizedIds;
        }

        private async Task<ProjectCopyDraftResponse> BuildProjectCopyDraftAsync(Project project)
        {
            if (project == null
                || string.IsNullOrWhiteSpace(project.ProjectNo)
                || string.IsNullOrWhiteSpace(project.RoadmapSysId))
            {
                return null;
            }

            var roadmap = await _roadmapService.GetRoadmapByIdAsync(project.RoadmapSysId);
            if (roadmap == null)
            {
                return null;
            }

            var roadmapTree = await _roadmapService.GetTreeResponseAsync(project.RoadmapSysId) ?? new dtoTreeResponse();
            var projectMembers = ((await _projectmemberService.GetAllProjectMembersAsync(project.ProjectNo)) ?? Enumerable.Empty<ProjectMember>())
                .Where(member => member != null)
                .ToList();
            var projectNodes = ((await _projectService.GetProjectNodesAsync(project.ProjectNo)) ?? new List<ProjectExtend>())
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.NodeId))
                .GroupBy(node => node.NodeId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            string BuildLabel(string firstName, string lastName, string userName, string fallbackText = null)
            {
                var fullName = string.Join(" ", new[] { NormalizeText(firstName), NormalizeText(lastName) }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim();
                var normalizedUserName = NormalizeText(userName);
                if (!string.IsNullOrWhiteSpace(normalizedUserName))
                {
                    return string.IsNullOrWhiteSpace(fullName)
                        ? normalizedUserName
                        : $"{fullName} ({normalizedUserName})";
                }

                return !string.IsNullOrWhiteSpace(fullName) ? fullName : NormalizeText(fallbackText) ?? string.Empty;
            }

            var memberReferences = new List<(string UserId, string UserName, string FirstName, string LastName, string Email, string Label)>();

            void AddMemberReference(string userId, string userName, string firstName, string lastName, string email, string fallbackText = null)
            {
                var normalizedUserId = NormalizeText(userId);
                var normalizedUserName = NormalizeText(userName);
                if (string.IsNullOrWhiteSpace(normalizedUserId) && string.IsNullOrWhiteSpace(normalizedUserName))
                {
                    return;
                }

                var exists = memberReferences.Any(member =>
                    (!string.IsNullOrWhiteSpace(normalizedUserId)
                        && !string.IsNullOrWhiteSpace(member.UserId)
                        && string.Equals(member.UserId, normalizedUserId, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(normalizedUserName)
                        && !string.IsNullOrWhiteSpace(member.UserName)
                        && string.Equals(member.UserName, normalizedUserName, StringComparison.OrdinalIgnoreCase)));

                if (exists)
                {
                    return;
                }

                var label = BuildLabel(firstName, lastName, normalizedUserName, fallbackText ?? normalizedUserId);
                if (string.IsNullOrWhiteSpace(label))
                {
                    return;
                }

                memberReferences.Add((
                    normalizedUserId,
                    normalizedUserName,
                    NormalizeText(firstName),
                    NormalizeText(lastName),
                    NormalizeText(email),
                    label));
            }

            foreach (var member in projectMembers)
            {
                AddMemberReference(
                    member.UserId,
                    member.User?.UserName,
                    member.User?.FirstName,
                    member.User?.LastName,
                    member.User?.Email,
                    member.UserId);
            }

            var ownerMember = projectMembers.FirstOrDefault(member =>
                string.Equals(member.UserId, project.ProjectOwnerId, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(member.User?.UserName)
                    && string.Equals(member.User.UserName, project.ProjectOwnerUserName, StringComparison.OrdinalIgnoreCase)));

            AddMemberReference(
                project.ProjectOwnerId,
                ownerMember?.User?.UserName ?? project.ProjectOwnerUserName,
                ownerMember?.User?.FirstName,
                ownerMember?.User?.LastName,
                ownerMember?.User?.Email,
                project.ProjectOwnerUserName ?? project.ProjectOwnerId);

            var memberByUserId = memberReferences
                .Where(member => !string.IsNullOrWhiteSpace(member.UserId))
                .ToDictionary(member => member.UserId, member => member, StringComparer.OrdinalIgnoreCase);
            var memberByUserName = memberReferences
                .Where(member => !string.IsNullOrWhiteSpace(member.UserName))
                .ToDictionary(member => member.UserName, member => member, StringComparer.OrdinalIgnoreCase);

            List<ProjectCopyDraftOwnerData> ParseOwnerData(string rawJson)
            {
                var normalizedJson = NormalizeText(rawJson);
                if (string.IsNullOrWhiteSpace(normalizedJson))
                {
                    return new List<ProjectCopyDraftOwnerData>();
                }

                try
                {
                    return JsonConvert.DeserializeObject<List<ProjectCopyDraftOwnerData>>(normalizedJson) ?? new List<ProjectCopyDraftOwnerData>();
                }
                catch
                {
                    return new List<ProjectCopyDraftOwnerData>();
                }
            }

            List<string> ResolveOwnerLabels(ProjectExtend node)
            {
                var owners = ParseOwnerData(node?.JsonNodeOwners ?? node?.JsonMembers);
                var labels = new List<string>();

                foreach (var owner in owners)
                {
                    var normalizedUserId = NormalizeText(owner?.UserId);
                    var normalizedUserName = NormalizeText(owner?.UserName);

                    if (!string.IsNullOrWhiteSpace(normalizedUserId)
                        && memberByUserId.TryGetValue(normalizedUserId, out var memberById))
                    {
                        labels.Add(memberById.Label);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(normalizedUserName)
                        && memberByUserName.TryGetValue(normalizedUserName, out var memberByName))
                    {
                        labels.Add(memberByName.Label);
                        continue;
                    }

                    var fallbackLabel = BuildLabel(owner?.FirstName, owner?.LastName, normalizedUserName, owner?.Text ?? normalizedUserId);
                    if (!string.IsNullOrWhiteSpace(fallbackLabel))
                    {
                        labels.Add(fallbackLabel);
                    }
                }

                return labels
                    .Where(label => !string.IsNullOrWhiteSpace(label))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            double? ParseMandays(string value)
            {
                if (double.TryParse(value, out var parsedValue))
                {
                    return parsedValue;
                }

                return null;
            }

            List<dtoProjectForm> MapForms(IEnumerable<dtoNodeForm> forms)
            {
                return (forms ?? Enumerable.Empty<dtoNodeForm>())
                    .Where(form => form != null)
                    .Select(form => new dtoProjectForm
                    {
                        Key = form.Key,
                        Id = form.Id,
                        SysId = form.Id,
                        Name = form.Name,
                        Desc = form.Desc
                    })
                    .ToList();
            }

            var milestones = new List<dtoMilestone>();
            var milestoneMap = new Dictionary<string, dtoMilestone>(StringComparer.OrdinalIgnoreCase);

            dtoMilestone GetOrCreateMilestone(dtoNode node)
            {
                var nodeId = NormalizeText(node?.Id) ?? Guid.NewGuid().ToString();
                if (milestoneMap.TryGetValue(nodeId, out var milestone))
                {
                    return milestone;
                }

                projectNodes.TryGetValue(nodeId, out var projectNode);
                milestone = new dtoMilestone
                {
                    Name = NormalizeText(node?.Data?.Name) ?? "Untitled milestone",
                    Owners = ResolveOwnerLabels(projectNode),
                    StartDate = projectNode?.ProjectNodeTargetStartYear?.ToString() ?? string.Empty,
                    EndDate = projectNode?.ProjectNodeTargetCompletionYear?.ToString() ?? string.Empty,
                    StartWeek = NormalizeText(projectNode?.ProjectNodeTargetStartWorkWeek) ?? string.Empty,
                    EndWeek = NormalizeText(projectNode?.ProjectNodeTargetCompletionWorkWeek) ?? string.Empty,
                    Tasks = new List<dtoTask>(),
                    Meta = new dtoMilestoneMeta
                    {
                        Id = nodeId,
                        Maturity = NormalizeText(node?.Data?.Maturity),
                        Mandays = ParseMandays(node?.Data?.Mandays),
                        IsRequired = node?.Data?.IsRequired ?? false,
                        Desc = NormalizeText(node?.Data?.Desc) ?? string.Empty
                    }
                };

                milestoneMap[nodeId] = milestone;
                milestones.Add(milestone);
                return milestone;
            }

            dtoMilestone GetOrCreateRootActivitiesMilestone()
            {
                const string rootActivitiesId = "__ROOT_ACTIVITIES__";
                if (milestoneMap.TryGetValue(rootActivitiesId, out var milestone))
                {
                    return milestone;
                }

                milestone = new dtoMilestone
                {
                    Name = "Root activities",
                    Owners = new List<string>(),
                    StartDate = string.Empty,
                    EndDate = string.Empty,
                    StartWeek = string.Empty,
                    EndWeek = string.Empty,
                    Tasks = new List<dtoTask>(),
                    Meta = new dtoMilestoneMeta
                    {
                        Id = rootActivitiesId,
                        Desc = "Activities not under a milestone"
                    }
                };

                milestoneMap[rootActivitiesId] = milestone;
                milestones.Add(milestone);
                return milestone;
            }

            void AddActivityTask(dtoNode node, dtoMilestone milestone)
            {
                if (milestone == null)
                {
                    return;
                }

                projectNodes.TryGetValue(NormalizeText(node?.Id) ?? string.Empty, out var projectNode);
                milestone.Tasks.Add(new dtoTask
                {
                    Name = NormalizeText(node?.Data?.Name) ?? "Untitled activity",
                    Owners = ResolveOwnerLabels(projectNode),
                    StartDate = projectNode?.ProjectNodeTargetStartYear?.ToString() ?? string.Empty,
                    EndDate = projectNode?.ProjectNodeTargetCompletionYear?.ToString() ?? string.Empty,
                    StartWeek = NormalizeText(projectNode?.ProjectNodeTargetStartWorkWeek) ?? string.Empty,
                    EndWeek = NormalizeText(projectNode?.ProjectNodeTargetCompletionWorkWeek) ?? string.Empty,
                    Meta = new dtoTaskMeta
                    {
                        Id = NormalizeText(node?.Id),
                        Desc = NormalizeText(node?.Data?.Desc) ?? string.Empty,
                        Maturity = NormalizeText(node?.Data?.Maturity),
                        Mandays = ParseMandays(node?.Data?.Mandays),
                        IsRequired = node?.Data?.IsRequired,
                        Prerequisites = (node?.Prerequisites ?? new List<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).ToList(),
                        Forms = MapForms(node?.Forms),
                        Collapsed = node?.Collapsed
                    }
                });
            }

            void TraverseTree(dtoNode node, dtoMilestone currentMilestone)
            {
                if (node == null)
                {
                    return;
                }

                var normalizedType = (NormalizeText(node.Type) ?? string.Empty).ToLowerInvariant();
                var milestoneForChildren = currentMilestone;

                if (normalizedType == "milestone")
                {
                    milestoneForChildren = GetOrCreateMilestone(node);
                }
                else if (normalizedType == "activity")
                {
                    if (currentMilestone != null)
                    {
                        AddActivityTask(node, currentMilestone);
                    }
                    else
                    {
                        var rootMilestone = GetOrCreateRootActivitiesMilestone();
                        AddActivityTask(node, rootMilestone);
                        milestoneForChildren = rootMilestone;
                    }
                }

                foreach (var child in node.Children ?? new List<dtoNode>())
                {
                    TraverseTree(child, milestoneForChildren);
                }
            }

            foreach (var rootNode in roadmapTree.TreeData ?? new List<dtoNode>())
            {
                TraverseTree(rootNode, null);
            }

            var ownerReference = memberReferences.FirstOrDefault(member =>
                (!string.IsNullOrWhiteSpace(project.ProjectOwnerId)
                    && !string.IsNullOrWhiteSpace(member.UserId)
                    && string.Equals(member.UserId, project.ProjectOwnerId, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(project.ProjectOwnerUserName)
                    && !string.IsNullOrWhiteSpace(member.UserName)
                    && string.Equals(member.UserName, project.ProjectOwnerUserName, StringComparison.OrdinalIgnoreCase)));

            var ownerUserName = NormalizeText(ownerReference.UserName) ?? NormalizeText(project.ProjectOwnerUserName);
            var ownerLabel = BuildLabel(ownerReference.FirstName, ownerReference.LastName, ownerUserName, ownerReference.UserId ?? project.ProjectOwnerId);
            var templateName = NormalizeText(roadmap.RoadmapName) ?? NormalizeText(project.RoadmapSysId);
            var templateCategoryCode = NormalizeText(project.CategoryCode) ?? NormalizeText(roadmap.CategoryCode);

            return new ProjectCopyDraftResponse
            {
                SiteValue = NormalizeText(project.PlantCode),
                SiteText = NormalizeText(project.PlantCode),
                TemplatePlantRoadmapLinkSysId = NormalizeText(project.PlantRoadmapLinkSysId),
                TemplateValue = NormalizeText(project.RoadmapSysId),
                TemplateText = string.IsNullOrWhiteSpace(templateCategoryCode)
                    ? templateName
                    : $"{templateName} - {templateCategoryCode}",
                TemplateDescription = NormalizeText(roadmap.RoadmapDescription),
                TemplateCategory = templateCategoryCode,
                TemplateCategoryValue = templateCategoryCode,
                TemplateJson = JsonConvert.SerializeObject(roadmapTree, Formatting.Indented),
                Title = string.IsNullOrWhiteSpace(project.ProjectName)
                    ? "Copied Project"
                    : $"{project.ProjectName} (Copy)",
                Description = NormalizeText(project.ProjectDescription),
                Icon = NormalizeText(project.ProjectIcon),
                IconColor = NormalizeText(project.ProjectIconColor),
                OwnerValue = ownerUserName,
                OwnerText = ownerLabel,
                OwnerData = new ProjectCopyDraftOwnerData
                {
                    Id = ownerUserName,
                    Text = ownerLabel,
                    Email = NormalizeText(ownerReference.Email),
                    FirstName = NormalizeText(ownerReference.FirstName),
                    LastName = NormalizeText(ownerReference.LastName),
                    UserId = NormalizeText(ownerReference.UserId) ?? NormalizeText(project.ProjectOwnerId),
                    UserName = ownerUserName
                },
                ProductgroupValue = NormalizeText(project.ProductGroupCode),
                ProductdivisionValue = NormalizeText(project.ProductDivisionCode),
                ProjectstartYear = project.TargetStartYear?.ToString(),
                ProjectstartWorkWeek = NormalizeText(project.TargetStartWorkWeek),
                ProjectendYear = project.TargetCompletionYear?.ToString(),
                ProjectendWorkWeek = NormalizeText(project.TargetCompletionWorkWeek),
                Members = memberReferences
                    .Where(member => !string.IsNullOrWhiteSpace(member.Label))
                    .Select(member => new dtoMember
                    {
                        UserId = member.UserId,
                        UserName = member.UserName,
                        Name = member.Label
                    })
                    .ToList(),
                Milestones = milestones
            };
        }

        [HttpGet]
        [Route("{projectno}/copy-draft")]
        public async Task<IHttpActionResult> GetProjectCopyDraft(string projectno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggedUser = User.Identity.GetClaim("employeeid");
                if (!await CanViewProjectDetailsAsync(loggedUser, projectno))
                {
                    return ForbiddenProjectAccess("You are not authorized to copy this project.");
                }

                var project = await _projectService.GetProjectByIdAsync(projectno);
                if (project == null)
                {
                    return NotFound();
                }

                var draft = await BuildProjectCopyDraftAsync(project);
                if (draft == null)
                {
                    return BadRequest("Unable to prepare a copy for this project.");
                }

                return Ok(draft);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // POST api/Projects
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateProject()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var _modelObject = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "project");
            ProjectInitViewModel model = null;
            if (_modelObject != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };


                try
                {
                    var modelJson = await _modelObject.ReadAsStringAsync();
                    model = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectInitViewModel>(modelJson);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }

            }



            if (!ModelState.IsValid)
                return BadRequest(ModelState);










            if (model == null)
                return BadRequest("Request body is empty.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Project title is required.");

            if (string.IsNullOrWhiteSpace(model.SiteValue))
                return BadRequest("Site is required.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await HasActivePlantMembershipAsync(loggeduser))
                {
                    return ForbiddenProjectAccess("Only plant members can register new projects.");
                }

                var project = Mapper.Map<Project>(model);

                project.CreatedBy = loggeduser;
                project.ModifiedBy = loggeduser;
                project.Status = model.AutoStart ? "ONGOING" : "NOT STARTED";

                //var projectId = Guid.NewGuid().ToString();
                var _projectId = await _projectService.CreateProjectAsync(project);


                // raise event that Task status was changed


                // Return 201 Created + some result
                return Content(HttpStatusCode.Created, new
                {
                    title = project.ProjectName,
                    projectId = _projectId,
                    message = "Project created successfully."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }

        // POST api/Projects
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> UpdateProject(ProjectUpdateViewModel model)
        {
            if (model == null)
                return BadRequest("Request body is empty.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Project title is required.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, model.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update this project.");
                }

                var claimsIdentity = User.Identity as ClaimsIdentity;

                var project = await _projectService.GetProjectByIdAsync(model.ProjectNo);
                project.ProjectName = model.Title;
                project.ProjectDescription = model.Description;
                project.ProductGroupCode = model.ProductGroupCode;
                project.ProductDivisionCode = model.ProductDivisionCode;
                project.ProjectIcon = model.Icon;
                project.ProjectIconColor = model.IconColor;
                project.TransactionKey = model.TransactionKey;

                project.ModifiedBy = loggeduser;
                project.UserModified = new User
                {
                    UserId = loggeduser,
                    FirstName = User.Identity.GetClaim("firstname"),
                    LastName = User.Identity.GetClaim("lastname"),
                    Email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value,
                };

                await _projectService.UpdateProjectAsync(project);

                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    title = project.ProjectName,
                    projectId = model.ProjectNo,
                    message = "Project updated successfully."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }



        /// <summary>
        /// Updates an basic info of roadmap.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("ChangeStatus/{code}")]
        //[RequireRoadmapExistsAttribute]
        //[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPEDIT")]
        public async Task<IHttpActionResult> ChangeStatus(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "roadmap");
            dtoProjectChangeStatus dtoproject = null;
            if (objContent != null)
            {
                var objJson = await objContent.ReadAsStringAsync();
                dtoproject = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoProjectChangeStatus>(objJson);
            }

            if (dtoproject == null)
                return BadRequest("Project data is missing.");

            if (code != dtoproject.ProjectNo)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!await CanManageProjectAsync(User.Identity.GetClaim("employeeid"), dtoproject.ProjectNo))
            {
                return ForbiddenProjectAccess("Only project members can change project status.");
            }

            if (dtoproject.IsActive == 0)
            {
                await _projectService.HoldAsync(new Project
                {
                    ProjectNo = dtoproject.ProjectNo,
                    TransactionKey = dtoproject.TransactionKey,
                    ModifiedBy = User.Identity.GetClaim("employeeid")
                }, dtoproject.Reason, true, false, true);
            }
            else
            {
                await _projectService.UnholdAsync(new Project
                {
                    ProjectNo = dtoproject.ProjectNo,
                    TransactionKey = dtoproject.TransactionKey,
                    ModifiedBy = User.Identity.GetClaim("employeeid")
                }, dtoproject.Reason, true, false, true);
            }

            var newproject = await _projectService.GetProjectByIdAsync(dtoproject.ProjectNo);


            // return StatusCode(System.Net.HttpStatusCode.NoContent);

            return Content(HttpStatusCode.Accepted, new
            {
                projectNo = dtoproject.ProjectNo,
                transactionKey = newproject.TransactionKey,
                message = "Project updated successfully."
            });
        }



        ///////// <summary>
        ///////// Deletes a roadmap by Code.
        ///////// </summary>
        //////[HttpDelete]
        //////[Authorize]
        //////[Route("{code}")]
        ////////[RequireProjectExistsAttribute]
        ////////[Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPDEL")]
        //////public async Task<IHttpActionResult> Delete(string code)
        //////{
        //////    if (!Request.Content.IsMimeMultipartContent())
        //////        return BadRequest("Unsupported media type.");

        //////    var provider = new MultipartMemoryStreamProvider();
        //////    await Request.Content.ReadAsMultipartAsync(provider);

        //////    // Get the plant data (assuming the input name is 'plant')
        //////    var roadmapContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "roadmap");
        //////    dtoRoadmapExtended dtoroadmapextended = null;
        //////    if (roadmapContent != null)
        //////    {
        //////        var roadmapJson = await roadmapContent.ReadAsStringAsync();
        //////        dtoroadmapextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
        //////    }

        //////    if (dtoroadmapextended == null)
        //////        return BadRequest("Roadmap data is missing.");

        //////    if (code != dtoroadmapextended.RoadmapSysId)
        //////        return BadRequest("Invalid request.");

        //////    if (!ModelState.IsValid)
        //////        return BadRequest(ModelState);

        //////    var roadmap = await _roadmapService.GetRoadmapByIdAsync(code);

        //////    if (roadmap == null)
        //////        return NotFound();

        //////    await _roadmapService.DeleteRoadmapAsync(code, User.Identity.GetClaim("employeeid"));
        //////    return Ok();
        //////}



        [Route("dashboard/counter")]

        public async Task<IHttpActionResult> GetProjectCounter(bool showAllUsers = false)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");

            var obj = await _projectService.GetDashboardCardsCounter(showAllUsers || HasSuperUserModule() ? null : loggeduser);

            return Ok(obj);
        }

        [HttpGet]
        [Route("search/global")]
        public async Task<IHttpActionResult> SearchGlobal(string q = "", int take = 60)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var query = (q ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(Array.Empty<object>());
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var effectiveLoggedUser = HasSuperUserModule() ? null : loggeduser;
            var pageSize = Math.Max(1, Math.Min(take, 100));
            var normalizedQuery = query.ToLowerInvariant();
            var projectSearch = new ProjectExtendSearch
            {
                Search = string.Empty,
                LoggedUser = effectiveLoggedUser,
                NodeType = "roadmap",
                OrderColumn = "projectno",
                OrderDir = "asc",
                StartIndex = 0,
                LengthCount = int.MaxValue
            };

            bool MatchesProject(ProjectExtend item)
            {
                var haystack = string.Join(" ", new[]
                {
                    item.ProjectNo,
                    item.ProjectName,
                    item.ProjectOwnerFirstName,
                    item.ProjectOwnerLastName,
                    item.PlantCode,
                    item.CategoryCode,
                    item.ProductCodes,
                    item.Status
                }.Where(value => !string.IsNullOrWhiteSpace(value))).ToLowerInvariant();

                return haystack.Contains(normalizedQuery);
            }

            bool MatchesNode(ProjectExtend item)
            {
                var haystack = string.Join(" ", new[]
                {
                    item.ProjectNo,
                    item.ProjectName,
                    item.NodeName,
                    item.NodeDescription,
                    item.NodeFullPath,
                    item.ProjectOwnerFirstName,
                    item.ProjectOwnerLastName,
                    item.PlantCode,
                    item.CategoryCode,
                    item.ProductCodes,
                    item.ProjectNodeStatus,
                    item.Status
                }.Where(value => !string.IsNullOrWhiteSpace(value))).ToLowerInvariant();

                return haystack.Contains(normalizedQuery);
            }

            var projectResults = (await _projectService.GetPagedFullProjectsAsync(projectSearch)).Data
                .Where(item => !string.IsNullOrWhiteSpace(item.ProjectNo))
                .Where(MatchesProject)
                .GroupBy(item => item.ProjectNo, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Select(item => new
                {
                    projectNo = item.ProjectNo,
                    projectName = item.ProjectName,
                    nodeId = item.NodeId,
                    nodeType = item.NodeType,
                    nodeName = item.NodeName,
                    nodeFullPath = item.NodeFullPath,
                    resultType = string.IsNullOrWhiteSpace(item.NodeType) ? "project" : item.NodeType,
                    status = string.IsNullOrWhiteSpace(item.ProjectNodeStatus) ? item.Status : item.ProjectNodeStatus,
                    targetCompletion = item.ProjectNodeTargetCompletionDate
                        ?? item.ProjectNodeTargetCompletion
                        ?? item.TargetCompletionDate,
                    ownerName = ($"{item.ProjectOwnerFirstName} {item.ProjectOwnerLastName}").Trim(),
                    plantCode = item.PlantCode,
                    categoryCode = item.CategoryCode,
                    productCodes = item.ProductCodes,
                    linkUrl = $"/Projects/{Uri.EscapeDataString(item.ProjectNo)}/Details"
                })
                .ToList();

            var nodeResults = (await _projectRepository.GetProjectNodesByUserAsync(effectiveLoggedUser))
                .Where(item => !string.IsNullOrWhiteSpace(item.ProjectNo))
                .Where(item => string.Equals(item.NodeType, "milestone", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.NodeType, "activity", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.NodeType, "task", StringComparison.OrdinalIgnoreCase))
                .Where(MatchesNode)
                .Select(item => new
                {
                    projectNo = item.ProjectNo,
                    projectName = item.ProjectName,
                    nodeId = item.NodeId,
                    nodeType = item.NodeType,
                    nodeName = item.NodeName,
                    nodeFullPath = item.NodeFullPath,
                    resultType = string.Equals(item.NodeType, "activity", StringComparison.OrdinalIgnoreCase) ? "task" : item.NodeType,
                    status = string.IsNullOrWhiteSpace(item.ProjectNodeStatus) ? item.Status : item.ProjectNodeStatus,
                    targetCompletion = item.ProjectNodeTargetCompletionDate
                        ?? item.ProjectNodeTargetCompletion
                        ?? item.TargetCompletionDate,
                    ownerName = ($"{item.ProjectOwnerFirstName} {item.ProjectOwnerLastName}").Trim(),
                    plantCode = item.PlantCode,
                    categoryCode = item.CategoryCode,
                    productCodes = item.ProductCodes,
                    linkUrl = $"/Projects/{Uri.EscapeDataString(item.ProjectNo)}/Details"
                })
                .GroupBy(item => new { item.projectNo, item.nodeId, item.resultType, item.nodeName, item.nodeFullPath })
                .Select(group => group.First())
                .ToList();

            var results = projectResults
                .Concat(nodeResults)
                .OrderBy(item => item.resultType == "project" ? 0 : 1)
                .ThenBy(item => item.projectNo)
                .ThenBy(item => item.nodeName)
                .Take(pageSize)
                .ToList();

            return Ok(results);
        }

        [HttpGet]
        [Route("report2/monitoring")]
        public async Task<IHttpActionResult> GetProjectMonitoringReport()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggedUser = User.Identity.GetClaim("employeeid");
            var report = await _projectService.GetProjectMonitoringReportAsync(HasSuperUserModule() ? null : loggedUser);
            return Ok(report);
        }


        /// <summary>
        /// Gets a paged list of roadmaps with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for plant code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetProjectsForDataTables([FromBody] DataTablesRequestProject request)
        {

            int pageNumber = 1;
            int pageSize = request.length;

            // If length == -1, fetch all rows
            if (request.length == -1)
            {
                pageSize = int.MaxValue; // or a large number, or remove paging logic
            }
            else
            {
                pageNumber = (request.start / request.length) + 1;
            }
            int startIndex = (pageNumber - 1) * pageSize;
            string searchValue = request.search?.value ?? "";
            ////bool? isActive = request.isActive;

            ////// Whitelist of allowed columns and directions
            ////var allowedColumns = new HashSet<string> { "ROADMAPSYSID", "ROADMAPNAME", "ROADMAPDESCRIPTION", "CATEGORYCODE", "CATEGORYNAME", "ISACTIVE" };
            ////var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            ////// Get user input (e.g., from request)
            ////string sortBy = request.sortBy ?? "ROADMAPNAME";
            ////string sortDir = request.sortDirection ?? "ASC";

            ////// Validate input
            ////if (!allowedColumns.Contains(sortBy.ToUpper()))
            ////    sortBy = "ROADMAPNAME"; // default column

            ////if (!allowedDirections.Contains(sortDir.ToUpper()))
            ////    sortDir = "ASC"; // default direction
            var loggeduser = User.Identity.GetClaim("employeeid");
            var showAllUsers = request?.ShowAllUsers == true || HasSuperUserModule();
            var searchTerm = new ProjectExtendSearch
            {
                Search = searchValue,
                Status = request.Status,
                ProjectNo = request.ProjectNo,
                ProductCode = request.ProductCode,
                ParentType = request.ParentType,
                NodeType = request.NodeType,
                ProjectOwnerId = request.ProjectOwnerId,
                ProductGroupCode = request.ProductGroupCode,
                ProductDivisionCode = request.ProductDivisionCode,
                PlantCode = request.PlantCode,
                CategoryCode = request.CategoryCode,
                LoggedUser = showAllUsers ? null : loggeduser,
                OrderColumn = request.OrderColumn ?? "projectno",
                OrderDir = request.OrderDir ?? "asc",
                StartIndex = startIndex,
                LengthCount = pageSize,
            };

            var pagedResult = await _projectService.GetPagedFullProjectsAsync(searchTerm);

            // Prepare DataTables response
            var response = new DataTablesResponse<ProjectExtend>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).ToList()
            };

            return Ok(response);
        }


        [HttpPost]
        [Route("withdetails/datatables")]
        public async Task<IHttpActionResult> GetProjectsWithDetailsForDataTables([FromBody] DataTablesRequestProject request)
        {

            int pageNumber = 1;
            int pageSize = request.length;

            // If length == -1, fetch all rows
            if (request.length == -1)
            {
                pageSize = int.MaxValue; // or a large number, or remove paging logic
            }
            else
            {
                pageNumber = (request.start / request.length) + 1;
            }
            int startIndex = (pageNumber - 1) * pageSize;
            string searchValue = request.search?.value ?? "";
            ////bool? isActive = request.isActive;

            ////// Whitelist of allowed columns and directions
            ////var allowedColumns = new HashSet<string> { "ROADMAPSYSID", "ROADMAPNAME", "ROADMAPDESCRIPTION", "CATEGORYCODE", "CATEGORYNAME", "ISACTIVE" };
            ////var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            ////// Get user input (e.g., from request)
            ////string sortBy = request.sortBy ?? "ROADMAPNAME";
            ////string sortDir = request.sortDirection ?? "ASC";

            ////// Validate input
            ////if (!allowedColumns.Contains(sortBy.ToUpper()))
            ////    sortBy = "ROADMAPNAME"; // default column

            ////if (!allowedDirections.Contains(sortDir.ToUpper()))
            ////    sortDir = "ASC"; // default direction
            var loggeduser = User.Identity.GetClaim("employeeid");
            var showAllUsers = request?.ShowAllUsers == true || HasSuperUserModule();
            var searchTerm = new ProjectExtendSearch
            {
                Search = searchValue,
                ProjectOwnerId = request.ProjectOwnerId,
                ProductGroupCode = request.ProductGroupCode,
                ProductDivisionCode = request.ProductDivisionCode,
                PlantCode = request.PlantCode,
                CategoryCode = request.CategoryCode,
                LoggedUser = showAllUsers ? null : loggeduser,
                OrderColumn = request.OrderColumn ?? "projectno",
                OrderDir = request.OrderDir ?? "asc",
                StartIndex = startIndex,
                LengthCount = pageSize,
            };

            var pagedResult = await _projectService.GetPagedFullProjectsAsync(searchTerm);

            // Prepare DataTables response
            var response = new DataTablesResponse<ProjectExtend>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).ToList()
            };

            return Ok(response);
        }

        private static string NormalizeBoardStatus(string status)
        {
            var normalized = (status ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized == "NOT_STARTED" || normalized == "NOTSTARTED")
            {
                return "NOT STARTED";
            }

            if (normalized == "COMPLETED")
            {
                return "COMPLETED";
            }

            if (normalized == "CANCEL" || normalized == "CANCELLED" || normalized == "CANCELED")
            {
                return "CANCELLED";
            }

            if (normalized == "ARCHIVED" || normalized == "ARCHIVE")
            {
                return "ARCHIVED";
            }

            if (normalized == "ONGOING" || normalized == "HOLD" || normalized == "NOT STARTED" || normalized == "CANCELLED" || normalized == "COMPLETED" || normalized == "ARCHIVED")
            {
                return normalized;
            }

            return "NOT STARTED";
        }

        private static string NormalizeBoardMode(string mode)
        {
            var normalized = (mode ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "milestones" || normalized == "milestone")
            {
                return "milestones";
            }

            if (normalized == "tasks" || normalized == "task")
            {
                return "tasks";
            }

            return "projects";
        }

        private static List<OwnerBoardUserData> ParseOwnerBoardUsers(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return new List<OwnerBoardUserData>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<OwnerBoardUserData>>(rawJson) ?? new List<OwnerBoardUserData>();
            }
            catch
            {
                return new List<OwnerBoardUserData>();
            }
        }

        private static bool IsNodeOwnedByUser(ProjectExtend node, string loggedUser)
        {
            if (node == null || string.IsNullOrWhiteSpace(loggedUser))
            {
                return false;
            }

            return ParseOwnerBoardUsers(node.JsonNodeOwners).Any(owner =>
                !string.IsNullOrWhiteSpace(owner?.UserId)
                && string.Equals(owner.UserId, loggedUser, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildBoardSearchBlob(params string[] values)
        {
            return string.Join(" ", (values ?? new string[0]).Where(value => !string.IsNullOrWhiteSpace(value))).Trim();
        }

        private static bool MatchesBoardQuery(string query, params string[] values)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            var normalizedQuery = query.Trim().ToLowerInvariant();
            return BuildBoardSearchBlob(values).ToLowerInvariant().Contains(normalizedQuery);
        }

        private static HashSet<string> ParseCsvToSet(string csv)
        {
            return new HashSet<string>(
                (csv ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .Where(value => !string.IsNullOrWhiteSpace(value)),
                StringComparer.OrdinalIgnoreCase);
        }

        private bool HasModuleCode(string moduleCode)
        {
            if (string.IsNullOrWhiteSpace(moduleCode))
            {
                return false;
            }

            var moduleCodes = ParseCsvToSet(User.Identity.GetClaim("modulecodes"));
            return moduleCodes.Contains(moduleCode.Trim()) || moduleCodes.Contains(SuperUserModuleCode);
        }

        private async Task<HashSet<string>> GetActivePlantCodesAsync(string loggedUserId)
        {
            if (HasSuperUserModule())
            {
                var allPlantCodes = ((await _projectService.GetAllProjectsAsync()) ?? Enumerable.Empty<Project>())
                    .Where(project => project != null && !string.IsNullOrWhiteSpace(project.PlantCode))
                    .Select(project => project.PlantCode.Trim());

                return new HashSet<string>(allPlantCodes, StringComparer.OrdinalIgnoreCase);
            }

            var memberships = ((await _plantMemberRepository.GetListAsync(null, loggedUserId)) ?? Enumerable.Empty<PlantMember>())
                .Where(member => member != null
                    && member.IsActive == 1
                    && !string.IsNullOrWhiteSpace(member.PlantCode))
                .Select(member => member.PlantCode.Trim());

            return new HashSet<string>(memberships, StringComparer.OrdinalIgnoreCase);
        }

        private static List<string> GetOwnerBoardDisabledLanes(string normalizedMode, bool hasAdvancedStatusModule)
        {
            if (hasAdvancedStatusModule)
            {
                return new List<string>();
            }

            if (string.Equals(normalizedMode, "tasks", StringComparison.OrdinalIgnoreCase))
            {
                return new List<string> { "CANCELLED", "ARCHIVED" };
            }

            return new List<string> { "COMPLETED", "CANCELLED", "ARCHIVED" };
        }

        private static bool IsOwnerBoardTransitionRestricted(string entityType, string normalizedStatus, bool hasAdvancedStatusModule)
        {
            if (hasAdvancedStatusModule)
            {
                return false;
            }

            var type = NormalizeEntityType(entityType);
            if (string.Equals(type, "TASK", StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(normalizedStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalizedStatus, "ARCHIVED", StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(normalizedStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedStatus, "ARCHIVED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> CanUpdateFromOwnerBoardAsync(string loggedUserId, string projectNo, bool hasAdvancedStatusModule)
        {
            if (HasSuperUserModule())
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(loggedUserId) || string.IsNullOrWhiteSpace(projectNo))
            {
                return false;
            }

            if (!hasAdvancedStatusModule)
            {
                return await IsProjectOwnerMemberAsync(loggedUserId, projectNo);
            }

            var project = await _projectService.GetProjectByIdAsync(projectNo);
            if (project == null || string.IsNullOrWhiteSpace(project.PlantCode))
            {
                return false;
            }

            return await HasActivePlantMembershipAsync(loggedUserId, project.PlantCode);
        }

        private async Task<List<ProjectExtend>> GetOwnerBoardNodesAsync(IEnumerable<string> ownerProjectNumbers)
        {
            var ownedProjectNumbers = (ownerProjectNumbers ?? Enumerable.Empty<string>())
                .Where(projectNo => !string.IsNullOrWhiteSpace(projectNo))
                .Select(projectNo => projectNo.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var ownerScopeNodes = new List<ProjectExtend>();

            foreach (var projectNo in ownedProjectNumbers)
            {
                var projectNodes = await _projectRepository.GetProjectNodesAsync(projectNo) ?? new List<ProjectExtend>();
                var scopedProjectNodes = projectNodes.Where(node => node != null
                    && !string.IsNullOrWhiteSpace(node.ProjectNo)
                    && string.Equals(node.ProjectNo.Trim(), projectNo, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (scopedProjectNodes.Any())
                {
                    ownerScopeNodes.AddRange(scopedProjectNodes);
                    continue;
                }

                var project = await _projectRepository.GetAsync(projectNo);
                if (project == null)
                {
                    continue;
                }

                ownerScopeNodes.Add(new ProjectExtend
                {
                    ProjectNo = project.ProjectNo,
                    ProjectName = project.ProjectName,
                    ProductGroupCode = project.ProductGroupCode,
                    ProductDivisionCode = project.ProductDivisionCode,
                    PlantCode = project.PlantCode,
                    CategoryCode = project.CategoryCode,
                    ProjectOwnerId = project.ProjectOwnerId,
                    ProjectOwnerUserName = project.ProjectOwnerUserName,
                    Status = project.Status,
                    TransactionKey = project.TransactionKey,
                    TargetStart = project.TargetStartDate,
                    TargetCompletion = project.TargetCompletionDate,
                    TargetCompletionDate = project.TargetCompletionDate
                });
            }

            return ownerScopeNodes;
        }

        private static string BuildNodeIdentity(ProjectExtend node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            return string.Join("|", new[]
            {
                node.NodeType ?? string.Empty,
                node.ProjectNodeSysId ?? string.Empty,
                node.NodeId ?? string.Empty,
                node.ProjectNo ?? string.Empty
            });
        }

        private static IEnumerable<ProjectExtend> GetDescendantNodes(ProjectExtend root, IEnumerable<ProjectExtend> allNodes)
        {
            if (root == null || allNodes == null)
            {
                return Enumerable.Empty<ProjectExtend>();
            }

            var nodes = allNodes.Where(node => node != null).ToList();
            var childrenByParent = nodes
                .Where(node => !string.IsNullOrWhiteSpace(node.ParentSysId))
                .GroupBy(node => node.ParentSysId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            var results = new List<ProjectExtend>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<ProjectExtend>();

            void EnqueueChildren(string parentKey)
            {
                if (string.IsNullOrWhiteSpace(parentKey))
                {
                    return;
                }

                if (!childrenByParent.TryGetValue(parentKey, out var children))
                {
                    return;
                }

                foreach (var child in children)
                {
                    var childIdentity = BuildNodeIdentity(child);
                    if (visited.Add(childIdentity))
                    {
                        stack.Push(child);
                    }
                }
            }

            EnqueueChildren(root.NodeId);
            EnqueueChildren(root.ProjectNodeSysId);

            while (stack.Any())
            {
                var node = stack.Pop();
                results.Add(node);
                EnqueueChildren(node.NodeId);
                EnqueueChildren(node.ProjectNodeSysId);
            }

            return results;
        }

        private static string ResolveTaskMaturityCode(ProjectExtend task, IEnumerable<ProjectExtend> allNodes)
        {
            if (task == null || allNodes == null)
            {
                return null;
            }

            var lookup = allNodes
                .Where(node => node != null)
                .SelectMany(node => new[]
                {
                    new { Key = node.NodeId, Node = node },
                    new { Key = node.ProjectNodeSysId, Node = node }
                })
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Key))
                .GroupBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Node, StringComparer.OrdinalIgnoreCase);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentParent = task.ParentSysId;

            while (!string.IsNullOrWhiteSpace(currentParent) && visited.Add(currentParent))
            {
                if (!lookup.TryGetValue(currentParent, out var parent))
                {
                    break;
                }

                if (string.Equals(parent.NodeType, "milestone", StringComparison.OrdinalIgnoreCase))
                {
                    return parent.NodeMaturityCode;
                }

                currentParent = parent.ParentSysId;
            }

            return null;
        }

        private static string ResolveParentNodeName(ProjectExtend node, IEnumerable<ProjectExtend> allNodes)
        {
            if (node == null || allNodes == null || string.IsNullOrWhiteSpace(node.ParentSysId))
            {
                return null;
            }

            var parent = allNodes.FirstOrDefault(candidate => candidate != null
                && (
                    string.Equals(candidate.NodeId, node.ParentSysId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.ProjectNodeSysId, node.ParentSysId, StringComparison.OrdinalIgnoreCase)
                ));

            return string.IsNullOrWhiteSpace(parent?.NodeName) ? null : parent.NodeName;
        }

        private static IEnumerable<string> ParsePrerequisiteNames(string prerequisitesJson)
        {
            return ParsePrerequisiteItems(prerequisitesJson)
                .Select(item => item.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsPrerequisiteSatisfiedStatus(string status)
        {
            var normalized = (status ?? string.Empty).Trim().ToUpperInvariant();
            return normalized == "COMPLETED"
                || normalized == "ARCHIVED"
                || normalized == "CANCELLED"
                || normalized == "CANCELED";
        }

        private static IReadOnlyList<OwnerBoardPrerequisiteItem> ParsePrerequisiteItems(string prerequisitesJson)
        {
            if (string.IsNullOrWhiteSpace(prerequisitesJson))
            {
                return Array.Empty<OwnerBoardPrerequisiteItem>();
            }

            try
            {
                var payload = JsonConvert.DeserializeObject<OwnerBoardPrerequisitePayload>(prerequisitesJson);
                return (payload?.Prerequisites ?? new List<OwnerBoardPrerequisiteItem>())
                    .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Name))
                    .Select(item =>
                    {
                        item.Name = item.Name?.Trim();
                        item.Status = item.Status?.Trim();
                        return item;
                    })
                    .Where(item => !string.IsNullOrWhiteSpace(item.Name))
                    .ToList();
            }
            catch
            {
                return Array.Empty<OwnerBoardPrerequisiteItem>();
            }
        }

        private static string NormalizeEntityType(string entityType)
        {
            var normalized = (entityType ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized == "MILESTONE" || normalized == "TASK")
            {
                return normalized;
            }

            return "PROJECT";
        }

        private static bool IsTaskNode(ProjectExtend node)
        {
            if (node == null)
            {
                return false;
            }

            return string.Equals(node.NodeType, "task", StringComparison.OrdinalIgnoreCase)
                || string.Equals(node.NodeType, "activity", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsClosedOrCancelledStatus(string status)
        {
            var normalized = NormalizeBoardStatus(status);
            return string.Equals(normalized, "COMPLETED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "CANCELLED", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNotStartedToOngoingTransition(string currentStatus, string nextStatus)
        {
            return string.Equals(NormalizeBoardStatus(currentStatus), "NOT STARTED", StringComparison.OrdinalIgnoreCase)
                && string.Equals(NormalizeBoardStatus(nextStatus), "ONGOING", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetInnermostExceptionMessage(Exception ex)
        {
            if (ex == null)
            {
                return null;
            }

            var current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }

        private async Task<bool> StartMilestoneNodeIfAllowedAsync(ProjectExtend milestoneNode, string loggedUser, string reason)
        {
            if (milestoneNode == null
                || string.IsNullOrWhiteSpace(milestoneNode.ProjectNodeSysId)
                || IsClosedOrCancelledStatus(milestoneNode.ProjectNodeStatus ?? milestoneNode.Status))
            {
                return false;
            }

            var milestone = await _projectMilestoneService.GetProjectMilestoneByIdAsync(milestoneNode.ProjectNodeSysId);
            if (milestone == null || IsClosedOrCancelledStatus(milestone.Status))
            {
                return false;
            }

            var currentStatus = NormalizeBoardStatus(milestone.Status);
            if (string.Equals(currentStatus, "ONGOING", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            milestone.ModifiedBy = loggedUser;
            milestone.ActualStartDate = milestone.ActualStartDate ?? DateTime.UtcNow;

            if (string.Equals(currentStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
            {
                await _projectMilestoneService.UnholdAsync(milestone, reason, true, false, true);
                return true;
            }

            await _projectMilestoneService.StartAsync(milestone, reason, true, false, true);
            return true;
        }

        private async Task<bool> StartTaskNodeIfAllowedAsync(string projectNo, ProjectExtend taskNode, string loggedUser, string reason)
        {
            if (taskNode == null || IsClosedOrCancelledStatus(taskNode.ProjectNodeStatus ?? taskNode.Status))
            {
                return false;
            }

            var task = !string.IsNullOrWhiteSpace(taskNode.ProjectNodeSysId)
                ? await _projectTaskService.GetTaskByIdAsync(taskNode.ProjectNodeSysId)
                : null;

            if (task == null && !string.IsNullOrWhiteSpace(taskNode.NodeId))
            {
                task = await MaterializeBoardTaskAsync(loggedUser, projectNo, taskNode.NodeId);
            }

            if (task == null || IsClosedOrCancelledStatus(task.Status))
            {
                return false;
            }

            var currentStatus = NormalizeBoardStatus(task.Status);
            if (string.Equals(currentStatus, "ONGOING", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            task.ModifiedBy = loggedUser;
            task.ActualStartDate = task.ActualStartDate ?? DateTime.UtcNow;

            if (string.Equals(currentStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
            {
                await _projectTaskService.UnholdAsync(task, reason, false, false, true);
                return true;
            }

            await _projectTaskService.StartAsync(task, reason, false, false, true);
            return true;
        }

        private async Task<int> CascadeStartDescendantsAsync(string projectNo, ProjectExtend rootNode, IEnumerable<ProjectExtend> allNodes, string loggedUser, string reason)
        {
            var descendants = GetDescendantNodes(rootNode, allNodes).ToList();
            var startedCount = 0;
            foreach (var descendant in descendants)
            {
                if (descendant == null || IsClosedOrCancelledStatus(descendant.ProjectNodeStatus ?? descendant.Status))
                {
                    continue;
                }

                if (string.Equals(descendant.NodeType, "milestone", StringComparison.OrdinalIgnoreCase))
                {
                    if (await StartMilestoneNodeIfAllowedAsync(descendant, loggedUser, reason))
                    {
                        startedCount++;
                    }
                    continue;
                }

                if (IsTaskNode(descendant))
                {
                    if (await StartTaskNodeIfAllowedAsync(projectNo, descendant, loggedUser, reason))
                    {
                        startedCount++;
                    }
                }
            }

            return startedCount;
        }

        private async Task<ProjectTask> MaterializeBoardTaskAsync(string loggedUser, string projectNo, string roadmapActivitySysId)
        {
            var project = await _projectService.GetProjectByIdAsync(projectNo);
            if (project == null)
            {
                return null;
            }

            var task = new ProjectTask
            {
                ProjectNo = projectNo,
                RoadmapActivitySysId = roadmapActivitySysId,
                RoadmapSysId = project.RoadmapSysId,
                PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                Status = "NOT STARTED",
                CreatedBy = loggedUser
            };

            task.ProjectTaskSysId = await _projectTaskService.AddTaskAsync(task, loggedUser);
            return task;
        }

        private async Task AddBoardCommentAsync(string projectNo, string entityType, string entitySysId, string comment, string loggedUser)
        {
            await _projectCommentService.AddAsync(new ProjectComment
            {
                ProjectNo = projectNo,
                EntityType = entityType,
                EntitySysId = entitySysId,
                Comments = comment,
                CommentsRichText = null,
                CreatedBy = loggedUser,
                CreatedDate = DateTime.Now
            });
        }

        [HttpGet]
        [Route("owner-board")]
        public async Task<IHttpActionResult> GetOwnerBoardProjects([FromUri] string mode = null, [FromUri] string query = null, [FromUri] string status = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var normalizedMode = NormalizeBoardMode(mode);
            var normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
            var loggedUser = User.Identity.GetClaim("employeeid");
            var isSuperUser = HasSuperUserModule();
            var hasAdvancedStatusModule = HasModuleCode("ADVCHSTAT");
            var disabledLanes = GetOwnerBoardDisabledLanes(normalizedMode, hasAdvancedStatusModule);

            Func<object> permissionPayload = () => new
            {
                hasAdvancedStatusModule,
                accessScope = isSuperUser ? "ALL" : hasAdvancedStatusModule ? "PLANT" : "OWNER",
                disabledLanes
            };

            if (string.IsNullOrWhiteSpace(loggedUser))
            {
                return Ok(new
                {
                    mode = normalizedMode,
                    data = new List<object>(),
                    count = 0,
                    permissions = permissionPayload()
                });
            }

            HashSet<string> accessibleProjectSet;
            if (isSuperUser)
            {
                var allProjects = (await _projectService.GetAllProjectsAsync()) ?? Enumerable.Empty<Project>();
                accessibleProjectSet = new HashSet<string>(
                    allProjects
                        .Where(project => project != null && !string.IsNullOrWhiteSpace(project.ProjectNo))
                        .Select(project => project.ProjectNo.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            }
            else if (hasAdvancedStatusModule)
            {
                var activePlantCodes = await GetActivePlantCodesAsync(loggedUser);
                if (!activePlantCodes.Any())
                {
                    return Ok(new
                    {
                        mode = normalizedMode,
                        data = new List<object>(),
                        count = 0,
                        permissions = permissionPayload()
                    });
                }

                var allProjects = (await _projectService.GetAllProjectsAsync()) ?? Enumerable.Empty<Project>();
                accessibleProjectSet = new HashSet<string>(
                    allProjects
                        .Where(project => project != null
                            && !string.IsNullOrWhiteSpace(project.ProjectNo)
                            && !string.IsNullOrWhiteSpace(project.PlantCode)
                            && activePlantCodes.Contains(project.PlantCode.Trim()))
                        .Select(project => project.ProjectNo.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var ownerMemberships = ((await _projectMemberRepository.GetByMemberIdAsync(loggedUser)) ?? Enumerable.Empty<ProjectMember>())
                    .Where(member => member != null
                        && member.IsOwner == 1
                        && !string.IsNullOrWhiteSpace(member.ProjectNo))
                    .Select(member => member.ProjectNo.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                accessibleProjectSet = new HashSet<string>(ownerMemberships, StringComparer.OrdinalIgnoreCase);

                var directOwnerProjects = ((await _projectService.GetAllProjectsAsync()) ?? Enumerable.Empty<Project>())
                    .Where(project => project != null
                        && !string.IsNullOrWhiteSpace(project.ProjectNo)
                        && string.Equals(project.ProjectOwnerId, loggedUser, StringComparison.OrdinalIgnoreCase))
                    .Select(project => project.ProjectNo.Trim());

                accessibleProjectSet.UnionWith(directOwnerProjects);
            }

            if (!accessibleProjectSet.Any())
            {
                return Ok(new
                {
                    mode = normalizedMode,
                    data = new List<object>(),
                    count = 0,
                    permissions = permissionPayload()
                });
            }

            var ownerScopeNodes = await GetOwnerBoardNodesAsync(accessibleProjectSet);

            var ownedProjects = ownerScopeNodes
                .GroupBy(project => project.ProjectNo, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(project => project.ProjectName)
                .ToList();

            var nodeCache = ownerScopeNodes
                .GroupBy(node => node.ProjectNo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Where(node => node != null).ToList(),
                    StringComparer.OrdinalIgnoreCase);

            IEnumerable<object> data;
            if (normalizedMode == "milestones")
            {
                data = ownedProjects.SelectMany(project =>
                {
                    var nodes = nodeCache[project.ProjectNo];
                    return nodes
                        .Where(node => string.Equals(node.NodeType, "milestone", StringComparison.OrdinalIgnoreCase))
                        .Select(node =>
                        {
                            var descendantTasks = GetDescendantNodes(node, nodes)
                                .Where(descendant => IsTaskNode(descendant))
                                .ToList();
                            var nodeStatus = NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status);
                            return new
                            {
                                entityType = "MILESTONE",
                                entitySysId = node.ProjectNodeSysId,
                                nodeId = node.NodeId,
                                projectNo = project.ProjectNo,
                                projectName = project.ProjectName,
                                name = node.NodeName,
                                maturityCode = node.NodeMaturityCode,
                                productCodes = project.ProductCodes,
                                plantCode = project.PlantCode,
                                categoryCode = project.CategoryCode,
                                status = nodeStatus,
                                rawStatus = node.ProjectNodeStatus ?? node.Status,
                                transactionKey = node.TransactionKey,
                                targetDate = node.ProjectNodeTargetCompletionDate ?? node.ProjectNodeTargetCompletion,
                                taskCount = descendantTasks.Count,
                                taskPendingCount = descendantTasks.Count(task => NormalizeBoardStatus(task.ProjectNodeStatus ?? task.Status) == "NOT STARTED"),
                                taskOngoingCount = descendantTasks.Count(task => NormalizeBoardStatus(task.ProjectNodeStatus ?? task.Status) == "ONGOING"),
                                taskHoldCount = descendantTasks.Count(task => NormalizeBoardStatus(task.ProjectNodeStatus ?? task.Status) == "HOLD"),
                                taskClosedCount = descendantTasks.Count(task => NormalizeBoardStatus(task.ProjectNodeStatus ?? task.Status) == "COMPLETED")
                            };
                        });
                }).ToList();
            }
            else if (normalizedMode == "tasks")
            {
                data = ownedProjects.SelectMany(project =>
                {
                    var nodes = nodeCache[project.ProjectNo];
                    return nodes
                        .Where(node => IsTaskNode(node))
                        .Select(node =>
                        {
                            var prerequisiteItems = ParsePrerequisiteItems(node.PrerequisitesJson);
                            return new
                            {
                                entityType = "TASK",
                                entitySysId = node.ProjectNodeSysId,
                                nodeId = node.NodeId,
                                projectNo = project.ProjectNo,
                                projectName = project.ProjectName,
                                name = node.NodeName,
                                maturityCode = ResolveTaskMaturityCode(node, nodes),
                                productCodes = project.ProductCodes,
                                plantCode = project.PlantCode,
                                categoryCode = project.CategoryCode,
                                status = NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status),
                                rawStatus = node.ProjectNodeStatus ?? node.Status,
                                transactionKey = node.TransactionKey,
                                targetDate = node.ProjectNodeTargetCompletionDate ?? node.ProjectNodeTargetCompletion,
                                parentNodeName = ResolveParentNodeName(node, nodes),
                                prerequisites = prerequisiteItems.Select(item => item.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                                prerequisitesTotalCount = prerequisiteItems.Count,
                                prerequisitesSatisfiedCount = prerequisiteItems.Count(item => IsPrerequisiteSatisfiedStatus(item.Status))
                            };
                        });
                }).ToList();
            }
            else
            {
                data = ownedProjects.Select(project =>
                {
                    var nodes = nodeCache[project.ProjectNo];
                    var milestones = nodes.Where(node => string.Equals(node.NodeType, "milestone", StringComparison.OrdinalIgnoreCase)).ToList();
                    var tasks = nodes.Where(node => IsTaskNode(node)).ToList();

                    return new
                    {
                        entityType = "PROJECT",
                        entitySysId = project.ProjectNo,
                        nodeId = project.ProjectNo,
                        projectNo = project.ProjectNo,
                        projectName = project.ProjectName,
                        name = project.ProjectName,
                        productCodes = project.ProductCodes,
                        plantCode = project.PlantCode,
                        categoryCode = project.CategoryCode,
                        status = NormalizeBoardStatus(project.Status),
                        rawStatus = project.Status,
                        transactionKey = project.TransactionKey,
                        targetDate = project.TargetCompletionDate ?? project.TargetCompletion,
                        milestoneCount = milestones.Count,
                        milestonePendingCount = milestones.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "NOT STARTED"),
                        milestoneOngoingCount = milestones.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "ONGOING"),
                        milestoneHoldCount = milestones.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "HOLD"),
                        milestoneClosedCount = milestones.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "COMPLETED"),
                        taskCount = tasks.Count,
                        taskPendingCount = tasks.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "NOT STARTED"),
                        taskOngoingCount = tasks.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "ONGOING"),
                        taskHoldCount = tasks.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "HOLD"),
                        taskClosedCount = tasks.Count(node => NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status) == "COMPLETED"),
                        taskAtRiskCount = tasks.Count(node => string.Equals((node.ProjectNodeStatus ?? node.Status ?? string.Empty).Trim(), "AT RISK", StringComparison.OrdinalIgnoreCase))
                    };
                }).ToList();
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                if (normalizedMode == "milestones")
                {
                    data = data.Cast<dynamic>().Where(item => MatchesBoardQuery(query,
                        item.name,
                        item.maturityCode,
                        item.projectNo,
                        item.productCodes,
                        item.plantCode,
                        item.categoryCode)).ToList();
                }
                else if (normalizedMode == "tasks")
                {
                    data = data.Cast<dynamic>().Where(item => MatchesBoardQuery(query,
                        item.name,
                        item.maturityCode,
                        item.projectNo,
                        item.productCodes,
                        item.plantCode,
                        item.categoryCode)).ToList();
                }
                else
                {
                    data = data.Cast<dynamic>().Where(item => MatchesBoardQuery(query,
                        item.name,
                        item.projectNo,
                        item.productCodes,
                        item.plantCode,
                        item.categoryCode)).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedStatus))
            {
                var expectedStatus = NormalizeBoardStatus(normalizedStatus);
                data = data.Cast<dynamic>()
                    .Where(item => string.Equals(NormalizeBoardStatus((string)item.status), expectedStatus, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(new
            {
                mode = normalizedMode,
                data = data,
                count = data.Count(),
                permissions = permissionPayload()
            });
        }

        [HttpPost]
        [Route("owner-board/status")]
        public async Task<IHttpActionResult> UpdateOwnerBoardStatus([FromBody] ProjectBoardStatusUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.ProjectNo))
            {
                return BadRequest("Project number is required.");
            }

            var comment = (request.Comment ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(comment))
            {
                return BadRequest("Comment is required when moving an item to another lane.");
            }

            var loggedUser = User.Identity.GetClaim("employeeid");
            var entityType = NormalizeEntityType(request.EntityType);
            var nextLaneStatus = NormalizeBoardStatus(request.NewStatus);
            var hasAdvancedStatusModule = HasModuleCode("ADVCHSTAT");
            string cascadeMessage = null;

            if (IsOwnerBoardTransitionRestricted(entityType, nextLaneStatus, hasAdvancedStatusModule))
            {
                return ForbiddenProjectAccess("You are not allowed to move this item to the selected lane.");
            }

            if (entityType == "PROJECT")
            {
                if (!await CanUpdateFromOwnerBoardAsync(loggedUser, request.ProjectNo, hasAdvancedStatusModule))
                {
                    return ForbiddenProjectAccess(hasAdvancedStatusModule
                        ? "You are not authorized to update this project from the selected plant scope."
                        : "Only owner members can update project status from the board.");
                }

                var project = await _projectService.GetProjectByIdAsync(request.ProjectNo);
                if (project == null)
                {
                    return NotFound();
                }

                var currentLaneStatus = NormalizeBoardStatus(project.Status);
                if (string.Equals(currentLaneStatus, nextLaneStatus, StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new
                    {
                        entityType,
                        entitySysId = request.ProjectNo,
                        projectNo = request.ProjectNo,
                        status = currentLaneStatus,
                        transactionKey = project.TransactionKey,
                        message = "Project status is already in the selected lane."
                    });
                }

                var reason = $"Change status: {currentLaneStatus} to {nextLaneStatus} | {comment}";
                project.ModifiedBy = loggedUser;
                project.TransactionKey = string.IsNullOrWhiteSpace(request.TransactionKey) ? project.TransactionKey : request.TransactionKey;
                try
                {
                    if (string.Equals(nextLaneStatus, "NOT STARTED", StringComparison.OrdinalIgnoreCase))
                    {
                        await _projectService.InitializeAsync(project, reason, true, false, true);
                    }
                    else if (string.Equals(nextLaneStatus, "ONGOING", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(currentLaneStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
                        {
                            await _projectService.UnholdAsync(project, reason, true, false, true);
                        }
                        else
                        {
                            project.ActualStartDate = DateTime.UtcNow;
                            await _projectService.StartAsync(project, reason, true, false, true);
                        }
                    }
                    else if (string.Equals(nextLaneStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
                    {
                        await _projectService.HoldAsync(project, reason, true, false, true);
                    }
                    else if (string.Equals(nextLaneStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                    {
                        await _projectService.CancelAsync(project, reason, true, false, true);
                    }
                    else if (string.Equals(nextLaneStatus, "ARCHIVED", StringComparison.OrdinalIgnoreCase))
                    {
                        await _projectService.ArchiveAsync(project, reason, true, false, true);
                    }
                    else
                    {
                        await _projectService.CompleteAsync(project, reason, true, false, true);
                    }

                    if (IsNotStartedToOngoingTransition(currentLaneStatus, nextLaneStatus))
                    {
                        var projectNodes = await _projectService.GetProjectNodesAsync(request.ProjectNo) ?? new List<ProjectExtend>();
                        var milestoneNodes = projectNodes
                            .Where(node => node != null && string.Equals(node.NodeType, "milestone", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        var activeMilestones = milestoneNodes
                            .Where(node => !IsClosedOrCancelledStatus(node.ProjectNodeStatus ?? node.Status))
                            .ToList();
                        var hasOngoingMilestone = activeMilestones.Any(node =>
                            string.Equals(NormalizeBoardStatus(node.ProjectNodeStatus ?? node.Status), "ONGOING", StringComparison.OrdinalIgnoreCase));

                        if (!hasOngoingMilestone)
                        {
                            var firstMilestone = activeMilestones
                                .OrderBy(node => node.OrderIndex)
                                .ThenBy(node => node.NodeName)
                                .FirstOrDefault();

                            if (firstMilestone != null)
                            {
                                var cascadeReason = reason + " | Auto-cascade start from project board move";
                                var startedCount = 0;
                                if (await StartMilestoneNodeIfAllowedAsync(firstMilestone, loggedUser, cascadeReason))
                                {
                                    startedCount++;
                                }

                                startedCount += await CascadeStartDescendantsAsync(request.ProjectNo, firstMilestone, projectNodes, loggedUser, cascadeReason);
                                if (startedCount > 0)
                                {
                                    cascadeMessage = $"Auto-started {startedCount} related item(s).";
                                }
                            }
                        }
                    }

                    await AddBoardCommentAsync(request.ProjectNo, entityType, request.ProjectNo, reason, loggedUser);
                    var updatedProject = await _projectService.GetProjectByIdAsync(request.ProjectNo);

                    return Content(HttpStatusCode.Accepted, new
                    {
                        entityType,
                        entitySysId = request.ProjectNo,
                        projectNo = request.ProjectNo,
                        status = NormalizeBoardStatus(updatedProject?.Status),
                        rawStatus = updatedProject?.Status,
                        transactionKey = updatedProject?.TransactionKey,
                        cascadeMessage,
                        message = "Status updated successfully."
                    });
                }
                catch (Exception ex)
                {
                    var errorMessage = GetInnermostExceptionMessage(ex);
                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = "Unable to update project status from the board.";
                    }

                    return Content(HttpStatusCode.Conflict, new
                    {
                        entityType,
                        entitySysId = request.ProjectNo,
                        projectNo = request.ProjectNo,
                        status = currentLaneStatus,
                        rawStatus = project.Status,
                        transactionKey = project.TransactionKey,
                        message = errorMessage
                    });
                }
            }

            var nodes = await _projectService.GetProjectNodesAsync(request.ProjectNo) ?? new List<ProjectExtend>();
            var targetNode = nodes.FirstOrDefault(node => node != null
                && (entityType.Equals("MILESTONE", StringComparison.OrdinalIgnoreCase)
                    ? string.Equals(node.NodeType, "milestone", StringComparison.OrdinalIgnoreCase)
                    : IsTaskNode(node))
                && (
                    (!string.IsNullOrWhiteSpace(request.EntitySysId) && string.Equals(node.ProjectNodeSysId, request.EntitySysId, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(request.NodeId) && string.Equals(node.NodeId, request.NodeId, StringComparison.OrdinalIgnoreCase))
                ));

            if (targetNode == null)
            {
                return NotFound();
            }

            if (!await CanUpdateFromOwnerBoardAsync(loggedUser, request.ProjectNo, hasAdvancedStatusModule))
            {
                return ForbiddenProjectAccess(hasAdvancedStatusModule
                    ? "You are not authorized to update this item from the selected plant scope."
                    : "Only owner members can update this status from the board.");
            }

            var currentNodeStatus = NormalizeBoardStatus(targetNode.ProjectNodeStatus ?? targetNode.Status);
            if (string.Equals(currentNodeStatus, nextLaneStatus, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new
                {
                    entityType,
                    entitySysId = targetNode.ProjectNodeSysId,
                    projectNo = request.ProjectNo,
                    status = currentNodeStatus,
                    transactionKey = targetNode.TransactionKey,
                    message = "Item status is already in the selected lane."
                });
            }

            var nodeReason = $"Change status: {currentNodeStatus} to {nextLaneStatus} | {comment}";

            if (entityType == "MILESTONE")
            {
                var milestone = await _projectMilestoneService.GetProjectMilestoneByIdAsync(targetNode.ProjectNodeSysId);
                if (milestone == null)
                {
                    return NotFound();
                }

                milestone.ModifiedBy = loggedUser;
                milestone.TransactionKey = string.IsNullOrWhiteSpace(request.TransactionKey) ? milestone.TransactionKey : request.TransactionKey;
                milestone.ActualStartDate = milestone.ActualStartDate ?? DateTime.UtcNow;
                milestone.ActualCompletionDate = milestone.ActualCompletionDate ?? DateTime.UtcNow;

                if (string.Equals(nextLaneStatus, "NOT STARTED", StringComparison.OrdinalIgnoreCase))
                {
                    await _projectMilestoneService.InitializeAsync(milestone, nodeReason, true, false, true);
                }
                else if (string.Equals(nextLaneStatus, "ONGOING", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(currentNodeStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
                    {
                        await _projectMilestoneService.UnholdAsync(milestone, nodeReason, true, false, true);
                    }
                    else
                    {
                        milestone.ActualStartDate = DateTime.UtcNow;
                        await _projectMilestoneService.StartAsync(milestone, nodeReason, true, false, true);
                    }
                }
                else if (string.Equals(nextLaneStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
                {
                    await _projectMilestoneService.HoldAsync(milestone, nodeReason, true, false, true);
                }
                else if (string.Equals(nextLaneStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    milestone.ActualCompletionDate = DateTime.UtcNow;
                    await _projectMilestoneService.CancelAsync(milestone, nodeReason, true, false, true);
                }
                else if (string.Equals(nextLaneStatus, "ARCHIVED", StringComparison.OrdinalIgnoreCase))
                {
                    milestone.ActualCompletionDate = DateTime.UtcNow;
                    await _projectMilestoneService.ArchiveAsync(milestone, nodeReason, true, false, true);
                }
                else
                {
                    milestone.ActualCompletionDate = DateTime.UtcNow;
                    await _projectMilestoneService.CompleteAsync(milestone, nodeReason, true, false, true);
                }

                if (IsNotStartedToOngoingTransition(currentNodeStatus, nextLaneStatus))
                {
                    var cascadeReason = nodeReason + " | Auto-cascade start from milestone board move";
                    var startedCount = await CascadeStartDescendantsAsync(request.ProjectNo, targetNode, nodes, loggedUser, cascadeReason);
                    if (startedCount > 0)
                    {
                        cascadeMessage = $"Auto-started {startedCount} descendant item(s).";
                    }
                }

                await AddBoardCommentAsync(request.ProjectNo, entityType, targetNode.ProjectNodeSysId, nodeReason, loggedUser);
                var updatedMilestone = await _projectMilestoneService.GetProjectMilestoneByIdAsync(targetNode.ProjectNodeSysId);

                return Content(HttpStatusCode.Accepted, new
                {
                    entityType,
                    entitySysId = targetNode.ProjectNodeSysId,
                    nodeId = targetNode.NodeId,
                    projectNo = request.ProjectNo,
                    status = NormalizeBoardStatus(updatedMilestone?.Status),
                    rawStatus = updatedMilestone?.Status,
                    transactionKey = updatedMilestone?.TransactionKey,
                    cascadeMessage,
                    message = "Status updated successfully."
                });
            }

            var task = !string.IsNullOrWhiteSpace(targetNode.ProjectNodeSysId)
                ? await _projectTaskService.GetTaskByIdAsync(targetNode.ProjectNodeSysId)
                : null;
            if (task == null && !string.IsNullOrWhiteSpace(targetNode.NodeId)
                && !string.Equals(nextLaneStatus, "NOT STARTED", StringComparison.OrdinalIgnoreCase))
            {
                task = await MaterializeBoardTaskAsync(loggedUser, request.ProjectNo, targetNode.NodeId);
            }

            if (task == null)
            {
                return Content(HttpStatusCode.Accepted, new
                {
                    entityType,
                    entitySysId = targetNode.ProjectNodeSysId,
                    nodeId = targetNode.NodeId,
                    projectNo = request.ProjectNo,
                    status = "NOT STARTED",
                    rawStatus = "NOT STARTED",
                    transactionKey = targetNode.TransactionKey,
                    message = "Task is already in not started status."
                });
            }

            task.ModifiedBy = loggedUser;
            task.TransactionKey = string.IsNullOrWhiteSpace(request.TransactionKey) ? task.TransactionKey : request.TransactionKey;

            if (string.Equals(nextLaneStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                var blockingPrerequisites = ParsePrerequisiteItems(targetNode.PrerequisitesJson)
                    .Where(item =>
                    {
                        var prerequisiteStatus = NormalizeBoardStatus(item.Status);
                        return string.Equals(prerequisiteStatus, "NOT STARTED", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prerequisiteStatus, "ONGOING", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prerequisiteStatus, "HOLD", StringComparison.OrdinalIgnoreCase);
                    })
                    .Select(item => item.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (blockingPrerequisites.Any())
                {
                    return Content(HttpStatusCode.Conflict, new
                    {
                        entityType,
                        entitySysId = task.ProjectTaskSysId,
                        nodeId = targetNode.NodeId,
                        projectNo = request.ProjectNo,
                        status = currentNodeStatus,
                        rawStatus = task.Status,
                        transactionKey = task.TransactionKey,
                        blockingPrerequisites,
                        message = "Task cannot be completed while prerequisite tasks are still not started, ongoing, or on hold."
                    });
                }
            }

            if (string.Equals(nextLaneStatus, "NOT STARTED", StringComparison.OrdinalIgnoreCase))
            {
                await _projectTaskService.InitializeAsync(task, nodeReason, false, false, true);
            }
            else if (string.Equals(nextLaneStatus, "ONGOING", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(currentNodeStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
                {
                    await _projectTaskService.UnholdAsync(task, nodeReason, false, false, true);
                }
                else
                {
                    task.ActualStartDate = DateTime.UtcNow;
                    await _projectTaskService.StartAsync(task, nodeReason, false, false, true);
                }
            }
            else if (string.Equals(nextLaneStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
            {
                await _projectTaskService.HoldAsync(task, nodeReason, false, false, true);
            }
            else if (string.Equals(nextLaneStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                task.ActualCompletionDate = DateTime.UtcNow;
                await _projectTaskService.CancelAsync(task, nodeReason, false, false, true);
            }
            else if (string.Equals(nextLaneStatus, "ARCHIVED", StringComparison.OrdinalIgnoreCase))
            {
                task.ActualCompletionDate = DateTime.UtcNow;
                await _projectTaskService.ArchiveAsync(task, nodeReason, false, false, true);
            }
            else
            {
                task.ActualCompletionDate = DateTime.UtcNow;
                await _projectTaskService.CompleteAsync(task, nodeReason, false, false, true);
            }

            if (IsNotStartedToOngoingTransition(currentNodeStatus, nextLaneStatus))
            {
                var cascadeReason = nodeReason + " | Auto-cascade start from task board move";
                var startedCount = await CascadeStartDescendantsAsync(request.ProjectNo, targetNode, nodes, loggedUser, cascadeReason);
                if (startedCount > 0)
                {
                    cascadeMessage = $"Auto-started {startedCount} descendant item(s).";
                }
            }

            await AddBoardCommentAsync(request.ProjectNo, entityType, task.ProjectTaskSysId, nodeReason, loggedUser);
            var updatedTask = await _projectTaskService.GetTaskByIdAsync(task.ProjectTaskSysId);

            return Content(HttpStatusCode.Accepted, new
            {
                entityType,
                entitySysId = updatedTask?.ProjectTaskSysId,
                nodeId = targetNode.NodeId,
                projectNo = request.ProjectNo,
                status = NormalizeBoardStatus(updatedTask?.Status),
                rawStatus = updatedTask?.Status,
                transactionKey = updatedTask?.TransactionKey,
                cascadeMessage,
                message = "Status updated successfully."
            });
        }

        [HttpPost]
        [Route("{projectno}/status")]
        public async Task<IHttpActionResult> UpdateProjectBoardStatus(string projectno, [FromBody] ProjectBoardStatusUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            request.ProjectNo = string.IsNullOrWhiteSpace(request.ProjectNo) ? projectno : request.ProjectNo;
            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggedUser = User.Identity.GetClaim("employeeid");
            if (!await IsProjectOwnerMemberAsync(loggedUser, request.ProjectNo))
            {
                return ForbiddenProjectAccess("Only owner members can update project status from the board.");
            }

            var project = await _projectService.GetProjectByIdAsync(request.ProjectNo);
            if (project == null)
            {
                return NotFound();
            }

            var comment = (request.Comment ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(comment))
            {
                return BadRequest("Comment is required when moving a project to another lane.");
            }

            var currentLaneStatus = NormalizeBoardStatus(project.Status);
            var nextLaneStatus = NormalizeBoardStatus(request.NewStatus);
            if (string.Equals(currentLaneStatus, nextLaneStatus, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new
                {
                    message = "Project status is already in the selected lane.",
                    status = currentLaneStatus,
                    projectNo = request.ProjectNo,
                    transactionKey = project.TransactionKey
                });
            }

            var reason = $"Change status: {currentLaneStatus} to {nextLaneStatus} | {comment}";

            project.ModifiedBy = loggedUser;
            project.TransactionKey = string.IsNullOrWhiteSpace(request.TransactionKey) ? project.TransactionKey : request.TransactionKey;

            if (string.Equals(nextLaneStatus, "NOT STARTED", StringComparison.OrdinalIgnoreCase))
            {
                await _projectService.InitializeAsync(project, reason, true, false, true);
            }
            else if (string.Equals(nextLaneStatus, "ONGOING", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(currentLaneStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
                {
                    await _projectService.UnholdAsync(project, reason, true, false, true);
                }
                else
                {
                    project.ActualStartDate = DateTime.UtcNow;
                    await _projectService.StartAsync(project, reason, true, false, true);
                }
            }
            else if (string.Equals(nextLaneStatus, "HOLD", StringComparison.OrdinalIgnoreCase))
            {
                await _projectService.HoldAsync(project, reason, true, false, true);
            }
            else
            {
                await _projectService.CompleteAsync(project, reason, true, false, true);
            }

            await _projectCommentService.AddAsync(new ProjectComment
            {
                ProjectNo = request.ProjectNo,
                EntityType = "PROJECT",
                EntitySysId = request.ProjectNo,
                Comments = reason,
                CommentsRichText = null,
                CreatedBy = loggedUser,
                CreatedDate = DateTime.Now
            });

            var updatedProject = await _projectService.GetProjectByIdAsync(request.ProjectNo);

            return Content(HttpStatusCode.Accepted, new
            {
                projectNo = request.ProjectNo,
                status = NormalizeBoardStatus(updatedProject?.Status),
                rawStatus = updatedProject?.Status,
                transactionKey = updatedProject?.TransactionKey,
                message = "Project status updated successfully."
            });
        }



        [HttpGet]
        [Route("{projectno}/nodes")]
        public async Task<IHttpActionResult> GetProjectNodes(string projectno)
        {
            try
            {
                if (!await CanViewProjectDetailsAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("You are not authorized to view this project.");
                }

                var data = await _projectService.GetProjectNodesAsync(projectno);

                return Ok(data);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

        }

        [HttpGet]
        [Route("{projectno}/roadmap-structure/preview")]
        public async Task<IHttpActionResult> GetRoadmapStructurePreview(string projectno)
        {
            try
            {
                if (!await CanManageProjectAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("Only project members can access roadmap refresh.");
                }

                var preview = await _projectService.PreviewRoadmapRefreshAsync(projectno);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{projectno}/roadmap-structure/apply")]
        public async Task<IHttpActionResult> ApplyRoadmapStructurePreview(string projectno, [FromBody] ProjectRoadmapRefreshSelection selection)
        {
            if (selection == null)
            {
                return BadRequest("Request body is empty.");
            }

            selection.ProjectNo = string.IsNullOrWhiteSpace(selection.ProjectNo) ? projectno : selection.ProjectNo;
            if (!string.Equals(projectno, selection.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match the request body.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                if (!await CanManageProjectAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("Only project members can apply roadmap refresh.");
                }

                var result = await _projectService.ApplyRoadmapRefreshAsync(selection, User.Identity.GetClaim("employeeid"));
                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Roadmap structure merged successfully.",
                    result.AddedMilestones,
                    result.UpdatedMilestones,
                    result.AddedActivities,
                    result.UpdatedActivities,
                    result.AddedDependencyLinks,
                    result.AutoIncludedParents
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{projectno}/node")]
        public async Task<IHttpActionResult> GetProjectsNode(string projectno, [FromBody] ProjectNodeRequest request)
        {
            try
            {
                if (!await CanViewProjectDetailsAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("You are not authorized to view this project.");
                }

                var data = await _projectService.GetProjectNodeItemAsync(projectno, request.NodeType, request.NodeId);



                return Ok(data);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

        }

        [HttpPost]
        [Route("children")]
        [Route("{projectno}/{nodetype}/{nodeid}/children")]
        public async Task<IHttpActionResult> GetProjectsNodeChildren([FromBody] ProjectNodeRequest request)
        {
            try
            {
                if (!await CanViewProjectDetailsAsync(User.Identity.GetClaim("employeeid"), request.ProjectNo))
                {
                    return ForbiddenProjectAccess("You are not authorized to view this project.");
                }

                var data = await _projectService.GetProjectNodeChildrenAsync(request.ProjectNo, request.NodeType, request.NodeId);

                var response = new DataTablesResponse<ProjectExtend>
                {
                    data = (data).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }



        [HttpGet]
        [Route("{projectno}/formsubmissions")]
        [Route("formsubmissions")]
        public async Task<IHttpActionResult> GetProjectsFormSubmissions(string projectno)
        {
            if (!await CanViewProjectDetailsAsync(User.Identity.GetClaim("employeeid"), projectno))
            {
                return ForbiddenProjectAccess("You are not authorized to view this project.");
            }

            var pagedResult = await _projectService.GetSubmittedForms(projectno);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoProjectFormSubmission>
            {
                data = pagedResult.Select(Mapper.Map<dtoProjectFormSubmission>).ToList()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("{projectno}/data/{nodetype}/{nodesysid}")]
        public async Task<IHttpActionResult> GetProjectsFormSubmissions(string projectno, string nodetype, string nodesysid)
        {
            if (!await CanViewProjectDetailsAsync(User.Identity.GetClaim("employeeid"), projectno))
            {
                return ForbiddenProjectAccess("You are not authorized to view this project.");
            }

            var obj = await _projectService.GetSubmittedForms(projectno, nodetype, nodesysid);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoProjectFormSubmission>
            {
                data = obj.Select(Mapper.Map<dtoProjectFormSubmission>).ToList()
            };

            return Ok(response);
        }

        #region Members
        [Route("{projectno}/members")]
        public async Task<IHttpActionResult> GetMembers(string projectno)
        {
            if (!await CanViewProjectDetailsAsync(User.Identity.GetClaim("employeeid"), projectno))
            {
                return ForbiddenProjectAccess("You are not authorized to view this project.");
            }

            var obj = await _projectmemberService.GetAllProjectMembersAsync(projectno);

            // Prepare DataTables response
            var response = new DataTablesResponse<ProjectMember>
            {
                data = obj.Select(Mapper.Map<ProjectMember>).ToList()
            };

            return Ok(response);
        }
        [HttpPost]
        [Route("{projectno}/addmember")]
        public async Task<IHttpActionResult> AddMember(string projectno, [FromBody] ProjectMember member)
        {

            if (member == null)
                return BadRequest("Request body is empty.");

            if (string.IsNullOrWhiteSpace(member.UserId))
                return BadRequest("Member Id is required.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only project members can update project members.");
                }

                member.CreatedBy = loggeduser;
                member.IsOwner = 0;
                member.User.CreatedBy = loggeduser;


                await _projectmemberService.EnrollMemberAsync(member);

                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Member successfully registered."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("{projectno}/removemember")]
        public async Task<IHttpActionResult> RemoveMember(string projectno, [FromBody] ProjectMember member)
        {

            if (member == null)
                return BadRequest("Request body is empty.");

            if (string.IsNullOrWhiteSpace(member.UserId))
                return BadRequest("Member Id is required.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only project members can update project members.");
                }

                member.ModifiedBy = loggeduser;
                await _projectmemberService.RemoveMemberAsync(member.ProjectMemberSysId, loggeduser);

                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Member successfully removed from list."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{projectno}/members/{projectmembersysid}/owner")]
        public async Task<IHttpActionResult> UpdateMemberOwner(string projectno, string projectmembersysid, [FromBody] ProjectMember member)
        {
            if (string.IsNullOrWhiteSpace(projectmembersysid))
            {
                return BadRequest("Project member id is required.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
            {
                return ForbiddenProjectAccess("Only owner members can change owner access.");
            }

            var existing = await _projectmemberService.GetProjectMemberByIdAsync(projectmembersysid);
            if (existing == null || !string.Equals(existing.ProjectNo, projectno, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            existing.IsOwner = member != null && member.IsOwner == 1 ? 1 : 0;
            existing.ModifiedBy = loggeduser;

            await _projectmemberService.UpdateMemberAsync(existing);

            return Content(HttpStatusCode.Accepted, new
            {
                message = "Member owner access updated.",
                data = existing
            });
        }

        [HttpPost]
        [Route("{projectno}/takeover")]
        public async Task<IHttpActionResult> TakeOverProject(string projectno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
            {
                return ForbiddenProjectAccess("Only owner members can take over the project.");
            }

            var project = await _projectService.GetProjectByIdAsync(projectno);
            if (project == null)
            {
                return NotFound();
            }

            project.ProjectOwnerId = loggeduser;
            project.ModifiedBy = loggeduser;
            project.ModifiedDate = DateTime.UtcNow;
            project.UserModified = new User
            {
                UserId = loggeduser,
                FirstName = User.Identity.GetClaim("firstname"),
                LastName = User.Identity.GetClaim("lastname"),
                Email = (User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.Email)?.Value,
            };

            await _projectService.UpdateProjectAsync(project);

            return Content(HttpStatusCode.Accepted, new
            {
                message = "Project ownership updated.",
                projectOwnerId = project.ProjectOwnerId
            });
        }


        [HttpPost]
        [Route("{projectno}/member-tasks/update")]
        public async Task<IHttpActionResult> UpdateMemberTasks(string projectno, [FromBody] UpdateMemberTasksRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (string.IsNullOrWhiteSpace(request.MemberId))
            {
                return BadRequest("MemberId is required.");
            }

            try
            {
                if (!await CanManageProjectAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("Only project members can update task ownership.");
                }

                List<ProjectOwner> NewlySelected = new List<ProjectOwner>();
                List<ProjectOwner> NewlyUnSelected = new List<ProjectOwner>();
                foreach (var id in request.NewlySelectedTaskIds)
                {
                    NewlySelected.Add(new ProjectOwner
                    {
                        ProjectNo = request.ProjectNo,
                        UserId = request.MemberId,
                        ParentType = "TASK",
                        ParentSysId = id
                    });
                }

                foreach (var id in request.NewlyUnselectedTaskIds)
                {
                    NewlyUnSelected.Add(new ProjectOwner
                    {
                        ProjectNo = request.ProjectNo,
                        UserId = request.MemberId,
                        ParentType = "TASK",
                        ParentSysId = id
                    });
                }

                var (addcount, deletedcount) = await _projectownerService.UpdateNodeOwnerAsync(NewlySelected, NewlyUnSelected);

                return Ok(new
                {
                    success = true,
                    addCount = addcount,
                    deletedCount = deletedcount
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{projectno}/custom-milestones/templates")]
        public async Task<IHttpActionResult> GetCustomMilestoneTemplates(string projectno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can insert milestones.");
                }

                var templates = await _projectService.GetMilestoneTemplateCatalogAsync(projectno);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{projectno}/custom-milestones/arrangement")]
        public async Task<IHttpActionResult> GetMilestoneArrangement(string projectno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can manage milestone arrangement.");
                }

                var catalog = await _projectService.GetMilestoneArrangementCatalogAsync(projectno);
                return Ok(catalog);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{projectno}/custom-milestones/arrangement")]
        public async Task<IHttpActionResult> SaveMilestoneArrangement(string projectno, [FromBody] ProjectMilestoneArrangementSaveRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            request.ProjectNo = string.IsNullOrWhiteSpace(request.ProjectNo) ? projectno : request.ProjectNo;
            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can manage milestone arrangement.");
                }

                await _projectService.SaveMilestoneArrangementAsync(request, loggeduser);

                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Milestone arrangement saved successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{projectno}/custom-milestones/additional")]
        public async Task<IHttpActionResult> GetAdditionalCustomMilestones(string projectno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can manage additional milestones.");
                }

                var catalog = await _projectService.GetAdditionalMilestonesAsync(projectno);
                return Ok(catalog);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{projectno}/custom-milestones")]
        public async Task<IHttpActionResult> CreateCustomMilestone(string projectno, [FromBody] ProjectCustomMilestoneRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            request.ProjectNo = string.IsNullOrWhiteSpace(request.ProjectNo) ? projectno : request.ProjectNo;
            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (string.IsNullOrWhiteSpace(request.AnchorNodeId))
            {
                return BadRequest("Reference milestone is required.");
            }

            var sourceMode = NormalizeCustomMilestoneSourceMode(request.SourceMode);
            if (sourceMode == "MANUAL" && string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Milestone title is required.");
            }

            var normalizedTitle = sourceMode == "MANUAL" ? request.Title.Trim() : null;
            if (sourceMode == "MANUAL" && normalizedTitle.Length > 40)
            {
                return BadRequest("Milestone title must be 40 characters or fewer.");
            }

            var normalizedDescription = sourceMode == "MANUAL" && !string.IsNullOrWhiteSpace(request.Description) ? request.Description.Trim() : null;
            if (sourceMode == "MANUAL" && !string.IsNullOrWhiteSpace(normalizedDescription) && normalizedDescription.Length > 400)
            {
                return BadRequest("Milestone description must be 400 characters or fewer.");
            }

            var selectedTemplateMilestoneIds = sourceMode == "TEMPLATE"
                ? NormalizeTemplateMilestoneIds(request.TemplateMilestoneSysIds, request.TemplateMilestoneSysId)
                : new List<string>();

            if (sourceMode == "TEMPLATE"
                && (string.IsNullOrWhiteSpace(request.TemplateRoadmapSysId) || !selectedTemplateMilestoneIds.Any()))
            {
                return BadRequest("Select at least one roadmap template milestone to insert.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can insert milestones.");
                }

                var roadmapMilestoneSysIds = sourceMode == "TEMPLATE"
                    ? await _projectService.InsertProjectMilestoneTemplateAsync(
                        projectno,
                        request.AnchorNodeId,
                        request.InsertPosition,
                        request.TemplateRoadmapSysId,
                        selectedTemplateMilestoneIds,
                        loggeduser)
                    : new List<string>
                    {
                        await _projectService.InsertProjectMilestoneAsync(
                            projectno,
                            request.AnchorNodeId,
                            request.InsertPosition,
                            normalizedTitle,
                            normalizedDescription,
                            request.IsRequired,
                            loggeduser)
                    };

                return Content(HttpStatusCode.Created, new
                {
                    message = sourceMode == "TEMPLATE"
                        ? $"{roadmapMilestoneSysIds.Count} roadmap milestone template{(roadmapMilestoneSysIds.Count == 1 ? string.Empty : "s")} inserted successfully."
                        : "Milestone inserted successfully.",
                    roadmapMilestoneSysId = roadmapMilestoneSysIds.FirstOrDefault(),
                    roadmapMilestoneSysIds
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{projectno}/custom-milestones/{roadmapmilestonesysid}/reorder")]
        public async Task<IHttpActionResult> ReorderAdditionalCustomMilestone(string projectno, string roadmapmilestonesysid, [FromBody] ProjectCustomMilestoneReorderRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            request.ProjectNo = string.IsNullOrWhiteSpace(request.ProjectNo) ? projectno : request.ProjectNo;
            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (string.IsNullOrWhiteSpace(request.Direction))
            {
                return BadRequest("Move direction is required.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can manage additional milestones.");
                }

                await _projectService.ReorderAdditionalMilestoneAsync(projectno, roadmapmilestonesysid, request.Direction, loggeduser);

                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Milestone sequence updated successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("{projectno}/custom-milestones/{roadmapmilestonesysid}")]
        public async Task<IHttpActionResult> DeleteAdditionalCustomMilestone(string projectno, string roadmapmilestonesysid)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await IsProjectOwnerMemberAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only owner members can manage additional milestones.");
                }

                await _projectService.DeleteAdditionalMilestoneAsync(projectno, roadmapmilestonesysid, loggeduser);

                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Additional milestone removed successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{projectno}/custom-tasks")]
        public async Task<IHttpActionResult> CreateCustomTask(string projectno, [FromBody] ProjectCustomTaskRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (string.IsNullOrWhiteSpace(request.ParentNodeId))
            {
                return BadRequest("Milestone is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Task title is required.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only project members can add tasks.");
                }

                var project = await _projectRepository.GetAsync(projectno);
                if (project == null)
                {
                    return NotFound();
                }

                var members = await _projectMemberRepository.GetListAsync(projectno);
                var memberIds = new HashSet<string>(
                    members.Where(member => member != null && !string.IsNullOrWhiteSpace(member.UserId))
                        .Select(member => member.UserId),
                    StringComparer.OrdinalIgnoreCase);

                var ownerIds = (request.OwnerIds ?? Enumerable.Empty<string>())
                    .Where(ownerId => !string.IsNullOrWhiteSpace(ownerId))
                    .Select(ownerId => ownerId.Trim())
                    .Where(memberIds.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!ownerIds.Any())
                {
                    ownerIds.Add(loggeduser);
                }

                var normalizedParentNodeId = request.ParentNodeId.Trim();
                var isRootActivityParent = string.Equals(normalizedParentNodeId, "__ROOTACTIVITY__", StringComparison.OrdinalIgnoreCase);
                var parentType = isRootActivityParent ? "ROADMAP" : "MILESTONE";
                var parentSysId = isRootActivityParent ? project.RoadmapSysId : normalizedParentNodeId;

                var siblingTasks = await _projectTaskRepository.GetListAsync(projectno, parentType, parentSysId);
                var orderIndex = siblingTasks.Any() ? siblingTasks.Max(tsk => tsk.OrderIndex) + 1 : 1;

                var task = new ProjectTask
                {
                    ProjectNo = projectno,
                    RoadmapActivitySysId = null,
                    ParentType = parentType,
                    ParentSysId = parentSysId,
                    PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                    RoadmapSysId = project.RoadmapSysId,
                    AltTaskName = request.Title.Trim(),
                    AltTaskDescription = NormalizeText(request.Description),
                    EstimatedMandays = Math.Max(0, request.EstimatedMandays ?? 0),
                    Status = "NOT STARTED",
                    IsRequired = request.IsRequired ? 1 : 0,
                    OrderIndex = orderIndex,
                    IsActive = 1,
                    CreatedBy = loggeduser
                };

                task.ProjectTaskSysId = await _projectTaskRepository.AddAsync(task);

                foreach (var ownerId in ownerIds)
                {
                    await _projectOwnerRepository.AddAsync(new ProjectOwner
                    {
                        ProjectNo = projectno,
                        UserId = ownerId,
                        ParentType = "TASK",
                        ParentSysId = task.ProjectTaskSysId
                    });
                }

                return Content(HttpStatusCode.Created, new
                {
                    message = "Task created successfully.",
                    projectTaskSysId = task.ProjectTaskSysId
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{projectno}/custom-tasks/{projecttasksysid}")]
        public async Task<IHttpActionResult> UpdateCustomTask(string projectno, string projecttasksysid, [FromBody] ProjectCustomTaskRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is empty.");
            }

            if (!string.Equals(projectno, request.ProjectNo, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Route project number does not match body.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Task title is required.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only project members can update tasks.");
                }

                var task = await GetCustomProjectTaskAsync(projectno, projecttasksysid);
                if (task == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(task.RoadmapActivitySysId))
                {
                    return BadRequest("Only custom tasks can be edited here.");
                }

                task.AltTaskName = request.Title.Trim();
                task.AltTaskDescription = NormalizeText(request.Description);
                task.EstimatedMandays = Math.Max(0, request.EstimatedMandays ?? 0);
                task.IsRequired = request.IsRequired ? 1 : 0;
                task.TransactionKey = string.IsNullOrWhiteSpace(request.TransactionKey) ? task.TransactionKey : request.TransactionKey.Trim();
                task.ModifiedBy = loggeduser;

                var rows = await _projectTaskRepository.UpdateAsync(task);
                if (rows <= 0)
                {
                    return Content(HttpStatusCode.Conflict, new { message = "Task was updated by another user. Refresh and try again." });
                }

                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Task updated successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("{projectno}/custom-tasks/{projecttasksysid}")]
        public async Task<IHttpActionResult> DeleteCustomTask(string projectno, string projecttasksysid)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, projectno))
                {
                    return ForbiddenProjectAccess("Only project members can delete tasks.");
                }

                var task = await GetCustomProjectTaskAsync(projectno, projecttasksysid);
                if (task == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(task.RoadmapActivitySysId))
                {
                    return BadRequest("Only custom tasks can be deleted here.");
                }

                var members = await _projectMemberRepository.GetListAsync(projectno);
                foreach (var member in members.Where(member => member != null && !string.IsNullOrWhiteSpace(member.UserId)))
                {
                    var owner = await _projectOwnerRepository.GetAsync(projectno, member.UserId, "TASK", projecttasksysid);
                    if (owner != null)
                    {
                        await _projectOwnerRepository.DeleteAsync(owner.ProjectOwnerSysId);
                    }
                }

                await _projectTaskRepository.DeleteAsync(projecttasksysid);

                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Task deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("{projectno}")]
        public async Task<IHttpActionResult> DeleteProject(string projectno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!await IsProjectOwnerMemberAsync(User.Identity.GetClaim("employeeid"), projectno))
            {
                return ForbiddenProjectAccess("Only owner members can delete this project.");
            }

            await _projectService.DeleteProjectAsync(projectno, User.Identity.GetClaim("employeeid"), string.Empty);
            return Ok(new { success = true, projectNo = projectno });
        }
        #endregion



        #region Product
        [HttpPost]
        [Authorize]
        [Route("{projectno}/products/link")]
        public async Task<IHttpActionResult> LinkProduct(string projectno, [FromBody] ProjectProduct product)
        {

            if (product == null)
                return BadRequest("Request body is empty.");

            if (string.IsNullOrWhiteSpace(product.ProductCode))
                return BadRequest("Product Code is required.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                if (!await CanManageProjectAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("Only project members can update project products.");
                }



                await _projectService.LinkProduct(product);

                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Product successfully linked."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }
        public class ProjectCustomTaskRequest
        {
            public string ProjectNo { get; set; }
            public string ParentNodeId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int? EstimatedMandays { get; set; }
            public bool IsRequired { get; set; }
            public string TransactionKey { get; set; }
            public IEnumerable<string> OwnerIds { get; set; }
        }

        public class ProjectBoardStatusUpdateRequest
        {
            public string EntityType { get; set; }
            public string EntitySysId { get; set; }
            public string NodeId { get; set; }
            public string ProjectNo { get; set; }
            public string NewStatus { get; set; }
            public string Comment { get; set; }
            public string TransactionKey { get; set; }
        }

        private sealed class OwnerBoardUserData
        {
            [JsonProperty("userid")]
            public string UserId { get; set; }

            [JsonProperty("userId")]
            public string AlternateUserId { set { if (string.IsNullOrWhiteSpace(UserId)) { UserId = value; } } }

            [JsonProperty("username")]
            public string UserName { get; set; }
        }

        private sealed class OwnerBoardPrerequisitePayload
        {
            [JsonProperty("prerequisites")]
            public List<OwnerBoardPrerequisiteItem> Prerequisites { get; set; }
        }

        private sealed class OwnerBoardPrerequisiteItem
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }
        }

        public class ProjectCustomMilestoneRequest
        {
            public string ProjectNo { get; set; }
            public string AnchorNodeId { get; set; }
            public string InsertPosition { get; set; }
            public string SourceMode { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool IsRequired { get; set; }
            public string TemplateRoadmapSysId { get; set; }
            public string TemplateMilestoneSysId { get; set; }
            public IEnumerable<string> TemplateMilestoneSysIds { get; set; }
        }

        public class ProjectCustomMilestoneReorderRequest
        {
            public string ProjectNo { get; set; }
            public string Direction { get; set; }
        }
        

        [HttpDelete]
        [Authorize]
        [Route("{projectno}/products/unlink")]
        public async Task<IHttpActionResult> UnlinkProduct(string projectno, [FromBody] ProjectProduct product)
        {

            if (product == null)
                return BadRequest("Request body is empty.");

            if (string.IsNullOrWhiteSpace(product.ProductCode))
                return BadRequest("Product Code is required.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                if (!await CanManageProjectAsync(User.Identity.GetClaim("employeeid"), projectno))
                {
                    return ForbiddenProjectAccess("Only project members can update project products.");
                }

                await _projectService.UnlinkProduct(product);

                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Product successfully removed from list."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }
        #endregion
    }
}