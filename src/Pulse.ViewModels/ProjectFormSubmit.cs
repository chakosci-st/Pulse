using Newtonsoft.Json;
using Pulse.DataTransformationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.ViewModels
{
    public class ProjectFormSubmit
    {
        [JsonProperty("projectNo")]
        public string ProjectNo { get; set; }
        [JsonProperty("submissionSysId")]
        public string SubmissionSysId { get; set; }
        [JsonProperty("transactionKey")]
        public string TransactionKey { get; set; }
        [JsonProperty("formSysId")]
        public string FormSysId { get; set; }
        [JsonProperty("formEntityLinkSysId")]
        public string FormEntityLinkSysId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        [JsonProperty("fields")]
        public List<dtoSubmissionValue> Fields { get; set; }
    }
}
