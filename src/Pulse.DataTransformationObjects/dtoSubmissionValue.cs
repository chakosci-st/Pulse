using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoSubmissionValue
    {
        [JsonProperty("submissionValueSysId")]
        public string SubmissionValueSysId { get; set; }
        [JsonProperty("formFieldSysId")]
        public string FormFieldSysId { get; set; }
        [JsonProperty("entitySysId")]
        public string EntitySysId { get; set; }
        [JsonProperty("entityType")]
        public string EntityType { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("formSysId")]
        public string FormSysId { get; set; }
        [JsonProperty("formEntityLinkSysId")]
        public string FormEntityLinkSysId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("transactionKey")]
        public string TransactionKey { get; set; }
    }
}
