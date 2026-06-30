using AutoMapper;
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
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/ProjectTasks")]
    public class ProjectTasksController : ApiController
    {
        private const string SuperUserModuleCode = "SUPERUSER";

        private readonly IProjectTaskService _projecttaskService;
        private readonly IProjectService _projectService;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;

        public ProjectTasksController(IProjectTaskService projecttaskService, IProjectService projectService,
            IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository)
        {
            _projecttaskService = projecttaskService;
            _projectService = projectService;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
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

            var project = await _projectRepository.GetAsync(projectNo);
            if (project == null)
            {
                return false;
            }

            if (string.Equals(project.ProjectOwnerId, loggedUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var members = await _projectMemberRepository.GetListAsync(projectNo);
            return members.Any(member => member != null && string.Equals(member.UserId, loggedUserId, StringComparison.OrdinalIgnoreCase));
        }

        private IHttpActionResult ForbiddenProjectAccess(string message)
        {
            return Content(HttpStatusCode.Forbidden, new { message });
        }

        private bool HasAdvancedStatusModule()
        {
            return HasModuleCode("ADVCHSTAT");
        }

        private bool HasSuperUserModule()
        {
            return HasModuleCode(SuperUserModuleCode, false);
        }

        private bool HasModuleCode(string moduleCode, bool includeSuperUser = true)
        {
            var rawModules = User.Identity.GetClaim("modulecodes") ?? string.Empty;
            var moduleCodes = rawModules
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(module => module.Trim())
                .ToList();

            return moduleCodes.Any(module => string.Equals(module, moduleCode, StringComparison.OrdinalIgnoreCase))
                || (includeSuperUser && moduleCodes.Any(module => string.Equals(module, SuperUserModuleCode, StringComparison.OrdinalIgnoreCase)));
        }

        private async Task<ProjectTaskUpdateViewModel> ReadTaskUpdateModelAsync()
        {
            if (Request.Content == null)
            {
                return null;
            }

            if (Request.Content.IsMimeMultipartContent())
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var taskContent = provider.Contents.FirstOrDefault(c =>
                    string.Equals(c.Headers.ContentDisposition?.Name?.Trim('"'), "projecttask", StringComparison.OrdinalIgnoreCase));

                if (taskContent == null)
                {
                    return null;
                }

                var modelJson = await taskContent.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectTaskUpdateViewModel>(modelJson);
            }

            var rawBody = await Request.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawBody))
            {
                return null;
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectTaskUpdateViewModel>(rawBody);
        }

        private static string ResolveTaskStatusReason(string remarks, string fallback)
        {
            return string.IsNullOrWhiteSpace(remarks) ? fallback : remarks.Trim();
        }

        private static void ApplyRouteCodeToMissingIdentifiers(ProjectTaskUpdateViewModel model, string code)
        {
            if (model == null || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                && string.IsNullOrWhiteSpace(model.RoadmapActivitySysId))
            {
                model.ProjectTaskSysId = code;
            }
        }


        [HttpPut]
        [Authorize]
        [Route("{code}/complete")]
        public async Task<IHttpActionResult> CompleteTask(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var model = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "projecttask");
            ProjectTaskUpdateViewModel modelTask = null;
            if (model != null)
            {
                var modelJson = await model.ReadAsStringAsync();
                modelTask = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectTaskUpdateViewModel>(modelJson);
            }

            if (modelTask == null)
                return BadRequest("Task data is missing.");

            ApplyRouteCodeToMissingIdentifiers(modelTask, code);

            if (!string.Equals(code, modelTask.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, modelTask.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);





            if (model == null)
                return BadRequest("Request body is empty.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, modelTask.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                var projecttask = !string.IsNullOrWhiteSpace(modelTask.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(modelTask.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                {
                    var project = await _projectService.GetProjectByIdAsync(modelTask.ProjectNo);

                    if (project == null)
                    {
                        return NotFound();
                    }


                    projecttask = new ProjectTask
                    {
                        ProjectNo = modelTask.ProjectNo,
                        RoadmapActivitySysId = modelTask.RoadmapActivitySysId,
                        RoadmapSysId = project.RoadmapSysId,
                        PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                        Status = "ONGOING",
                        CreatedBy = loggeduser
                    };


                    projecttask.ProjectTaskSysId = await _projecttaskService.AddTaskAsync(projecttask, loggeduser);

                    await _projecttaskService.InitializeAsync(projecttask, "", false);


                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(modelTask.TransactionKey))
                    {
                        projecttask.TransactionKey = modelTask.TransactionKey;
                    }
                }

                projecttask.ModifiedBy = loggeduser;
                projecttask.ActualCompletedBy = loggeduser;
                projecttask.ActualCompletionDate = DateTime.UtcNow;


                await _projecttaskService.CompleteAsync(projecttask, ResolveTaskStatusReason(modelTask.Remarks, "Task completed"), false);




                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully completed."
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
        [Authorize]
        [Route("{code}/reopen")]
        public async Task<IHttpActionResult> ReOpenTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            ApplyRouteCodeToMissingIdentifiers(model, code);

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, model.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                var projecttask = !string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                    return NotFound();

                if (!await CanManageProjectAsync(loggeduser, projecttask.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                if (!string.IsNullOrWhiteSpace(model.TransactionKey))
                {
                    projecttask.TransactionKey = model.TransactionKey;
                }
                projecttask.ModifiedBy = loggeduser;
                projecttask.ActualStartDate = null;
                await _projecttaskService.InitializeAsync(projecttask, ResolveTaskStatusReason(model.Remarks, "Task reopened"), false, false, true);


                // raise event that Task status was changed


                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully reopened."
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
        [Authorize]
        [Route("{code}/initialize")]
        public async Task<IHttpActionResult> InitializeTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            ApplyRouteCodeToMissingIdentifiers(model, code);

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, model.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, model.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                var projecttask = !string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                {
                    return Content(HttpStatusCode.Accepted, new
                    {
                        taskid = model.ProjectTaskSysId,
                        message = "Task is already in not started status."
                    });
                }

                if (!string.IsNullOrWhiteSpace(model.TransactionKey))
                {
                    projecttask.TransactionKey = model.TransactionKey;
                }
                projecttask.ModifiedBy = loggeduser;

                await _projecttaskService.InitializeAsync(projecttask, ResolveTaskStatusReason(model.Remarks, "Task reset to not started"), false, false, true);

                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully reset to not started."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPut]
        [Authorize]
        [Route("{code}/start")]
        public async Task<IHttpActionResult> StartTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            ApplyRouteCodeToMissingIdentifiers(model, code);

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, model.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, model.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                var projecttask = !string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                {
                    var project = await _projectService.GetProjectByIdAsync(model.ProjectNo);

                    if (project == null)
                    {
                        return NotFound();
                    }

                    projecttask = new ProjectTask
                    {
                        ProjectNo = model.ProjectNo,
                        RoadmapActivitySysId = model.RoadmapActivitySysId,
                        RoadmapSysId = project.RoadmapSysId,
                        PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                        Status = "NOT STARTED",
                        CreatedBy = loggeduser
                    };

                    projecttask.ProjectTaskSysId = await _projecttaskService.AddTaskAsync(projecttask, loggeduser);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(model.TransactionKey))
                    {
                        projecttask.TransactionKey = model.TransactionKey;
                    }
                }

                projecttask.ModifiedBy = loggeduser;
                projecttask.ActualStartDate = DateTime.UtcNow;

                await _projecttaskService.StartAsync(projecttask, ResolveTaskStatusReason(model.Remarks, "Task started"), false, false, true);

                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully started."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPut]
        [Authorize]
        [Route("{code}/hold")]
        public async Task<IHttpActionResult> HoldTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            ApplyRouteCodeToMissingIdentifiers(model, code);

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, model.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, model.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                var projecttask = !string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                {
                    var project = await _projectService.GetProjectByIdAsync(model.ProjectNo);

                    if (project == null)
                    {
                        return NotFound();
                    }

                    projecttask = new ProjectTask
                    {
                        ProjectNo = model.ProjectNo,
                        RoadmapActivitySysId = model.RoadmapActivitySysId,
                        RoadmapSysId = project.RoadmapSysId,
                        PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                        Status = "NOT STARTED",
                        CreatedBy = loggeduser
                    };

                    projecttask.ProjectTaskSysId = await _projecttaskService.AddTaskAsync(projecttask, loggeduser);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(model.TransactionKey))
                    {
                        projecttask.TransactionKey = model.TransactionKey;
                    }
                }

                projecttask.ModifiedBy = loggeduser;
                await _projecttaskService.HoldAsync(projecttask, ResolveTaskStatusReason(model.Remarks, "Task put on hold"), false, false, true);

                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully put on hold."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPut]
        [Authorize]
        [Route("{code}/cancel")]
        public async Task<IHttpActionResult> CancelTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            ApplyRouteCodeToMissingIdentifiers(model, code);

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, model.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!HasAdvancedStatusModule())
            {
                return ForbiddenProjectAccess("You are not allowed to move tasks to Cancelled lane.");
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, model.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                var projecttask = !string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                {
                    var project = await _projectService.GetProjectByIdAsync(model.ProjectNo);

                    if (project == null)
                    {
                        return NotFound();
                    }

                    projecttask = new ProjectTask
                    {
                        ProjectNo = model.ProjectNo,
                        RoadmapActivitySysId = model.RoadmapActivitySysId,
                        RoadmapSysId = project.RoadmapSysId,
                        PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                        Status = "NOT STARTED",
                        CreatedBy = loggeduser
                    };

                    projecttask.ProjectTaskSysId = await _projecttaskService.AddTaskAsync(projecttask, loggeduser);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(model.TransactionKey))
                    {
                        projecttask.TransactionKey = model.TransactionKey;
                    }
                }

                projecttask.ModifiedBy = loggeduser;
                projecttask.ActualCompletionDate = DateTime.UtcNow;
                await _projecttaskService.CancelAsync(projecttask, ResolveTaskStatusReason(model.Remarks, "Task cancelled"), false, false, true);

                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully cancelled."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPut]
        [Authorize]
        [Route("{code}/archive")]
        public async Task<IHttpActionResult> ArchiveTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            ApplyRouteCodeToMissingIdentifiers(model, code);

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, model.RoadmapActivitySysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!HasAdvancedStatusModule())
            {
                return ForbiddenProjectAccess("You are not allowed to move tasks to Archived lane.");
            }

            try
            {
                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, model.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                var projecttask = !string.IsNullOrWhiteSpace(model.ProjectTaskSysId)
                    ? await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId)
                    : null;
                if (projecttask == null)
                {
                    var project = await _projectService.GetProjectByIdAsync(model.ProjectNo);

                    if (project == null)
                    {
                        return NotFound();
                    }

                    projecttask = new ProjectTask
                    {
                        ProjectNo = model.ProjectNo,
                        RoadmapActivitySysId = model.RoadmapActivitySysId,
                        RoadmapSysId = project.RoadmapSysId,
                        PlantRoadmapLinkSysId = project.PlantRoadmapLinkSysId,
                        Status = "NOT STARTED",
                        CreatedBy = loggeduser
                    };

                    projecttask.ProjectTaskSysId = await _projecttaskService.AddTaskAsync(projecttask, loggeduser);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(model.TransactionKey))
                    {
                        projecttask.TransactionKey = model.TransactionKey;
                    }
                }

                projecttask.ModifiedBy = loggeduser;
                projecttask.ActualCompletionDate = DateTime.UtcNow;
                await _projecttaskService.ArchiveAsync(projecttask, ResolveTaskStatusReason(model.Remarks, "Task archived"), false, false, true);

                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully archived."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPut]
        [Authorize]
        [Route("{code}/unlock")]
        public async Task<IHttpActionResult> UnlockTask(string code)
        {
            var model = await ReadTaskUpdateModelAsync();
            if (model == null)
                return BadRequest("Request body is empty.");

            if (!string.Equals(code, model.ProjectTaskSysId, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid request.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                var projecttask = await _projecttaskService.GetTaskByIdAsync(model.ProjectTaskSysId);
                if (projecttask == null)
                    return NotFound();

                if (!await CanManageProjectAsync(loggeduser, projecttask.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                projecttask.TransactionKey = model.TransactionKey;
                projecttask.ModifiedBy = loggeduser;
                projecttask.ActualStartDate = projecttask.ActualStartDate ?? DateTime.UtcNow;
                await _projecttaskService.UnholdAsync(projecttask, "Task unlocked", false, false, true);


                // raise event that Task status was changed


                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    taskid = projecttask.ProjectTaskSysId,
                    message = "Task is successfully unlocked."
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
        [Authorize]
        [Route("{code}/targetchange")]
        public async Task<IHttpActionResult> TargetChange(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var model = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "targetdate");
            Models.TargetRevision modelTask = null;
            if (model != null)
            {
                var modelJson = await model.ReadAsStringAsync();
                modelTask = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.TargetRevision>(modelJson);
            }

            if (model == null)
                return BadRequest("Request body is empty.");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                if (!await CanManageProjectAsync(loggeduser, modelTask.ProjectNo))
                {
                    return ForbiddenProjectAccess("Only project members can update project tasks.");
                }

                await _projecttaskService.SetTargetAsync(new ProjectTask
                {
                    ProjectTaskSysId = modelTask.ProjectNodeSysId,
                    RoadmapActivitySysId = modelTask.NodeId,
                    ProjectNo = modelTask.ProjectNo,
                    TargetStartDate = modelTask.TargetStartDate,
                    TargetCompletionDate = modelTask.TargetCompletionDate
                }, modelTask.Remarks, loggeduser);

                // Return 201 Created + some result
                return Content(HttpStatusCode.Accepted, new
                {
                    message = "Task is successfully updated."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Assigned")]
        public async Task<IHttpActionResult> Assigned()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var obj = await _projecttaskService.GetItemListAsync(HasSuperUserModule() ? null : loggeduser);


            return Ok(obj);
        }

        [HttpGet]
        [Route("Details/{id}")]
        [Route("Details")]
        public async Task<IHttpActionResult> GetDetails(string id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var obj = (await _projecttaskService.GetItemDetailsAsync(id, HasSuperUserModule() ? null : loggeduser));


            return Ok(obj);
        }

        [HttpGet]
        [Route("DetailsReadonly/{id}")]
        public async Task<IHttpActionResult> GetDetailsReadonly(string id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var obj = await _projecttaskService.GetItemDetailsReadOnlyAsync(id, HasSuperUserModule() ? null : loggeduser);

            return Ok(obj);
        }
    }
}
