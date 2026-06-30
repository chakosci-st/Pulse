using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoProductionCalendarWithStats : dtoProductionCalendar
    {
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
    }
}
