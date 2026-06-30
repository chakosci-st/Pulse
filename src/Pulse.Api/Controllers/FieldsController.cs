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
    [RoutePrefix("api/fields")]

    public class FieldsController : ApiController
    {
        private readonly IFieldService _fieldService;

        public FieldsController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        /// <summary>
        /// Gets all fields.
        /// </summary>
        [HttpGet]
        [Authorize]
       // [AuthorizeUserGroupAttribute(Modules = "FIELDVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var fields = await _fieldService.GetAllFieldsAsync();
            return Ok(fields);
        }

        /// <summary>
        /// Gets a paged list of fields with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for field code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [AuthorizeUserGroupAttribute(Modules = "FIELDVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetFieldsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "FIELDTITLE", "FORMLINKEDCOUNT" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "FIELDTITLE";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "FIELDTITLE"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction

            var pagedResult = await _fieldService.GetPagedListAsync(searchValue, sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoFieldWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoFieldWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a field by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
     //   [AuthorizeUserGroupAttribute(Modules = "FIELDVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var field = await _fieldService.GetFieldByIdAsync(code);
            if (field == null)
                return NotFound();

            return Ok(Mapper.Map<dtoField>(field));
        }

        /// <summary>
        /// Creates a new field.
        /// </summary> 
        [HttpPost]
        [Authorize]
        [Route("")] 
        [AuthorizeUserGroupAttribute(Modules = "FIELDADD")]
        public async Task<IHttpActionResult> Create()
        {
            System.Diagnostics.Debug.WriteLine(">>> FieldsController.Get HIT <<<");

            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the field data (assuming the input name is 'field')
            var fieldContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "field");
            dtoField field = null;
            if (fieldContent != null)
            {
                var fieldJson = await fieldContent.ReadAsStringAsync();
                field = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoField>(fieldJson);
            }

            if (field == null)
                return BadRequest("Field data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            field.CreatedBy = User.Identity.GetClaim("employeeid");


            await _fieldService.AddAsync(Mapper.Map<Field>(field));

            return Created($"api/fields/{field.FieldSysId}", field);
        }

        /// <summary>
        /// Updates an existing field.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]  
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FIELDEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            System.Diagnostics.Debug.WriteLine(">>> FieldsController Update.Get HIT <<<");
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var fieldContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "field");
            dtoField field = null;
            if (fieldContent != null)
            {
                var fieldJson = await fieldContent.ReadAsStringAsync();
                field = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoField>(fieldJson);
            }

            if (field == null)
                return BadRequest("Field data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (code != field.FieldSysId)
                return BadRequest("Field Code mismatch.");

            field.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _fieldService.UpdateAsync(Mapper.Map<Field>(field));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a field by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")] 
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FIELDDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {

            var field = _fieldService.GetFieldByIdAsync(code);
            if (field == null)
                return NotFound();
            var loggeduser = User.Identity.GetClaim("employeeid");
            await _fieldService.DeleteAsync(code, loggeduser);
            return Ok();
        }
    }
    

    /*
      [RoutePrefix("api/fields")]
    public class FieldsController : ApiController
    {
        private readonly IFieldService _fieldService;

        public FieldsController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var fields = _fieldService.GetAllFieldsAsync();
            return Ok(fields);
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetById(int id)
        {
            var field = _fieldService.GetById(id);
            if (field == null)
            {
                return NotFound();
            }
            return Ok(field);
        }

        [HttpGet]
        [Route("location/{location}")]
        public IHttpActionResult GetByLocation(string location)
        {
            var fields = _fieldService.GetByLocation(location);
            return Ok(fields);
        }
    }
     */
}
