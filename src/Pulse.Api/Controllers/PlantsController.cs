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
    [RoutePrefix("api/plants")]
    public class PlantsController : ApiController
    {
        private readonly IPlantService _plantService;

        public PlantsController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        /// <summary>
        /// Gets all plants.
        /// </summary>
        [HttpGet]
        [Authorize]
       // [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTVIEW")]
        [Route("")] 
        public async Task<IHttpActionResult> GetAll()
        {
            var plants = await _plantService.GetAllPlantsAsync();
            return Ok(plants);
        }


        [HttpGet]
        [Authorize]
        [Route("Allowed")] 
        public async Task<IHttpActionResult> GetAllowedList()
        {
            var loggedUser = User?.Identity?.GetClaim("employeeid");

            // If no user or no employeeid claim -> force re-authentication
            if (string.IsNullOrEmpty(loggedUser))
            {
                // Option 1: simple 401
                return Unauthorized();

                // Option 2: add a message (if you prefer)
                //return Content(HttpStatusCode.Unauthorized, "User is not authenticated. Please sign in again.");
            }

            var plants = await _plantService.GetAllPlantsByUserAsync(loggedUser);
            return Ok(plants);
        }


        /// <summary>
        /// Gets a paged list of plants with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for plant code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
     //   [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTVIEW")]
        [Route("datatables")] 
        public async Task<IHttpActionResult> GetPlantsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "PLANTCODE", "PLANTNAME", "ISACTIVE", "ACTIVEPROJECTSCOUNT", "ACTIVETASKSCOUNT", "PRODUCTCOUNT", "TASKDUECOUNT" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "PLANTCODE";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "PLANTCODE"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction


            var pagedResult = await _plantService.GetPagedPlantsAsync(searchValue, sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoPlantWithStats>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).Select(Mapper.Map<dtoPlantWithStats>).ToList()
            };

            return Ok(response);
        }


        ///// <summary>
        ///// Gets a plant by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
       // [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTVIEW")]
        [Route("{code:string}")] 
        public async Task<IHttpActionResult> GetById(string code)
        {
            var plant = await _plantService.GetPlantByCodeAsync(code);
            if (plant == null)
                return NotFound();

            return Ok(Mapper.Map<dtoPlant>(plant));
        }

        /// <summary>
        /// Creates a new plant.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTADD")]
        public async Task<IHttpActionResult> Create()
        {
            var loggedUser = User.Identity.GetClaim("employeeid");

            if (string.IsNullOrEmpty(loggedUser))
            {
                // Option 1: simple 401
                return Unauthorized();

            }


            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the file (assuming the input name is 'file')
            var file = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "file");
            byte[] fileBytes = null;
            string fileName = null;
            if (file != null)
            {
                fileBytes = await file.ReadAsByteArrayAsync();
                var originalFileName = file.Headers.ContentDisposition.FileName?.Trim('\"');
                var fileExtension = System.IO.Path.GetExtension(originalFileName);
                // Use PlantCode as file name (will get from plant object below)
                fileName = fileExtension; // We'll update this after deserializing plant
            }

            // Get the plant data (assuming the input name is 'plant')
            var plantContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plant");
            dtoPlant plant = null;
            if (plantContent != null)
            {
                var plantJson = await plantContent.ReadAsStringAsync();
                plant = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoPlant>(plantJson);
            }

            if (plant == null)
                return BadRequest("Plant data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            plant.CreatedBy = loggedUser;

            // Set file name to PlantCode + extension if file was uploaded
            if (fileBytes != null && fileName != null)
            {
                fileName = plant.PlantCode + fileName;
            }

            await _plantService.AddPlantAsync(Mapper.Map<Plant>(plant), fileBytes, fileName);

            return Created($"api/plants/{plant.PlantCode}", plant);
        }

        /// <summary>
        /// Updates an existing plant.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        [RequirePlantCodeExists] 
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var file = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "file");
            byte[] fileBytes = null;
            string newFileName = null;
            if (file != null)
            {
                fileBytes = await file.ReadAsByteArrayAsync();
                var originalFileName = file.Headers.ContentDisposition.FileName?.Trim('\"');
                var fileExtension = System.IO.Path.GetExtension(originalFileName);
                newFileName = code + (fileExtension ?? "");
            }

            var plantContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plant");
            dtoPlant plant = null;
            if (plantContent != null)
            {
                var plantJson = await plantContent.ReadAsStringAsync();
                plant = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoPlant>(plantJson);
            }

            if (plant == null)
                return BadRequest("Plant data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (code != plant.PlantCode)
                return BadRequest("Plant Code mismatch.");

            plant.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _plantService.UpdatePlantAsync(Mapper.Map<Plant>(plant), fileBytes, newFileName);


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a plant by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        [RequirePlantCodeExists] 
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {

            var plant = _plantService.GetPlantByCodeAsync(code);
            if (plant == null)
                return NotFound();

            await _plantService.DeletePlantAsync(code, "");
            return Ok();
        }
    }


    /*
      [RoutePrefix("api/plants")]
    public class PlantsController : ApiController
    {
        private readonly IPlantService _plantService;

        public PlantsController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var plants = _plantService.GetAllPlantsAsync();
            return Ok(plants);
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetById(int id)
        {
            var plant = _plantService.GetById(id);
            if (plant == null)
            {
                return NotFound();
            }
            return Ok(plant);
        }

        [HttpGet]
        [Route("location/{location}")]
        public IHttpActionResult GetByLocation(string location)
        {
            var plants = _plantService.GetByLocation(location);
            return Ok(plants);
        }
    }
     */
}
