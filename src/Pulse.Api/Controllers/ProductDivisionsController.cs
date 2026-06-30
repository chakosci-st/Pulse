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
    [RoutePrefix("api/productdivisions")]
    public class ProductDivisionsController : ApiController
    {
        private readonly IProductDivisionService _productdivisionService;

        public ProductDivisionsController(IProductDivisionService productdivisionService)
        {
            _productdivisionService = productdivisionService;
        }

        /// <summary>
        /// Gets all productdivisions.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var productdivisions = await _productdivisionService.GetAllProductDivisionsAsync();
            return Ok(productdivisions);
        }

        /// <summary>
        /// Gets a paged list of productdivisions with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for productdivision code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PDIVVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetProductDivisionsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "PRODUCTDIVISIONCODE", "PRODUCTDIVISIONNAME", "ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "PRODUCTDIVISIONCODE";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "PRODUCTDIVISIONCODE"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction

            var pagedResult = await _productdivisionService.GetPagedProductDivisionsAsync(searchValue,  sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoProductDivisionWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoProductDivisionWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a productdivision by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PDIVVIEW")]
        [Route("{code:string}")]

        public async Task<IHttpActionResult> GetById(string code)
        {
            var productdivision = await _productdivisionService.GetProductDivisionByCodeAsync(code);
            if (productdivision == null)
                return NotFound();

            return Ok(Mapper.Map<dtoProductDivision>(productdivision));
        }

        /// <summary>
        /// Creates a new productdivision.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PDIVADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the productdivision data (assuming the input name is 'productdivision')
            var productdivisionContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "productdivision");
            dtoProductDivision productdivision = null;
            if (productdivisionContent != null)
            {
                var productdivisionJson = await productdivisionContent.ReadAsStringAsync();
                productdivision = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoProductDivision>(productdivisionJson);
            }

            if (productdivision == null)
                return BadRequest("Product Division data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            productdivision.CreatedBy = User.Identity.GetClaim("employeeid");


            await _productdivisionService.AddProductDivisionAsync(Mapper.Map<ProductDivision>(productdivision));

            return Created($"api/productdivisions/{productdivision.ProductDivisionCode}", productdivision);
        }

        /// <summary>
        /// Updates an existing productdivision.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{id:string}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PDIVEDIT")]
        public async Task<IHttpActionResult> Put(string id, [FromBody] dtoProductDivision productDivision)
        {
            if (productDivision == null)
                return BadRequest("Product Division data is missing.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.Equals(id, productDivision.ProductDivisionCode, System.StringComparison.OrdinalIgnoreCase))
                return BadRequest("Product Division Code mismatch.");

            productDivision.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _productdivisionService.UpdateProductDivisionAsync(
                Mapper.Map<ProductDivision>(productDivision));

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a productdivision by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{id:string}")]
        //[RequireProductDivisionCodeExists] 
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PDIVDEL")]
        public async Task<IHttpActionResult> Delete(string id)
        {

            var productdivision = _productdivisionService.GetProductDivisionByCodeAsync(id);
            if (productdivision == null)
                return NotFound();

            await _productdivisionService.DeleteProductDivisionAsync(id, "");
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
     
}
