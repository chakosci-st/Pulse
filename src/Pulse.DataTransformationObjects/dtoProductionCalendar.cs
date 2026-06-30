using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
 public   class dtoProductionCalendar
    {
        [Key]
        [Required]
        public DateTime FiscalDate { get; set; }
        [Required]
        public string CalendarYear { get; set; }
        [Required]
        public string CalendarQuarter { get; set; }
        [Required]
        public string CalendarMonth { get; set; }
        [Required]
        public string CalendarWorkWeek { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

    }
}
