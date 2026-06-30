using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoForm 
    {
        public dtoForm()
        {
            Fields = new HashSet<dtoFormField>();
            EntityLinks = new HashSet<dtoFormEntityLink>();
        }

        [Key]
 
        public string FormSysId { get; set; }
        public string FormName { get; set; }
        public string FormDescription { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

 
        public ICollection<dtoFormField> Fields { get; set; }
        public ICollection<dtoFormEntityLink> EntityLinks { get; set; }


    }
}
