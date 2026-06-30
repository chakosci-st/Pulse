using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/annotationtypes")]
    public class AnnotationTypesController : ApiController
    {
        private readonly IAnnotationTypeService _annotationtypeService;
        public AnnotationTypesController(IAnnotationTypeService annotationtypeService)
        {
            _annotationtypeService = annotationtypeService;
        }

        /// <summary>
        /// Gets all annotation types.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var plants = await _annotationtypeService.GetAllAnnotationTypesAsync();
            return Ok(plants);
        }

        /// <summary>
        /// Gets a annotation type by ID.
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var plant = await _annotationtypeService.GetAnnotationTypeByIdAsync(id);
            if (plant == null)
                return NotFound();

            return Ok(plant);
        }

        /// <summary>
        /// Creates a new annotation type.
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(AnnotationType annotationtype)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

          await  _annotationtypeService.AddAnnotationTypeAsync(annotationtype);
            return Created($"api/annotationtypes/{annotationtype.AnnotationTypeId}", annotationtype);
        }

        /// <summary>
        /// Updates an existing annotation type.
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Update(int id, AnnotationType annotationtype)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != annotationtype.AnnotationTypeId)
                return BadRequest("Id mismatch.");

          await  _annotationtypeService.UpdateAnnotationTypeAsync(annotationtype);
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a annotation type by Id.
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {

            var plant = _annotationtypeService.GetAnnotationTypeByIdAsync(id);
            if (plant == null)
                return NotFound();

            await _annotationtypeService.DeleteAnnotationTypeAsync(id, "");
            return Ok();
        }
    }
}
