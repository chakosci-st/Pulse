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
    [RoutePrefix("api/modules")]
    public class ModulesController : ApiController
    {
        private readonly IModuleService _moduleService;

        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        /// <summary>
        /// Gets all modules.
        /// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MODULEVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var modules = await _moduleService.GetAllModulesAsync();
            return Ok(modules);
        }

        /// <summary>
        /// Gets a paged list of modules with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for module code or name (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
     //   [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MODULEVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetModulesForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "MODULECODE", "MODULENAME", "ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "MODULECODE";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy))
                sortBy = "MODULECODE"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction

 
            var pagedResult = await _moduleService.GetPagedModulesAsync(searchValue, sortBy, sortDir,  isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoModule>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoModule>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a module by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MODULEVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var module = await _moduleService.GetModuleByCodeAsync(code);
            if (module == null)
                return NotFound();

            return Ok(Mapper.Map<dtoModule>(module));
        }

        /// <summary>
        /// Creates a new module.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MODULEADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the module data (assuming the input name is 'module')
            var moduleContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "module");
            dtoModule module = null;
            if (moduleContent != null)
            {
                var moduleJson = await moduleContent.ReadAsStringAsync();
                module = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoModule>(moduleJson);
            }

            if (module == null)
                return BadRequest("Module data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            module.CreatedBy = User.Identity.GetClaim("employeeid");


            await _moduleService.AddModuleAsync(Mapper.Map<Module>(module));

            return Created($"api/modules/{module.ModuleCode}", module);
        }

        /// <summary>
        /// Updates an existing module.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        [RequireModuleCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MODULEEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var moduleContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "module");
            dtoModule module = null;
            if (moduleContent != null)
            {
                var moduleJson = await moduleContent.ReadAsStringAsync();
                module = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoModule>(moduleJson);
            }

            if (module == null)
                return BadRequest("Module data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (code != module.ModuleCode)
                return BadRequest("Module Code mismatch.");

            module.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _moduleService.UpdateModuleAsync(Mapper.Map<Module>(module));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a module by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        [RequireModuleCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MODULEDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {

            var module = _moduleService.GetModuleByCodeAsync(code);
            if (module == null)
                return NotFound();

            await _moduleService.DeleteModuleAsync(code, "");
            return Ok();
        }
    } 
}
