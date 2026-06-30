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
    [RoutePrefix("api/activities")]
    public class ActivitiesController : ApiController
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// Gets all activities.
        /// </summary>
        [HttpGet]
        [Route("{search}")]
        [Route("")]
        public async Task<IHttpActionResult> Search(string search)
        {
            var activities = await _activityService.GetByKeywordAsync(search);
            return Ok(activities);
        }

    }
}
