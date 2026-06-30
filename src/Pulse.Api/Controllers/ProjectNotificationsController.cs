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
    [RoutePrefix("api/notifications")]
    public class ProjectNotificationsController : ApiController
    {
        private readonly IProjectNotificationService _projectnotificationService;

        public ProjectNotificationsController(IProjectNotificationService projectnotificationService)
        {
            _projectnotificationService = projectnotificationService;
        }

        [HttpPost]
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

            var loggeduser = User.Identity.GetClaim("employeeid");
            var loggeduserFirstName = User.Identity.GetClaim("firstname");
            var loggeduserLastName = User.Identity.GetClaim("lastname");

            var obj = new ProjectNotification
            {
                Title = model.Title,
                Message = model.Message,
                Recipients = model.Recipients,
                NotificationDate = model.NotificationDate,
                CreatedBy = loggeduser,
                ProjectNo = model.ProjectNo,
                EntitySysId = model.EntitySysId,
                EntityType = model.EntityType,
                CreatedDate = DateTime.Now,
                CreatedByMeta = new User
                {
                    FirstName = loggeduserFirstName,
                    LastName = loggeduserLastName
                }
            };

            var sysId = await _projectnotificationService.AddAsync(obj);
            obj.NotificationSysId = sysId;

            return Ok(new
            {
                success = true,
                data = obj
            });
        }

        [Route("{projectno}")]
        public async Task<IHttpActionResult> GetPerProject(string projectno)
        {

            var data = await _projectnotificationService.GetByProjectAsync(projectno);

            return Ok(new { data = data });
        }

        [Route("{projectno}/{entitytype}")]
        public async Task<IHttpActionResult> GetProjectAttachmentsPerEntity(string projectno, string entitytype, string entitysysid)
        {

            var data = await _projectnotificationService.GetByEntityAsync(projectno, entitytype, entitysysid);

            return Ok(new { data = data });
        }
    }
}
