using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoFormFieldRule
    {
        public string FormFieldRuleSysId { get; set; }
        public string FormFieldSysId { get; set; } 
        public string RuleField { get; set; } 
        public string RuleOperator { get; set; } 
        public string RuleValue { get; set; } 
        public string RuleAction { get; set; } 
        public string RuleActionValue { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }

}
