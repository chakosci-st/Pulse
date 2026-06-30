using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class FormFieldRule
    {
        [JsonProperty("id")]
        public string FormFieldRuleSysId { get; set; }
        public string FormFieldSysId { get; set; }
        [JsonProperty("field")]
        public string RuleField { get; set; }

        [JsonProperty("operator")]
        public string RuleOperator { get; set; }

        [JsonProperty("value")]
        public string RuleValue { get; set; }

        [JsonProperty("action")]
        public string RuleAction { get; set; }

        [JsonProperty("actionValue")]
        public string RuleActionValue { get; set; } 
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }

}
