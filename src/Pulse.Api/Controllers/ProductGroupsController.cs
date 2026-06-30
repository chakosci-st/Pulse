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
    [RoutePrefix("api/productgroups")]
    public class ProductGroupsController : ApiController
    {
        private readonly IProductGroupService _productgroupService;

        public ProductGroupsController(IProductGroupService productgroupService)
        {
            _productgroupService = productgroupService;
        }

        /// <summary>
        /// Gets all productgroups.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var productgroups = await _productgroupService.GetAllProductGroupsAsync();
            return Ok(productgroups);
        }

        /// <summary>
        /// Gets a paged list of productgroups with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for productgroup code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PGRPVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetProductGroupsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "PRODUCTGROUPCODE", "PRODUCTGROUPNAME", "ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "PRODUCTGROUPNAME";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "PRODUCTGROUPNAME"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction

            var pagedResult = await _productgroupService.GetPagedProductGroupsAsync(searchValue,  sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoProductGroupWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoProductGroupWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a productgroup by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PGRPVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var productgroup = await _productgroupService.GetProductGroupByCodeAsync(code);
            if (productgroup == null)
                return NotFound();

            return Ok(Mapper.Map<dtoProductGroup>(productgroup));
        }

        /// <summary>
        /// Creates a new productgroup.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PGRPADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the productgroup data (assuming the input name is 'productgroup')
            var productgroupContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "productgroup");
            dtoProductGroup productgroup = null;
            if (productgroupContent != null)
            {
                var productgroupJson = await productgroupContent.ReadAsStringAsync();
                productgroup = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoProductGroup>(productgroupJson);
            }

            if (productgroup == null)
                return BadRequest("Product Division data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            productgroup.CreatedBy = User.Identity.GetClaim("employeeid");


            await _productgroupService.AddProductGroupAsync(Mapper.Map<ProductGroup>(productgroup));

            return Created($"api/productgroups/{productgroup.ProductGroupCode}", productgroup);
        }

        /// <summary>
        /// Updates an existing productgroup.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        //[RequireProductGroupCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PGRPEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var productgroupContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "productgroup");
            dtoProductGroup productgroup = null;
            if (productgroupContent != null)
            {
                var productgroupJson = await productgroupContent.ReadAsStringAsync();
                productgroup = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoProductGroup>(productgroupJson);
            }

            if (productgroup == null)
                return BadRequest("Product Division data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (code != productgroup.ProductGroupCode)
                return BadRequest("Product Division Code mismatch.");

            productgroup.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _productgroupService.UpdateProductGroupAsync(Mapper.Map<ProductGroup>(productgroup));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a productgroup by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        //[RequireProductGroupCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PGRPDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {

            var productgroup = _productgroupService.GetProductGroupByCodeAsync(code);
            if (productgroup == null)
                return NotFound();

            await _productgroupService.DeleteProductGroupAsync(code, "");
            return Ok();
        }
    }
     
}
