using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
 public class dtoProductionCalendarCreate
    {
 
        [Required]
        public string CalendarYear { get; set; }
        [Required]
        public DateTime Week1End { get; set; }
        [Range(4, 5)]
        public int JanuaryWorkWeeks { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

    }
}
