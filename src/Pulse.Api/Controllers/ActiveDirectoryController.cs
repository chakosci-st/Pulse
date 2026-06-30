using AutoMapper;
using Pulse.Core.Interfaces;
using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Pulse.Api.Controllers
{
    [RoutePrefix("api/ActiveDirectory")]
    public class ActiveDirectoryController : ApiController
    {
        private readonly IActiveDirectoryService _activedirectoryService;

        public ActiveDirectoryController(IActiveDirectoryService activedirectoryService)
        {
            // Initialize the AdSearchService with your domain and container
            _activedirectoryService = activedirectoryService;
        }

        [HttpGet]
        [Route("Search/{key}")]
        [Route("Search")]
        public IHttpActionResult Search(string key)
        {
            var users = Enumerable.Empty<dtoUser>();
            try
            {
                users = _activedirectoryService.SearchUsers(key).Select(Mapper.Map<dtoUser>);
                if (users == null)
                {

                    users = Enumerable.Empty<dtoUser>();
                }
            }
            catch
            {
                users = Enumerable.Empty<dtoUser>();
            }


            return Ok(new { data = users });
        }

        ////[HttpGet]
        ////[Route("api/AdSearch/SearchADGroup/{key}")]
        ////[Route("api/AdSearch/SearchADGroup")]
        ////public IHttpActionResult SearchAdGroup(string key)
        ////{
        ////    var adgroup = new ADGroup();
        ////    adgroup = _adSearchService.SearchADGroup(key);
        ////    if (adgroup == null)
        ////    {
        ////        adgroup = new ADGroup();
        ////    }
        ////    return Ok(new { data = adgroup });
        ////}

    }
}
