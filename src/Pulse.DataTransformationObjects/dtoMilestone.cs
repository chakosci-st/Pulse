using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Pulse.DataTransformationObjects
{
    public class dtoMilestone
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("owners")]
        public List<string> Owners { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }    // year as string
        [JsonProperty("endDate")]
        public string EndDate { get; set; }      // year as string
        [JsonProperty("startWeek")]
        public string StartWeek { get; set; }    // "01".."52"
        [JsonProperty("endWeek")]
        public string EndWeek { get; set; }      // "01".."52"
        [JsonProperty("tasks")]
        public List<dtoTask> Tasks { get; set; }
        [JsonProperty("meta")]
        public dtoMilestoneMeta Meta { get; set; }
    }
}
