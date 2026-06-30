using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
    [RoutePrefix("api/roadmaps")]
    public class RoadmapsController : ApiController
    {
        private readonly IRoadmapService _roadmapService;
        private readonly IPlantService _plantService;

        public RoadmapsController(IRoadmapService roadmapService, IPlantService plantService)
        {
            _roadmapService = roadmapService;
            _plantService = plantService;
        }

        /// <summary>
        /// Gets all roadmaps.
        /// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPVIEW")]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var roadmaps = await _roadmapService.GetAllRoadmapsAsync();
            return Ok(roadmaps);
        }

        ///// <summary>
        ///// Gets a roadmap by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPVIEW")]
        [Route("{code:string}")]
        public async Task<IHttpActionResult> GetById(string code)
        {
            var roadmap = await _roadmapService.GetCompleteInfoRoadmapByIdAsync(code);
            if (roadmap == null)
                return NotFound();

            return Ok(Mapper.Map<dtoRoadmapExtended>(roadmap));
        }

        ///// <summary>
        ///// Gets a roadmap by CODE.
        ///// </summary>
        [HttpGet]
        [Authorize]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPVIEW")]
        [Route("full/{code:string}")]
        [Route("full")]
        public async Task<IHttpActionResult> GetFullInformationById(string code)
        {
            var roadmap = Mapper.Map<dtoRoadmapExtended>(await _roadmapService.GetCompleteInfoRoadmapByIdAsync(code));
            if (roadmap == null)
                return NotFound();

            var result = await _roadmapService.GetTreeResponseAsync(code);
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(new
            {
                roadmapsysid = roadmap.RoadmapSysId,
                treeData = result.TreeData,
                rootForms = result.RootForms
            }, settings);

            roadmap.RoadmapJson = json;

            return Ok(roadmap);
        }


        /// <summary>
        /// Gets roadmap json.
        /// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPVIEW")]
        [Route("treemap/{code:string}")]
        [Route("treemap")]
        public async Task<IHttpActionResult> GetTreeMapOnly(string code)
        {
            var result = await _roadmapService.GetTreeResponseAsync(code);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            return Ok(json);
        }




        /// <summary>
        /// Gets a paged list of roadmaps with optional search and active status filter.
        /// </summary>
        /// <param name="search">Search term for plant code or name (optional).</param>
        /// <param name="sortBy">Sort by active status (optional).</param>
        /// <param name="sortDirection">Sort direction (ASC/DESC) by active status (optional).</param>
        /// <param name="isActive">Filter by active status (optional).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        [HttpPost]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPVIEW")]
        [Route("datatables")]
        public async Task<IHttpActionResult> GetRoadmapsForDataTables([FromBody] DataTablesRequest request)
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
            var allowedColumns = new HashSet<string> { "ROADMAPSYSID", "ROADMAPNAME", "ROADMAPDESCRIPTION", "CATEGORYCODE", "CATEGORYNAME", "ISACTIVE" };
            var allowedDirections = new HashSet<string> { "ASC", "DESC" };

            // Get user input (e.g., from request)
            string sortBy = request.sortBy ?? "ROADMAPNAME";
            string sortDir = request.sortDirection ?? "ASC";

            // Validate input
            if (!allowedColumns.Contains(sortBy.ToUpper()))
                sortBy = "ROADMAPNAME"; // default column

            if (!allowedDirections.Contains(sortDir.ToUpper()))
                sortDir = "ASC"; // default direction


            var pagedResult = await _roadmapService.GetPagedRoadmapsAsync(searchValue, sortBy, sortDir, isActive, pageNumber, pageSize);

            // Prepare DataTables response
            var response = new DataTablesResponse<RoadmapExtended>
            {
                draw = request.draw,
                recordsTotal = pagedResult.TotalRecords,
                recordsFiltered = pagedResult.TotalRecords,
                data = (pagedResult.Data).ToList()
            };

            return Ok(response);
        }







        /// <summary>
        /// Creates a new roadmap.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPADD")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/roadmap-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // 3. Find the "roadmap" part
            var roadmapContent = provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name?.Trim('"') == "roadmap");

            if (roadmapContent == null)
                return BadRequest("Missing 'roadmap' part in multipart content.");

            // 4. Read the JSON string
            var roadmapJson = await roadmapContent.ReadAsStringAsync();

            // 5. Deserialize to your DTO
            dtoRoadmapExtended roadmap;
            dtoStructRoadmapRoot structureRoadmap;
            try
            {
                roadmap = JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
                structureRoadmap = JsonConvert.DeserializeObject<dtoStructRoadmapRoot>(roadmap.RoadmapJson);

                Pulse.Api.Tools.Roadmap.Map(structureRoadmap, roadmap.RoadmapSysId, User.Identity.GetClaim("employeeid"), ref roadmap);
                roadmap.RoadmapSysId = await _roadmapService.BuildRoadmapAsync(Mapper.Map<Roadmap>(roadmap), roadmap.CreatedBy);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid roadmap json: " + ex.Message);
            }
            var _roadmap = Mapper.Map<Roadmap>(roadmap);

            return Created($"api/roadmaps/", roadmap);
        }

        /////// <summary>
        /////// Updates an existing roadmap.
        /////// </summary>
        ////[HttpPut]
        ////[Authorize]
        ////[Route("{code}")]
        ////[RequireRoadmapExistsAttribute]
        ////public async Task<IHttpActionResult> Update(string code)
        ////{
        ////    if (!Request.Content.IsMimeMultipartContent())
        ////        return BadRequest("Unsupported media type.");

        ////    var provider = new MultipartMemoryStreamProvider();
        ////    await Request.Content.ReadAsMultipartAsync(provider);

        ////    // Get the plant data (assuming the input name is 'plant')
        ////    var roadmapContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "roadmap");
        ////    dtoRoadmapExtended dtoroadmapextended = null;
        ////    if (roadmapContent != null)
        ////    {
        ////        var roadmapJson = await roadmapContent.ReadAsStringAsync();
        ////        dtoroadmapextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
        ////    }

        ////    if (dtoroadmapextended == null)
        ////        return BadRequest("Plant data is missing.");
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    var roadmap = Mapper.Map<Roadmap>(dtoroadmapextended);

        ////    roadmap.Fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RoadmapField>>(dtoroadmapextended.RoadmapJson).Select(Mapper.Map<RoadmapField>).ToList();
        ////    roadmap.ModifiedBy = User.Identity.GetClaim("employeeid");

        ////    await _roadmapService.RebuildRoadmapAsync(roadmap, dtoroadmapextended.TransactionKey, roadmap.ModifiedBy);

        ////    return StatusCode(System.Net.HttpStatusCode.NoContent);
        ////}

        /// <summary>
        /// Updates an basic info of roadmap.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        [RequireRoadmapExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var roadmapContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "roadmap");
            dtoRoadmapExtended dtoroadmapextended = null;
            if (roadmapContent != null)
            {
                var roadmapJson = await roadmapContent.ReadAsStringAsync();
                dtoroadmapextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
            }

            if (dtoroadmapextended == null)
                return BadRequest("Roadmap data is missing.");

            if (code != dtoroadmapextended.RoadmapSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadmap = Mapper.Map<Roadmap>(dtoroadmapextended);

            roadmap.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _roadmapService.UpdateAsync(roadmap, dtoroadmapextended.TransactionKey, roadmap.ModifiedBy);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Updates an existing roadmap.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("UpdateDetails/{code}")]
        [RequireRoadmapExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPEDIT")]
        public async Task<IHttpActionResult> UpdateDetails(string code)
        {
 
            // Check if the request contains multipart/roadmap-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // 3. Find the "roadmap" part
            var roadmapContent = provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name?.Trim('"') == "roadmap");

            if (roadmapContent == null)
                return BadRequest("Missing 'roadmap' part in multipart content.");

            // 4. Read the JSON string
            var roadmapJson = await roadmapContent.ReadAsStringAsync();

            // 5. Deserialize to your DTO
            dtoRoadmapExtended roadmap;
            dtoStructRoadmapRoot structureRoadmap;
            try
            {
                roadmap = JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
                structureRoadmap = JsonConvert.DeserializeObject<dtoStructRoadmapRoot>(roadmap.RoadmapJson);

                Pulse.Api.Tools.Roadmap.Map(structureRoadmap, roadmap.RoadmapSysId, User.Identity.GetClaim("employeeid"), ref roadmap);

                await _roadmapService.RebuildRoadmapAsync(Mapper.Map<Roadmap>(roadmap), roadmap.TransactionKey, roadmap.ModifiedBy);
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid roadmap json: " + ex.Message);
            }

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }


        /// <summary>
        /// Updates an basic info of roadmap.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("ChangeStatus/{code}")]
        [RequireRoadmapExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPEDIT")]
        public async Task<IHttpActionResult> ChangeStatus(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var roadmapContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "roadmap");
            dtoRoadmapExtended dtoroadmapextended = null;
            if (roadmapContent != null)
            {
                var roadmapJson = await roadmapContent.ReadAsStringAsync();
                dtoroadmapextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
            }

            if (dtoroadmapextended == null)
                return BadRequest("Roadmap data is missing.");

            if (code != dtoroadmapextended.RoadmapSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadmap = Mapper.Map<Roadmap>(dtoroadmapextended);

            roadmap.ModifiedBy = User.Identity.GetClaim("employeeid");

            await _roadmapService.ChangeStatusAsync(roadmap, dtoroadmapextended.TransactionKey, roadmap.ModifiedBy);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }



        /// <summary>
        /// Deletes a roadmap by Code.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("{code}")]
        [RequireRoadmapExistsAttribute]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "RMAPDEL")]
        public async Task<IHttpActionResult> Delete(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Get the plant data (assuming the input name is 'plant')
            var roadmapContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "roadmap");
            dtoRoadmapExtended dtoroadmapextended = null;
            if (roadmapContent != null)
            {
                var roadmapJson = await roadmapContent.ReadAsStringAsync();
                dtoroadmapextended = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoRoadmapExtended>(roadmapJson);
            }

            if (dtoroadmapextended == null)
                return BadRequest("Roadmap data is missing.");

            if (code != dtoroadmapextended.RoadmapSysId)
                return BadRequest("Invalid request.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roadmap = await _roadmapService.GetRoadmapByIdAsync(code);

            if (roadmap == null)
                return NotFound();

            await _roadmapService.DeleteRoadmapAsync(code, User.Identity.GetClaim("employeeid"));
            return Ok();
        }
    }
}
