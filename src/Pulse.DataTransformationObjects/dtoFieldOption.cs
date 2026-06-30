using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoFieldOption
    {
        public string FieldOptionSysId { get; set; }
        public string FieldSysId { get; set; }
 
        public string OptionValue { get; set; }

 
        public string OptionLabel { get; set; }
 
        public int OrderIndex { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
