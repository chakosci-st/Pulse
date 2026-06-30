using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pulse.Api.Models;
using Pulse.Core.Entities;
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
using System.Web;
using System.Web.Http;

namespace Pulse.Api.Controllers
{

    public class ProjectAttachmentsController : ApiController
    {
        private readonly IProjectAttachmentService _projectattachmentService;
        private readonly IProjectMemberService _projectMemberService;

        public ProjectAttachmentsController(IProjectAttachmentService projectattachmentService, IProjectMemberService projectMemberService)
        {
            _projectattachmentService = projectattachmentService;
            _projectMemberService = projectMemberService;
        }



        [Route("api/project/{projectno}/attachments")]
        public async Task<IHttpActionResult> GetPerProject(string projectno)
        {

            var data = await _projectattachmentService.GetByProjectAsync(projectno);

            await ApplyAttachmentPermissionsAsync(projectno, data);

            return Ok(new { data = data });
        }

        [Route("api/project/{projectno}/attachments/{entitytype}/{entitysysid}")]
        public async Task<IHttpActionResult> GetPerEntity(string projectno, string entitytype, string entitysysid)
        {

            var data = await _projectattachmentService.GetByEntityAsync(projectno, entitytype, entitysysid);

            await ApplyAttachmentPermissionsAsync(projectno, data);

            return Ok(new { data = data }); 
        }

        private async Task ApplyAttachmentPermissionsAsync(string projectno, IEnumerable<ProjectAttachment> attachments)
        {
            if (attachments == null)
            {
                return;
            }

            var currentUserId = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        attachment.CanManageAttachment = false;
                    }
                }

                return;
            }

            var members = await _projectMemberService.GetAllProjectMembersAsync(projectno);
            var isProjectMember = (members ?? Enumerable.Empty<ProjectMember>())
                .Any(member => member != null && string.Equals(member.UserId, currentUserId, StringComparison.OrdinalIgnoreCase));

            foreach (var attachment in attachments)
            {
                if (attachment == null)
                {
                    continue;
                }

                attachment.CanManageAttachment = isProjectMember || string.Equals(attachment.CreatedBy, currentUserId, StringComparison.OrdinalIgnoreCase);
            }
        }



    }
}
