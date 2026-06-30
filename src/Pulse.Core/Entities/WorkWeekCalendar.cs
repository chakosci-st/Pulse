using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class WorkWeekCalendar
    {
        public string WorkWeek { get; set; }
        public DateTime FiscalDateStart { get; set; }
        public DateTime FiscalDateEnd { get; set; }
    }
}
