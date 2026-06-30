using AutoMapper;
using Pulse.Api.Filters;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using Pulse.SharedUtilities.Extensions;
using Pulse.SharedUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/forms")]
    public class FormsController : ApiController
    {
        private readonly IFormService _formService;

        public FormsController(IFormService formService)
        {
            _formService = formService;
        }

        /// <summary>
        /// Gets all forms.
        /// </summary>
        [HttpGet]
        [Authorize]
    //    [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var forms = await _formService.GetAllFormsAsync();
            return Ok(forms);
        }





        /// <summary>
        /// Gets a paged list of forms with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for plant code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
     //   [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetFormsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "FORMSYSID", "FORMNAME", "FORMDESCRIPTION", "FORMJSON", "ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "FORMNAME";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "FORMNAME"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction


            var pagedResult = await _formService.GetPagedFormsAsync(searchValue, sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<FormExtended>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a form by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var form = await _formService.GetCompleteInfoFormByIdAsync(code);
            if (form == null)
                return NotFound();

            return Ok(Mapper.Map<dtoFormExtended>(form));
        }

        /// <summary>
        /// Creates a new form.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the plant data (assuming the input name is 'plant')
            var formContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "form");
            dtoFormExtended dtoformextended = null;
            if (formContent != null)
            {
                var formJson = await formContent.ReadAsStringAsync();
                dtoformextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoFormExtended>(formJson);
            }

            if (dtoformextended == null)
                return BadRequest("Plant data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var form = Mapper.Map<Form>(dtoformextended);

            form.Fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FormField>>(dtoformextended.FormJson).Select(Mapper.Map<FormField>).ToList();
            form.CreatedBy = User.Identity.GetClaim("employeeid");
            var id = await _formService.BuildFormAsync(form, form.CreatedBy);

            return Created($"api/forms/{id}", form);
        }

        /////// <summary>
        /////// Updates an existing form.
        /////// </summary>
        ////[HttpPut]
        ////[Authorize]
        ////[Route("{code}")]
        ////[RequireFormExistsAttribute]
        ////public async Task<IHttpActionResult> Update(string code)
        ////{
        ////    if (!Request.Content.IsMimeMultipartContent())
        ////        return BadRequest("Unsupported media type.");

        ////    var provider = new MultipartMemoryStreamProvider();
        ////    await Request.Content.ReadAsMultipartAsync(provider);

        ////    // Get the plant data (assuming the input name is 'plant')
        ////    var formContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "form");
        ////    dtoFormExtended dtoformextended = null;
        ////    if (formContent != null)
        ////    {
        ////        var formJson = await formContent.ReadAsStringAsync();
        ////        dtoformextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoFormExtended>(formJson);
        ////    }

        ////    if (dtoformextended == null)
        ////        return BadRequest("Plant data is missing.");
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    var form = Mapper.Map<Form>(dtoformextended);

        ////    form.Fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FormField>>(dtoformextended.FormJson).Select(Mapper.Map<FormField>).ToList();
        ////    form.ModifiedBy = User.Identity.GetClaim("employeeid");

        ////    await _formService.RebuildFormAsync(form, dtoformextended.TransactionKey, form.ModifiedBy);

        ////    return StatusCode(System.Net.HttpStatusCode.NoContent);
        ////}

        /// <summary>
        /// Updates an basic info of form.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        [RequireFormExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var formContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "form");
            dtoFormExtended dtoformextended = null;
            if (formContent != null)
            {
                var formJson = await formContent.ReadAsStringAsync();
                dtoformextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoFormExtended>(formJson);
            }
 
            if (dtoformextended == null)
                return BadRequest("Form data is missing.");

            if (code != dtoformextended.FormSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var form = Mapper.Map<Form>(dtoformextended);
             
            form.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _formService.UpdateAsync(form, dtoformextended.TransactionKey, form.ModifiedBy);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Updates an existing form.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("UpdateFields/{code}")]
        [RequireFormExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMEDIT")]
        public async Task<IHttpActionResult> UpdateFields(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'form')
            var formContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "form");
            dtoFormExtended dtoformextended = null;
            if (formContent != null)
            {
                var formJson = await formContent.ReadAsStringAsync();
                dtoformextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoFormExtended>(formJson);
            }


            if (dtoformextended == null)
                return BadRequest("Form data is missing.");


            if (code != dtoformextended.FormSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var form = Mapper.Map<Form>(dtoformextended);

            form.Fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FormField>>(dtoformextended.FormJson).Select(Mapper.Map<FormField>).ToList();
            form.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _formService.RebuildFormAsync(form, dtoformextended.TransactionKey, form.ModifiedBy);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Updates active status of a form field.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("fields/{fieldSysId}/status")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMEDIT")]
        public async Task<IHttpActionResult> ChangeFieldStatus(string fieldSysId, [FromBody] FormFieldStatusUpdateRequest request)
        {
            if (request == null)
                return BadRequest("Request body is missing.");

            if (string.IsNullOrWhiteSpace(fieldSysId))
                return BadRequest("Field id is required.");

            if (string.IsNullOrWhiteSpace(request.FormSysId))
                return BadRequest("Form id is required.");

            var form = await _formService.GetCompleteInfoFormByIdAsync(request.FormSysId);
            if (form == null)
                return NotFound();

            try
            {
                await _formService.ChangeFieldStatusAsync(fieldSysId, request.IsActive, User.Identity.GetClaim("employeeid"));
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Updates an basic info of form.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("ChangeStatus/{code}")]
        [RequireFormExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMEDIT")]
        public async Task<IHttpActionResult> ChangeStatus(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var formContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "form");
            dtoFormExtended dtoformextended = null;
            if (formContent != null)
            {
                var formJson = await formContent.ReadAsStringAsync();
                dtoformextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoFormExtended>(formJson);
            }

            if (dtoformextended == null)
                return BadRequest("Form data is missing.");

            if (code != dtoformextended.FormSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var form = Mapper.Map<Form>(dtoformextended);
             
            form.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _formService.ChangeStatusAsync(form, dtoformextended.TransactionKey, form.ModifiedBy);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }



        /// <summary>
        /// Deletes a form by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        [RequireFormExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "FORMDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var formContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "form");
            dtoFormExtended dtoformextended = null;
            if (formContent != null)
            {
                var formJson = await formContent.ReadAsStringAsync();
                dtoformextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoFormExtended>(formJson);
            }

            if (dtoformextended == null)
                return BadRequest("Form data is missing.");

            if (code != dtoformextended.FormSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
  
            var form = await _formService.GetFormByIdAsync(code);

            if (form == null)
                return NotFound();

            await _formService.DeleteFormAsync(code, User.Identity.GetClaim("employeeid"));
            return Ok();
        }
    }
}
