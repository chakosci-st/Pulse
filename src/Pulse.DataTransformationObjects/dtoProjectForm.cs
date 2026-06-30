using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
   public class dtoProjectForm
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("sysid")]
        public string SysId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; } 
    }
}
