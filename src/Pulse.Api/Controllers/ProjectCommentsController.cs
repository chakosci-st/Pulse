using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
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
    [RoutePrefix("api/comments")]
    public class ProjectCommentsController : ApiController
    {
        private readonly IProjectCommentService _projectcommentService;

        public ProjectCommentsController(IProjectCommentService projectcommentService)
        {
            _projectcommentService = projectcommentService;
        }

        [HttpPost]
        [Route("add")]
        public async Task<IHttpActionResult> Add([FromBody] CommentSubmit model)
        {
            var hasPlainComment = !string.IsNullOrWhiteSpace(model?.Comments);
            var hasRichComment = !string.IsNullOrWhiteSpace(model?.CommentsRichText);

            if (model == null || (!hasPlainComment && !hasRichComment))
            {
                return Ok(new
                {
                    Success = false,
                    Error = "Comment text is required."
                });
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var loggeduser = User.Identity.GetClaim("employeeid");
            var loggeduserFirstName = User.Identity.GetClaim("firstname");
            var loggeduserLastName = User.Identity.GetClaim("lastname");

            var obj = new ProjectComment
            {
                Comments = hasPlainComment
                    ? model.Comments.Trim()
                    : StripHtmlTags(model.CommentsRichText),
                CommentsRichText = hasRichComment ? model.CommentsRichText : null,
                CreatedBy = loggeduser,
                ProjectNo = model.ProjectNo,
                EntitySysId = model.EntitySysId,
                EntityType = model.EntityType,
                CreatedDate = DateTime.Now,
                CreatedByMeta = new User
                {
                    FirstName = loggeduserFirstName,
                    LastName = loggeduserLastName
                }
            };

            var sysId = await _projectcommentService.AddAsync(obj);
            obj.CommentSysId = sysId;

            return Ok(new
            {
                success = true,
                data = obj
            });
        }

        [Route("{projectno}")]
        public async Task<IHttpActionResult> GetPerProject(string projectno)
        {

            var data = await _projectcommentService.GetByProjectAsync(projectno);

            return Ok(new { data = data });
        }

        [Route("{projectno}/{entitytype}/{entitysysid}")]
        public async Task<IHttpActionResult> GetPerEntity(string projectno, string entitytype, string entitysysid)
        {

            var data = await _projectcommentService.GetByEntityAsync(projectno, entitytype, entitysysid);

            return Ok(new { data = data });
        }

        private static string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var noTags = System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(noTags).Trim();
        }
    }
}
