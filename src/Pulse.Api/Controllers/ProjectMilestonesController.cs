using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
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
    [RoutePrefix("api/milestones")]
    public class ProjectMilestonesController : ApiController
    {
        private readonly IProjectMilestoneService _projectmilestoneService;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;

        public ProjectMilestonesController(IProjectMilestoneService projectmilestoneService,
            IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository)
        {
            _projectmilestoneService = projectmilestoneService;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
        }

        private async Task<bool> CanManageProjectAsync(string loggedUserId, string projectNo)
        {
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

        [HttpPut]
        [Route("unlock")]
        public async Task<IHttpActionResult> Unlock([FromBody] ProjectMilestoneUnlockSubmit model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Reason))
            {
                return Ok(new
                {
                    Success = false,
                    Error = "Reason text is required."
                });
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var loggeduserFirstName = User.Identity.GetClaim("firstname");
            var loggeduserLastName = User.Identity.GetClaim("lastname");

            var node = await _projectmilestoneService.GetProjectMilestoneByIdAsync(model.Id);
            if (node == null)
            {
                return NotFound();
            }

            if (!await CanManageProjectAsync(loggeduser, node.ProjectNo))
            {
                return ForbiddenProjectAccess("Only project members can update project milestones.");
            }

            node.ActualStartDate = DateTime.UtcNow;
            node.ActualStartedBy = loggeduser;
            node.TransactionKey = model.TransactionKey;
            node.CreatedBy = loggeduser;
            node.ModifiedBy = loggeduser;

            await _projectmilestoneService.StartAsync(node, model.Reason, true, true, true);

            return Ok(new
            {
                success = true,
                data = node
            });
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
                    return ForbiddenProjectAccess("Only project members can update project milestones.");
                }

                await _projectmilestoneService.SetTargetAsync(new ProjectMilestone
                {

                    MilestoneSysId = modelTask.ProjectNodeSysId,
                    RoadmapMilestoneSysId = modelTask.NodeId,
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

    }
}
