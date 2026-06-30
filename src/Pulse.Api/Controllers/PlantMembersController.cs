using AutoMapper;
using Pulse.Api.Filters;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
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
    [RoutePrefix("api/plantmembers")]
    public class PlantMembersController : ApiController
    {
        private readonly IEventPublisher _eventBus;
        private readonly IUserService _userService;
        private readonly IPlantService _plantService;

        public PlantMembersController(IEventPublisher eventBus, IUserService userService, IPlantService plantService)
        {
            _plantService = plantService;
            _eventBus = eventBus;
            _userService = userService;
        }


        /// <summary>
        /// Gets all plants.
        /// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTVIEW")]
        [Route("PerPlant")]
        public async Task<IHttpActionResult> GetMembersPerPlant(string code)
        {
            var members = await _plantService.GetMembersByCode(code.ToUpper());
            return Ok(members);
        }

        /// <summary>
        /// Gets all members to a datatables format.
        /// </summary>
        [HttpGet]
        [Authorize]
      //  [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTVIEW")]
        [Route("PerPlantDatatables")]
        public async Task<IHttpActionResult> GetMembersPerPlantDatatables(string code)
        {
            var members = (await _plantService.GetMembersByCode(code.ToUpper())).ToList();

            var response = new DataTablesResponse<PlantMember>
            {
                draw = 1,
                recordsTotal = members.Count(),
                recordsFiltered = members.Count(),
                data = members
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a new plant member.
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

            // Get the plant data (assuming the input name is 'plant')
            var plantmemberContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plantmember");
            PlantMember plantMember = null;
            if (plantmemberContent != null)
            {
                var plantMemberJson = await plantmemberContent.ReadAsStringAsync();
                plantMember = Newtonsoft.Json.JsonConvert.DeserializeObject<PlantMember>(plantMemberJson);
            }

            if (plantMember == null)
                return BadRequest("Member data is missing.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            plantMember.CreatedBy = User.Identity.GetClaim("employeeid");
            plantMember.CreatedDate = DateTime.UtcNow;

            var key = await _plantService.AddMemberAsync(Mapper.Map<PlantMember>(plantMember));

            var createdby = await _userService.GetUserByIdAsync(plantMember.CreatedBy);

            // raise event that Task status was changed
            await _eventBus.Publish(new PlantMemberRegisteredEventArgs(plantMember.PlantCode, plantMember.PlantInfo.PlantName, plantMember.UserInfo.FirstName + " " + plantMember.UserInfo.LastName, plantMember.UserInfo.Email, createdby.FirstName + " " + createdby.LastName, createdby.Email, plantMember.CreatedDate));

            return Created($"api/plantmembers/{plantMember.PlantCode}", plantMember);
        }

        /// <summary>
        /// Updates an existing plant member.
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

            var plantContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plantmember");
            PlantMember plant = null;
            if (plantContent != null)
            {
                var plantJson = await plantContent.ReadAsStringAsync();
                plant = Newtonsoft.Json.JsonConvert.DeserializeObject<PlantMember>(plantJson);
            }

            if (plant == null)
                return BadRequest("Member data is missing.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (code != plant.PlantMemberSysId)
                return BadRequest("Plant - Member Code mismatch.");

            plant.ModifiedBy = User.Identity.GetClaim("employeeid");

            // Pass newFileName to your service
            await _plantService.UpdateMemberAsync(Mapper.Map<PlantMember>(plant));


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }


        /// <summary>
        /// Updates an existing plant member.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}/activate")]
        [RequirePlantCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        public async Task<IHttpActionResult> Activate(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var plantContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plantmember");
            PlantMember plant = null;
            if (plantContent != null)
            {
                var plantJson = await plantContent.ReadAsStringAsync();
                plant = Newtonsoft.Json.JsonConvert.DeserializeObject<PlantMember>(plantJson);
            }

            if (plant == null)
                return BadRequest("Member data is missing.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (code != plant.PlantMemberSysId)
                return BadRequest("Plant - Member Code mismatch.");

            plant.ModifiedBy = User.Identity.GetClaim("employeeid");

            var obj = Mapper.Map<PlantMember>(plant);
            obj.IsActive = 1;


            // Pass newFileName to your service
            await _plantService.UpdateMemberAsync(obj);


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Updates an existing plant member.
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("{code}/deactivate")]
        [RequirePlantCodeExists]
        [Pulse.Api.Filters.AuthorizeUserGroup(Modules = "PLANTEDIT")]
        public async Task<IHttpActionResult> Deactivate(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var plantContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "plantmember");
            PlantMember plant = null;
            if (plantContent != null)
            {
                var plantJson = await plantContent.ReadAsStringAsync();
                plant = Newtonsoft.Json.JsonConvert.DeserializeObject<PlantMember>(plantJson);
            }

            if (plant == null)
                return BadRequest("Member data is missing.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (code != plant.PlantMemberSysId)
                return BadRequest("Plant - Member Code mismatch.");

            plant.ModifiedBy = User.Identity.GetClaim("employeeid");

            var obj = Mapper.Map<PlantMember>(plant);
            obj.IsActive = 0;


            // Pass newFileName to your service
            await _plantService.UpdateMemberAsync(obj);


            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}
