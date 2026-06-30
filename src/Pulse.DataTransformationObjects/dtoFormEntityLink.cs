using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoFormEntityLink
    {
        public string FormEntityLinkSysId { get; set; }
        public string FormSysId { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
        public int OrderIndex { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public dtoForm Form { get; set; }
        
    }
}
