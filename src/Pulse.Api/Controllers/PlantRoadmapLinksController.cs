using AutoMapper;
using Newtonsoft.Json;
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
    [RoutePrefix("api/plantroadmaplinks")]
    public class PlantRoadmapLinksController : ApiController
    {
        private readonly IRoadmapService _roadmapService;
        private readonly IPlantService _plantService;

        public PlantRoadmapLinksController(IRoadmapService roadmapService, IPlantService plantService)
        {
            _roadmapService = roadmapService;
            _plantService = plantService;
        }

        private async Task<bool> HasPlantAccessAsync(string loggedUser, string plantCode)
        {
            if (string.IsNullOrWhiteSpace(loggedUser) || string.IsNullOrWhiteSpace(plantCode))
            {
                return false;
            }

            var allowedPlants = await _plantService.GetAllPlantsByUserAsync(loggedUser);
            return (allowedPlants ?? Enumerable.Empty<Plant>())
                .Any(p => string.Equals(p.PlantCode, plantCode, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all roadmaps.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("{code:string}")]
        [Route("")]
        public async Task<IHttpActionResult> GetAllByPlantLink(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Ok(new DataTablesResponse<PlantRoadmapLinkExtended>
                {
                    draw = 1,
                    data = new List<PlantRoadmapLinkExtended>()
                });
            }

            var loggedUser = User?.Identity?.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(loggedUser))
            {
                return Unauthorized();
            }

            var hasPlantAccess = await HasPlantAccessAsync(loggedUser, code);

            if (!hasPlantAccess)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Forbidden,
                    "You are not authorized to view templates for this site."));
            }

            var roadmaps = (await _plantService.GetRoadmapListAsync(code)).ToList();

            var response = new DataTablesResponse<PlantRoadmapLinkExtended>
            {
                draw = 1,
                data = roadmaps
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        [Route("{plantCode:string}/roadmaps/{roadmapSysId:string}/treemap")]
        public async Task<IHttpActionResult> GetTreeMapByPlantAndRoadmap(string plantCode, string roadmapSysId)
        {
            if (string.IsNullOrWhiteSpace(plantCode) || string.IsNullOrWhiteSpace(roadmapSysId))
            {
                return BadRequest("Plant code and roadmap id are required.");
            }

            var loggedUser = User?.Identity?.GetClaim("employeeid");
            if (string.IsNullOrWhiteSpace(loggedUser))
            {
                return Unauthorized();
            }

            var hasPlantAccess = await HasPlantAccessAsync(loggedUser, plantCode);
            if (!hasPlantAccess)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Forbidden,
                    "You are not authorized to view templates for this site."));
            }

            var linkedRoadmaps = await _plantService.GetRoadmapListAsync(plantCode);
            var isLinkedRoadmap = (linkedRoadmaps ?? Enumerable.Empty<PlantRoadmapLinkExtended>())
                .Any(link => string.Equals(link.RoadmapSysId, roadmapSysId, StringComparison.OrdinalIgnoreCase));

            if (!isLinkedRoadmap)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Forbidden,
                    "The selected template is not linked to this site."));
            }

            var result = await _roadmapService.GetTreeResponseAsync(roadmapSysId);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            return Ok(json);
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        public async Task<IHttpActionResult> Create()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);



            // Get the category data (assuming the input name is 'category')
            var content = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plantroadmaplink");
            dtoPlantRoadmapLink link = null;
            if (content != null)
            {
                var json = await content.ReadAsStringAsync();
                link = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoPlantRoadmapLink>(json);
            }

            if (link == null)
                return BadRequest("Link data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            link.PlantRoadmapLinkSysId = await _plantService.SelectRoadmapAsync(Mapper.Map<PlantRoadmapLink>(link));

            return Created($"api/plantroadmaplinks/{link.PlantRoadmapLinkSysId}", link);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}")]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        public async Task<IHttpActionResult> Update(string code)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
 
            // Get the category data (assuming the input name is 'category')
            var content = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plantroadmaplink");
            dtoPlantRoadmapLink link = null;
            if (content != null)
            {
                var json = await content.ReadAsStringAsync();
                link = Newtonsoft.Json.JsonConvert.DeserializeObject<dtoPlantRoadmapLink>(json);
            }

            if (link == null)
                return BadRequest("Link data is missing.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (link.IsSelected)
                link.PlantRoadmapLinkSysId = await _plantService.SelectRoadmapAsync(Mapper.Map<PlantRoadmapLink>(link));
            else
                await _plantService.UnselectRoadmapAsync(Mapper.Map<PlantRoadmapLink>(link));

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}
