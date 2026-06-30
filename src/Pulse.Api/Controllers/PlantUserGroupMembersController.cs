using Newtonsoft.Json;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.SharedUtilities.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/plantusergroupmembers")]
    public class PlantUserGroupMembersController : ApiController
    {
        private readonly IPlantUserGroupMemberService _plantUserGroupMemberService;
        private readonly IUserGroupService _userGroupService;

        public PlantUserGroupMembersController(IPlantUserGroupMemberService plantUserGroupMemberService, IUserGroupService userGroupService)
        {
            _plantUserGroupMemberService = plantUserGroupMemberService;
            _userGroupService = userGroupService;
        }

        [HttpGet]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        [Route("{plantCode}/user/{userId}")]
        public async Task<IHttpActionResult> GetAssignments(string plantCode, string userId)
        {
            if (string.IsNullOrWhiteSpace(plantCode) || string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("Plant code and user id are required.");
            }

            var userGroups = (await _userGroupService.GetAllUserGroupsAsync()).ToList();
            var assignments = await Task.WhenAll(userGroups.Select(async userGroup =>
            {
                var assignment = (await _plantUserGroupMemberService.GetAlMembersByPlantUserGroupAsync(plantCode, userGroup.UserGroupId))
                    .FirstOrDefault(member => string.Equals(member.UserId, userId, StringComparison.OrdinalIgnoreCase));

                return new
                {
                    id = assignment?.PlantUserGroupMemberSysId,
                    plantCode = plantCode,
                    userId = userId,
                    userGroupId = userGroup.UserGroupId,
                    userGroupName = userGroup.UserGroupName,
                    userGroupDescription = userGroup.UserGroupDescription,
                    isActive = userGroup.IsActive,
                    isSelected = assignment != null ? 1 : 0
                };
            }));

            return Ok(new { data = assignments.ToList() });
        }

        [HttpPost]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        [Route("link")]
        public async Task<IHttpActionResult> Link()
        {
            var obj = await ReadPlantUserGroupMemberAsync();
            if (obj == null)
            {
                return BadRequest("Plant user group member data is missing.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = (await _plantUserGroupMemberService.GetAlMembersByPlantUserGroupAsync(obj.PlantCode, obj.UserGroupId))
                .FirstOrDefault(member => string.Equals(member.UserId, obj.UserId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return Content(HttpStatusCode.Conflict, "Plant user group member already exists.");
            }

            obj.CreatedBy = User.Identity.GetClaim("employeeid");
            obj.CreatedDate = DateTime.UtcNow;

            var id = await _plantUserGroupMemberService.AddMemberAsync(obj);
            return Ok(new { id = id });
        }

        [HttpDelete]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        [Route("unlink")]
        public async Task<IHttpActionResult> Unlink()
        {
            var obj = await ReadPlantUserGroupMemberAsync();
            if (obj == null)
            {
                return BadRequest("Plant user group member data is missing.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignmentId = obj.PlantUserGroupMemberSysId;
            if (string.IsNullOrWhiteSpace(assignmentId))
            {
                assignmentId = (await _plantUserGroupMemberService.GetAlMembersByPlantUserGroupAsync(obj.PlantCode, obj.UserGroupId))
                    .Where(member => string.Equals(member.UserId, obj.UserId, StringComparison.OrdinalIgnoreCase))
                    .Select(member => member.PlantUserGroupMemberSysId)
                    .FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(assignmentId))
            {
                return NotFound();
            }

            await _plantUserGroupMemberService.DeleteMemberAsync(assignmentId, User.Identity.GetClaim("employeeid"));
            return Ok();
        }

        private async Task<PlantUserGroupMember> ReadPlantUserGroupMemberAsync()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return null;

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "plantusergroupmember");
            if (objContent == null)
            {
                return null;
            }

            var objJson = await objContent.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PlantUserGroupMember>(objJson);
        }
    }
}