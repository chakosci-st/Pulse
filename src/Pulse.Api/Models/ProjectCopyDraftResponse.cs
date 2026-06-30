using Newtonsoft.Json;
using Pulse.ViewModels;

namespace Pulse.Api.Models
{
    public class ProjectCopyDraftResponse : ProjectInitViewModel
    {
        [JsonProperty("ownerData")]
        public ProjectCopyDraftOwnerData OwnerData { get; set; }
    }

    public class ProjectCopyDraftOwnerData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}