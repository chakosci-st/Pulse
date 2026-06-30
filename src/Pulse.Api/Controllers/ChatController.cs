using Pulse.Core.Interfaces;
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
    public class ChatController : ApiController
    {
        private readonly IProjectChatService _projectChatService;

        public ChatController(IProjectChatService projectChatService)
        {
            _projectChatService = projectChatService;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Rooms()
        {
            var loggedUser = User?.Identity?.GetClaim("employeeid");
            var empId = User.Identity.Name; // or however you get EmployeeId
            var rooms = await _projectChatService
                .GetRoomsByUserIdAsync(loggedUser);

            return Ok(rooms);
        }
    }
}
