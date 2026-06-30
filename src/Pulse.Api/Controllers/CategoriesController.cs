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
    [RoutePrefix("api/categories")]

    public class CategoriesController : ApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        [HttpGet]
        [Authorize]
        //[AuthorizeUserGroupAttribute(Modules = "CATGRYVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Gets a paged list of categories with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for category code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
        //[AuthorizeUserGroupAttribute(Modules = "CATGRYVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetCategoriesForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "CATEGORYCODE", "CATEGORYNAME", "ISACTIVE", "ACTIVEPROJECTSCOUNT", "ACTIVETASKSCOUNT", "PRODUCTCOUNT", "TASKDUECOUNT" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "CATEGORYCODE";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "CATEGORYCODE"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction

            var pagedResult = await _categoryService.GetPagedCategoriesAsync(searchValue, sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoCategoryWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoCategoryWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a category by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
        //[AuthorizeUserGroupAttribute(Modules = "CATGRYVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var category = await _categoryService.GetCategoryByCodeAsync(code);
            if (category == null)
                return NotFound();

            return Ok(Mapper.Map<dtoCategory>(category));
        }

        /// <summary>
        /// Creates a new category.
        /// </summary> 
        [HttpPost]
        [Authorize]
        [Route("")] 
        [AuthorizeUserGroupAttribute(Modules = "CATGRYADD")]
        public async Task<IHttpActionResult> Create()
        {
            System.Diagnostics.Debug.WriteLine(">>> CategoriesController.Get HIT <<<");

            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the category data (assuming the input name is 'category')
            var categoryContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "category");
            dtoCategory category = null;
            if (categoryContent != null)
            {
                var categoryJson = await categoryContent.ReadAsStringAsync();
                category = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoCategory>(categoryJson);
            }

            if (category == null)
                return BadRequest("Category data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            category.CreatedBy = User.Identity.GetClaim("employeeid");


            await _categoryService.AddCategoryAsync(Mapper.Map<Category>(category));

            return Created($"api/categories/{category.CategoryCode}", category);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")] 
        [RequireCategoryCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "CATGRYEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            System.Diagnostics.Debug.WriteLine(">>> CategoriesController Update.Get HIT <<<");
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            var categoryContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "category");
            dtoCategory category = null;
            if (categoryContent != null)
            {
                var categoryJson = await categoryContent.ReadAsStringAsync();
                category = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoCategory>(categoryJson);
            }

            if (category == null)
                return BadRequest("Category data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (code != category.CategoryCode)
                return BadRequest("Category Code mismatch.");

            category.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _categoryService.UpdateCategoryAsync(Mapper.Map<Category>(category));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a category by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        [RequireCategoryCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "CATGRYDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {

            var category = _categoryService.GetCategoryByCodeAsync(code);
            if (category == null)
                return NotFound();

            await _categoryService.DeleteCategoryAsync(code, "");
            return Ok();
        }
    }
    

    /*
      [RoutePrefix("api/categories")]
    public class CategoriesController : ApiController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var categories = _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetById(int id)
        {
            var category = _categoryService.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpGet]
        [Route("location/{location}")]
        public IHttpActionResult GetByLocation(string location)
        {
            var categories = _categoryService.GetByLocation(location);
            return Ok(categories);
        }
    }
     */
}
