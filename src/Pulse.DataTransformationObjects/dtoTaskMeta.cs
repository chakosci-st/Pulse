using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
    public class dtoTaskMeta
    {
        public dtoTaskMeta() {
            
        }


        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("maturity")]
        public string Maturity { get; set; }
        [JsonProperty("mandays")]
        public double? Mandays { get; set; }
        [JsonProperty("isRequired")]
        public bool? IsRequired { get; set; }
        [JsonProperty("prerequisites")]
        public List<string> Prerequisites { get; set; }
        [JsonProperty("forms")]
        public List<dtoProjectForm> Forms { get; set; }
        [JsonProperty("collapsed")]
        public bool? Collapsed { get; set; }
    }
}
