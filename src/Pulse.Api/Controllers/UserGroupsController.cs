using AutoMapper;
using Pulse.Api.Filters;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using Pulse.SharedUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    //[Authorize]
    [RoutePrefix("api/usergroups")]
    public class UserGroupsController : ApiController
    {
        private readonly IUserGroupService _usergroupService;
        private readonly IUserGroupMemberService _usergroupmemberService;

        public UserGroupsController(IUserGroupService usergroupService, IUserGroupMemberService usergroupmemberService)
        {
            _usergroupService = usergroupService;
            _usergroupmemberService = usergroupmemberService;
        }

        /// <summary>
        /// Gets all usergroups.
        /// </summary>
        [HttpGet]
        [Authorize]
     //   [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var usergroups = await _usergroupService.GetAllUserGroupsAsync();
            return Ok(usergroups);
        }




        /// <summary>
        /// Gets a paged list of usergroups with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for usergroup code or name (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetUserGroupsForDataTables([FromBody] DataTablesRequest request)
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

            string searchValue = request.search?.value ?? "";
            bool? isActive = request.isActive;


            // Whitelist of allowed columns and directions
            var allowedColumns = new HashSet<string> { "USERGROUPNAME", "ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "USERGROUPNAME";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy))
                sortBy = "USERGROUPNAME"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction


            var pagedResult = await _usergroupService.GetPagedUserGroupsAsync(searchValue, sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoUserGroup>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoUserGroup>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a usergroup by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPVIEW")]
        [Route("{id}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var usergroup = await _usergroupService.GetUserGroupByIdAsync(id);
            if (usergroup == null)
                return NotFound();

            return Ok(Mapper.Map<dtoUserGroup>(usergroup));
        }

        /// <summary>
        /// Creates a new usergroup.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the usergroup data (assuming the input name is 'usergroup')
            var usergroupContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroup");
            dtoUserGroup usergroup = null;
            if (usergroupContent != null)
            {
                var usergroupJson = await usergroupContent.ReadAsStringAsync();
                usergroup = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoUserGroup>(usergroupJson);
            }

            if (usergroup == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            usergroup.CreatedBy = User.Identity.GetClaim("employeeid");


            await _usergroupService.AddUserGroupAsync(Mapper.Map<UserGroup>(usergroup));

            return Created($"api/usergroups/{usergroup.UserGroupId}", usergroup);
        }

        /// <summary>
        /// Updates an existing usergroup.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{id}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPEDIT")]
        //[RequireUserGroupIdExists]
        public async Task<IHttpActionResult> Update(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var usergroupContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroup");
            dtoUserGroup usergroup = null;
            if (usergroupContent != null)
            {
                var usergroupJson = await usergroupContent.ReadAsStringAsync();
                usergroup = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoUserGroup>(usergroupJson);
            }

            if (usergroup == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != usergroup.UserGroupId)
                return BadRequest("UserGroup Code mismatch.");

            usergroup.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _usergroupService.UpdateUserGroupAsync(Mapper.Map<UserGroup>(usergroup));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a usergroup by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{id}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPDEL")]
        //[RequireUserGroupIdExists]
        public async Task<IHttpActionResult> Delete(int id)
        {

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var usergroupContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroup");
            dtoUserGroup usergroup = null;
            if (usergroupContent != null)
            {
                var usergroupJson = await usergroupContent.ReadAsStringAsync();
                usergroup = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoUserGroup>(usergroupJson);
            }

            if (usergroup == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != usergroup.UserGroupId)
                return BadRequest("UserGroup Code mismatch.");

            usergroup.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _usergroupService.UpdateUserGroupAsync(Mapper.Map<UserGroup>(usergroup));

            await _usergroupService.DeleteUserGroupAsync(id, "");
            return Ok();
        }


        #region MEMBERS
        /// <summary>
        /// Gets all members.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPVIEW")]
        [Route("{id}/members")]
        public async Task<IHttpActionResult> GetMembers(int id)
        {
            var obj = await _usergroupmemberService.GetAllUserGroupMembersAsync(id, null);

            var response = new
            {
                data = obj.ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Link user to usergroup.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("{id}/member/link")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPEDIT")]
        //[RequireUserGroupIdExists]
        public async Task<IHttpActionResult> LinkToUserGroup(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroupmember");
            UserGroupMember obj = null;
            if (objContent != null)
            {
                var objJson = await objContent.ReadAsStringAsync();
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<UserGroupMember>(objJson);
            }

            if (obj == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != obj.UserGroupId)
                return BadRequest("UserGroup Code mismatch.");

            obj.CreatedBy = User.Identity.GetClaim("employeeid");


            await _usergroupmemberService.AddUserGroupMemberAsync(obj);


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a usergroup by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{id}/member/unlink")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPEDIT")]
        public async Task<IHttpActionResult> UnlinkToUserGroup(int id)
        {

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroupmember");
            UserGroupMember obj = null;
            if (objContent != null)
            {
                var usergroupJson = await objContent.ReadAsStringAsync();
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<UserGroupMember>(usergroupJson);
            }

            if (obj == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != obj.UserGroupId)
                return BadRequest("UserGroup Code mismatch.");

            obj.CreatedBy = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrEmpty(obj.Id))
            {
                var usergroupmoduleCurrent = (await _usergroupmemberService.GetAllUserGroupMembersAsync(obj.UserGroupId.Value, obj.UserId)).SingleOrDefault().UserGroupMemberSysId;

                await _usergroupmemberService.DeleteUserGroupMemberAsync(usergroupmoduleCurrent, obj.CreatedBy);
            }
            else
            {

                await _usergroupmemberService.DeleteUserGroupMemberAsync(obj.Id, obj.CreatedBy);
            }

            return Ok();
        }
        #endregion

        #region MODULES
        /// <summary>
        /// Gets all usergroups.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPVIEW")]
        [Route("{id}/modules")]
        public async Task<IHttpActionResult> GetModules(int id)
        {
            var modules = await _usergroupService.GetModulesAsync(id);

            var response = new
            {
                data = modules.ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Link module to usergroup.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("{id}/module/authorize/{modulecode}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPEDIT")]
        //[RequireUserGroupIdExists]
        public async Task<IHttpActionResult> AllowToModule(int id, string modulecode)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroupmodule");
            UserGroupModule obj = null;
            if (objContent != null)
            {
                var usergroupJson = await objContent.ReadAsStringAsync();
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<UserGroupModule>(usergroupJson);
            }

            if (obj == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != obj.UserGroupId)
                return BadRequest("UserGroup Code mismatch.");

            obj.CreatedBy = User.Identity.GetClaim("employeeid");


            await _usergroupService.AuthorizeToModule(obj);


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a usergroup by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{id}/module/restrict/{modulecode}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "USRGRPEDIT")]
        public async Task<IHttpActionResult> RestrictToModule(int id, string modulecode)
        {

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var objContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "usergroupmodule");
            UserGroupModule obj = null;
            if (objContent != null)
            {
                var usergroupJson = await objContent.ReadAsStringAsync();
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<UserGroupModule>(usergroupJson);
            }

            if (obj == null)
                return BadRequest("UserGroup data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != obj.UserGroupId)
                return BadRequest("UserGroup Code mismatch.");
            if (modulecode != obj.ModuleCode)
                return BadRequest("Module Code mismatch.");

            obj.CreatedBy = User.Identity.GetClaim("employeeid");
            if (string.IsNullOrEmpty(obj.Id))
            {
                var usergroupmoduleCurrent = await _usergroupService.GetModuleAsync(obj.UserGroupId, obj.ModuleCode);

                await _usergroupService.RestrictToModule(usergroupmoduleCurrent);
            }
            else
            {

                await _usergroupService.RestrictToModule(obj);
            }

            return Ok();
        }
        #endregion

    }
}
