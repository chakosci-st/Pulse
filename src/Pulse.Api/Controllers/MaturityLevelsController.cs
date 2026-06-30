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
    [RoutePrefix("api/maturitylevels")]
    public class MaturityLevelsController : ApiController
    {
        private readonly IMaturityLevelService _maturitylevelService;

        public MaturityLevelsController(IMaturityLevelService maturitylevelService)
        {
            _maturitylevelService = maturitylevelService;
        }

        /// <summary>
        /// Gets all maturitylevels.
        /// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MATVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var maturitylevels = await _maturitylevelService.GetAllMaturityLevelsAsync();
            return Ok(maturitylevels);
        }

        /// <summary>
        /// Gets a paged list of maturitylevels with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for maturitylevel code or name (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MATVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetMaturityLevelsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "MATURITYCODE", "MATURITYNUMBER", "SEQUENCENO","ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "MATURITYCODE";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "MATURITYCODE"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction


            var pagedResult = await _maturitylevelService.GetPagedMaturityLevelsAsync(searchValue, sortBy,  sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoMaturityLevelWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoMaturityLevelWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a maturitylevel by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MATVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var maturitylevel = await _maturitylevelService.GetMaturityLevelByCodeAsync(code);
            if (maturitylevel == null)
                return NotFound();

            return Ok(Mapper.Map<dtoMaturityLevel>(maturitylevel));
        }

        /// <summary>
        /// Creates a new maturitylevel.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MATADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
 

            // Get the maturitylevel data (assuming the input name is 'maturitylevel')
            var maturitylevelContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "maturitylevel");
            dtoMaturityLevel maturitylevel = null;
            if (maturitylevelContent != null)
            {
                var maturitylevelJson = await maturitylevelContent.ReadAsStringAsync();
                maturitylevel = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoMaturityLevel>(maturitylevelJson);
            }

            if (maturitylevel == null)
                return BadRequest("MaturityLevel data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            maturitylevel.CreatedBy = User.Identity.GetClaim("employeeid");
 

            await _maturitylevelService.AddMaturityLevelAsync(Mapper.Map<MaturityLevel>(maturitylevel));

            return Created($"api/maturitylevels/{maturitylevel.MaturityCode}", maturitylevel);
        }

        /// <summary>
        /// Updates an existing maturitylevel.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MATEDIT")]
        //[RequireMaturityLevelCodeExists]
        public async Task<IHttpActionResult> Update(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

 

            var maturitylevelContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "maturitylevel");
            dtoMaturityLevel maturitylevel = null;
            if (maturitylevelContent != null)
            {
                var maturitylevelJson = await maturitylevelContent.ReadAsStringAsync();
                maturitylevel = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoMaturityLevel>(maturitylevelJson);
            }

            if (maturitylevel == null)
                return BadRequest("MaturityLevel data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (code != maturitylevel.MaturityCode)
                return BadRequest("MaturityLevel Code mismatch.");

            maturitylevel.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _maturitylevelService.UpdateMaturityLevelAsync(Mapper.Map<MaturityLevel>(maturitylevel));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a maturitylevel by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        //[RequireMaturityLevelCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "MATDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {

            var maturitylevel = _maturitylevelService.GetMaturityLevelByCodeAsync(code);
            if (maturitylevel == null)
                return NotFound();

            await _maturitylevelService.DeleteMaturityLevelAsync(code, "");
            return Ok();
        }
    }

     
}
