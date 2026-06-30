using AutoMapper;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using Pulse.SharedUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IUserGroupService _userGroupService;
        private readonly IUserGroupMemberService _userGroupMemberService;
        private readonly IProjectService _projectService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IPlantService _plantService;
        private readonly IPlantMemberRepository _plantMemberRepository;

        public UsersController(
            IUserService userService,
            IUserGroupService userGroupService,
            IUserGroupMemberService userGroupMemberService,
            IProjectService projectService,
            IProjectMemberRepository projectMemberRepository,
            IProjectTaskService projectTaskService,
            IPlantService plantService,
            IPlantMemberRepository plantMemberRepository)
        {
            _userService = userService;
            _userGroupService = userGroupService;
            _userGroupMemberService = userGroupMemberService;
            _projectService = projectService;
            _projectMemberRepository = projectMemberRepository;
            _projectTaskService = projectTaskService;
            _plantService = plantService;
            _plantMemberRepository = plantMemberRepository;
        }

        [HttpGet]
        [Authorize]
        [Route("reports/cards")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USERVIEW")]
        public async Task<IHttpActionResult> GetReportCards()
        {
            var users = (await _userService.GetAllUsersAsync() ?? Enumerable.Empty<User>())
                .Where(user => user != null)
                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .ThenBy(user => user.UserName)
                .ToList();

            var projects = (await _projectService.GetAllProjectsAsync() ?? Enumerable.Empty<Project>())
                .Where(project => project != null)
                .ToList();

            var projectMembers = (await _projectMemberRepository.GetListAsync() ?? Enumerable.Empty<ProjectMember>())
                .Where(member => member != null && !string.IsNullOrWhiteSpace(member.UserId))
                .ToList();

            var closedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "COMPLETED",
                "FAILED",
                "CANCEL",
                "CANCELED",
                "CANCELLED"
            };

            var rows = new List<object>(users.Count);
            foreach (var user in users)
            {
                var userId = user.UserId ?? string.Empty;

                var ownedProjects = projects.Count(project =>
                    string.Equals(project.ProjectOwnerId, userId, StringComparison.OrdinalIgnoreCase));

                var memberProjects = projectMembers
                    .Where(member => string.Equals(member.UserId, userId, StringComparison.OrdinalIgnoreCase))
                    .Select(member => member.ProjectNo)
                    .Where(projectNo => !string.IsNullOrWhiteSpace(projectNo))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                var assignedTasks = (await _projectTaskService.GetItemListAsync(userId) ?? Enumerable.Empty<ProjectTaskItem>())
                    .Where(task => task != null)
                    .ToList();

                var pendingTasks = assignedTasks.Count(task =>
                    !closedStatuses.Contains((task.Status ?? string.Empty).Trim()));

                var initials = BuildInitials(user.FirstName, user.LastName, user.UserName, user.UserId);
                var photoUrl = BuildPhotoUrl(userId);

                rows.Add(new
                {
                    userId = user.UserId,
                    userName = user.UserName,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    isActive = user.IsActive == 1,
                    initials,
                    photoUrl,
                    stats = new
                    {
                        projectsOwned = ownedProjects,
                        projectsAsMember = memberProjects,
                        pendingTasks
                    }
                });
            }

            return Ok(new { data = rows });
        }

        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users.Select(Mapper.Map<dtoUser>));
        }

        [HttpPost]
        [Authorize]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetUsersForDataTables([FromBody] DataTablesRequest request)
        {
            request = request ?? new DataTablesRequest();

            var allUsers = await _userService.GetAllUsersAsync();
            var usersQuery = allUsers.AsQueryable();

            var searchValue = request.search != null ? request.search.value : string.Empty;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var search = searchValue.Trim();
                usersQuery = usersQuery.Where(u =>
                    (u.UserId ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.UserName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.FirstName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.LastName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (u.Email ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (request.isActive.HasValue)
            {
                var isActiveValue = request.isActive.Value ? 1 : 0;
                usersQuery = usersQuery.Where(u => u.IsActive == isActiveValue);
            }

            var sortBy = (request.sortBy ?? "USERNAME").ToUpperInvariant();
            var sortDirection = (request.sortDirection ?? "ASC").ToUpperInvariant();
            var sortAscending = sortDirection != "DESC";

            usersQuery = SortUsers(usersQuery, sortBy, sortAscending);

            var filteredCount = usersQuery.Count();
            var totalCount = allUsers.Count();

            var pageSize = request.length;
            if (pageSize <= 0 && pageSize != -1)
            {
                pageSize = 10;
            }

            IEnumerable<User> pagedData;
            if (pageSize == -1)
            {
                pagedData = usersQuery.ToList();
            }
            else
            {
                var start = request.start < 0 ? 0 : request.start;
                pagedData = usersQuery.Skip(start).Take(pageSize).ToList();
            }

            var response = new DataTablesResponse<dtoUser>
            {
                draw = request.draw,
                recordsTotal = totalCount,
                recordsFiltered = filteredCount,
                data = pagedData.Select(Mapper.Map<dtoUser>).ToList()
            };

            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        [Route("{id}")]
        public async Task<IHttpActionResult> GetById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(Mapper.Map<dtoUser>(user));
        }

        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USERADD")]
        public async Task<IHttpActionResult> Create()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var userContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "user");
            dtoUser user = null;
            if (userContent != null)
            {
                var userJson = await userContent.ReadAsStringAsync();
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoUser>(userJson);
            }

            if (user == null)
            {
                return BadRequest("User data is missing.");
            }

            var actionBy = User.Identity.GetClaim("employeeid");
            user.CreatedBy = actionBy;
            user.ModifiedBy = actionBy;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUserId = await _userService.AddUserAsync(Mapper.Map<User>(user));

            return Created("api/users/" + createdUserId, user);
        }

        [HttpPut]
        [Authorize]
        [Route("{id}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USEREDIT")]
        public async Task<IHttpActionResult> Update(string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var userContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "user");
            dtoUser user = null;
            if (userContent != null)
            {
                var userJson = await userContent.ReadAsStringAsync();
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoUser>(userJson);
            }

            if (user == null)
            {
                return BadRequest("User data is missing.");
            }

            var actionBy = User.Identity.GetClaim("employeeid");
            user.CreatedBy = actionBy;
            user.ModifiedBy = actionBy;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.Equals(id, user.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("User id mismatch.");
            }

            await _userService.UpdateUserAsync(Mapper.Map<User>(user));

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Authorize]
        [Route("{id}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USERDEL")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var userContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "user");
            dtoUser user = null;
            if (userContent != null)
            {
                var userJson = await userContent.ReadAsStringAsync();
                user = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoUser>(userJson);
            }

            if (user == null)
            {
                return BadRequest("User data is missing.");
            }

            if (!string.Equals(id, user.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("User id mismatch.");
            }

            var actionBy = User.Identity.GetClaim("employeeid");
            await _userService.DeleteUserAsync(id, actionBy);

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("{id}/groups")]
        public async Task<IHttpActionResult> GetGroups(string id)
        {
            var groups = await _userService.GetUserGroupsAsync(id);
            var response = new { data = groups.ToList() };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        [Route("{id}/plants")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USERVIEW")]
        public async Task<IHttpActionResult> GetPlants(string id)
        {
            var plants = (await _plantService.GetAllPlantsAsync() ?? Enumerable.Empty<Plant>())
                .Where(plant => plant != null)
                .OrderBy(plant => plant.PlantName)
                .ThenBy(plant => plant.PlantCode)
                .ToList();

            var memberships = (await _plantMemberRepository.GetListAsync(userid: id) ?? Enumerable.Empty<PlantMember>())
                .Where(member => member != null && !string.IsNullOrWhiteSpace(member.PlantCode))
                .GroupBy(member => member.PlantCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var response = plants.Select(plant =>
            {
                PlantMember membership;
                memberships.TryGetValue(plant.PlantCode, out membership);

                var hasActiveMembership = membership != null && membership.IsActive == 1;

                return new
                {
                    plantCode = plant.PlantCode,
                    plantName = plant.PlantName,
                    plantDescription = "",
                    plantMemberSysId = membership != null ? membership.PlantMemberSysId : null,
                    transactionKey = membership != null ? membership.TransactionKey : null,
                    hasMembership = membership != null,
                    isSelected = hasActiveMembership ? 1 : 0,
                    isActive = hasActiveMembership ? 1 : 0
                };
            }).ToList();

            return Ok(new { data = response });
        }

        [HttpPost]
        [Authorize]
        [Route("{id}/plant/link/{plantCode}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USEREDIT")]
        public async Task<IHttpActionResult> LinkPlant(string id, string plantCode)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(plantCode))
            {
                return BadRequest("User id and plant code are required.");
            }

            var existing = (await _plantMemberRepository.GetListAsync(plantcode: plantCode, userid: id) ?? Enumerable.Empty<PlantMember>())
                .FirstOrDefault();

            var actionBy = User.Identity.GetClaim("employeeid");
            if (existing == null)
            {
                var member = new PlantMember
                {
                    PlantCode = plantCode,
                    UserId = id,
                    CreatedBy = actionBy,
                    CreatedDate = DateTime.UtcNow
                };

                var createdId = await _plantService.AddMemberAsync(member);
                var created = await _plantMemberRepository.GetAsync(createdId);
                return Ok(new
                {
                    plantMemberSysId = created != null ? created.PlantMemberSysId : createdId,
                    transactionKey = created != null ? created.TransactionKey : null,
                    isActive = 1
                });
            }

            if (existing.IsActive != 1)
            {
                existing.IsActive = 1;
                existing.ModifiedBy = actionBy;
                await _plantService.UpdateMemberAsync(existing);
                existing = await _plantMemberRepository.GetAsync(existing.PlantMemberSysId);
            }

            return Ok(new
            {
                plantMemberSysId = existing.PlantMemberSysId,
                transactionKey = existing.TransactionKey,
                isActive = existing.IsActive
            });
        }

        [HttpDelete]
        [Authorize]
        [Route("{id}/plant/restrict/{plantCode}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USEREDIT")]
        public async Task<IHttpActionResult> RestrictPlant(string id, string plantCode)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(plantCode))
            {
                return BadRequest("User id and plant code are required.");
            }

            var existing = (await _plantMemberRepository.GetListAsync(plantcode: plantCode, userid: id) ?? Enumerable.Empty<PlantMember>())
                .FirstOrDefault();

            if (existing == null)
            {
                return NotFound();
            }

            if (existing.IsActive != 0)
            {
                existing.IsActive = 0;
                existing.ModifiedBy = User.Identity.GetClaim("employeeid");
                await _plantService.UpdateMemberAsync(existing);
                existing = await _plantMemberRepository.GetAsync(existing.PlantMemberSysId);
            }

            return Ok(new
            {
                plantMemberSysId = existing.PlantMemberSysId,
                transactionKey = existing.TransactionKey,
                isActive = existing.IsActive
            });
        }

        [HttpGet]
        [Authorize]
        [Route("groups/all")]
        public async Task<IHttpActionResult> GetAllGroups()
        {
            var groups = await _userGroupService.GetAllUserGroupsAsync();
            var response = new { data = groups.Select(Mapper.Map<dtoUserGroup>).ToList() };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        [Route("{id}/group/link/{usergroupid:int}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USEREDIT")]
        public async Task<IHttpActionResult> LinkGroup(string id, int usergroupid)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "usergroupmember");
            UserGroupMember obj = null;
            if (objContent != null)
            {
                var objJson = await objContent.ReadAsStringAsync();
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<UserGroupMember>(objJson);
            }

            if (obj == null)
            {
                return BadRequest("User group membership data is missing.");
            }

            if (!string.Equals(id, obj.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("User id mismatch.");
            }

            if (obj.UserGroupId != usergroupid)
            {
                return BadRequest("User group id mismatch.");
            }

            obj.CreatedBy = User.Identity.GetClaim("employeeid");
            await _userGroupMemberService.AddUserGroupMemberAsync(obj);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Authorize]
        [Route("{id}/group/unlink/{usergroupid:int}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USEREDIT")]
        public async Task<IHttpActionResult> UnlinkGroup(string id, int usergroupid)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "usergroupmember");
            UserGroupMember obj = null;
            if (objContent != null)
            {
                var objJson = await objContent.ReadAsStringAsync();
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<UserGroupMember>(objJson);
            }

            if (obj == null)
            {
                return BadRequest("User group membership data is missing.");
            }

            if (!string.Equals(id, obj.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("User id mismatch.");
            }

            if (obj.UserGroupId != usergroupid)
            {
                return BadRequest("User group id mismatch.");
            }

            var actionBy = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrEmpty(obj.Id))
            {
                var current = (await _userGroupMemberService.GetAllUserGroupMembersAsync(usergroupid, id)).SingleOrDefault();
                if (current == null)
                {
                    return NotFound();
                }

                await _userGroupMemberService.DeleteUserGroupMemberAsync(current.UserGroupMemberSysId, actionBy);
            }
            else
            {
                await _userGroupMemberService.DeleteUserGroupMemberAsync(obj.Id, actionBy);
            }

            return Ok();
        }

        private static IQueryable<User> SortUsers(IQueryable<User> users, string sortBy, bool asc)
        {
            switch (sortBy)
            {
                case "USERID":
                    return asc ? users.OrderBy(u => u.UserId) : users.OrderByDescending(u => u.UserId);
                case "FIRSTNAME":
                    return asc ? users.OrderBy(u => u.FirstName) : users.OrderByDescending(u => u.FirstName);
                case "LASTNAME":
                    return asc ? users.OrderBy(u => u.LastName) : users.OrderByDescending(u => u.LastName);
                case "EMAIL":
                    return asc ? users.OrderBy(u => u.Email) : users.OrderByDescending(u => u.Email);
                case "ISACTIVE":
                    return asc ? users.OrderBy(u => u.IsActive) : users.OrderByDescending(u => u.IsActive);
                case "USERNAME":
                default:
                    return asc ? users.OrderBy(u => u.UserName) : users.OrderByDescending(u => u.UserName);
            }
        }

        private static string BuildInitials(string firstName, string lastName, string userName, string userId)
        {
            var initials = string.Join(string.Empty, new[] { firstName, lastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim().Substring(0, 1).ToUpperInvariant()));

            if (!string.IsNullOrWhiteSpace(initials))
            {
                return initials;
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName.Trim().Substring(0, 1).ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return userId.Trim().Substring(0, 1).ToUpperInvariant();
            }

            return "?";
        }

        private static string BuildPhotoUrl(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return string.Empty;
            }

            return "/Settings/Profile/Photo?id=" + HttpUtility.UrlEncode(userId);
        }
    }
}
