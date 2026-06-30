using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
  public class dtoPlant
    {
        [Required]
        [StringLength(10, MinimumLength = 4)]
        public string PlantCode { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 4)]
        public string PlantName { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; } 
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; } 
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
