using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pulse.Api.Models;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using Pulse.SharedUtilities.Extensions;
using Pulse.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/ProjectForms")]
    public class ProjectFormsController : ApiController
    {
        private readonly IProjectFormService _projectformService;

        public ProjectFormsController(IProjectFormService projectformService)
        {
            _projectformService = projectformService;
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> SubmitProjectForm()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var _modelObject = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "formfields");
            ProjectFormSubmit model = null;
            if (_modelObject != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };



                try
                {
                    var modelJson = await _modelObject.ReadAsStringAsync();
                    model = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectFormSubmit>(modelJson);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }

            }



            if (!ModelState.IsValid)
                return BadRequest(ModelState);










            if (model == null)
                return BadRequest("Request body is empty.");


            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                var obj = Mapper.Map<ProjectFormSubmission>(model);

                obj.CreatedBy = loggeduser;
                obj.ModifiedBy = loggeduser;

                var submissionsysid = Guid.NewGuid().ToString();
                obj.SubmissionSysId = submissionsysid;

                await _projectformService.SubmitFormAsync(obj, loggeduser);


                // raise event that Task status was changed


                // Return 201 Created + some result
                return Content(HttpStatusCode.Created, new
                {

                    message = "Form successfully submitted."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{code}")]
        public async Task<IHttpActionResult> UpdateProjectForm(string code)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var _modelObject = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "formfields");
            ProjectFormSubmit model = null;
            if (_modelObject != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };



                try
                {
                    var modelJson = await _modelObject.ReadAsStringAsync();
                    model = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectFormSubmit>(modelJson);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }

            }
 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
 
            if (model == null)
                return BadRequest("Request body is empty.");


            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {

                var loggeduser = User.Identity.GetClaim("employeeid");
                var obj = Mapper.Map<ProjectFormSubmission>(model);

                obj.CreatedBy = loggeduser;
                obj.ModifiedBy = loggeduser;

 

                await _projectformService.UpdateFormAsync (obj, loggeduser);


                // raise event that Task status was changed


                // Return 201 Created + some result
                return Content(HttpStatusCode.Created, new
                {

                    message = "Form successfully submitted."
                });
            }
            catch (Exception ex)
            {
                // Log exception
                // _logger.Error(ex);

                return InternalServerError(ex);
            }
        }


        [Route("submissions/{code}")] 
        public async Task<IHttpActionResult> GetProjectsFormSubmissions(string code)
        {

            var pagedResult = await _projectformService.GetFormValuesBySubmissionAsync(code);

            // Prepare DataTables response
            var response = new DataTablesResponse<dtoFormSubmissionValue>
            {
                data = pagedResult.Select(Mapper.Map<dtoFormSubmissionValue>).ToList()
            };

            return Ok(response);
        }

        [Route("submissions/value/{code}")]
        public async Task<IHttpActionResult> GetProjectsFormSubmissionValue(string code)
        {

            var data = await _projectformService.GetFormValueAsync(code);
 
            return Ok(data);
        }

    }
}
