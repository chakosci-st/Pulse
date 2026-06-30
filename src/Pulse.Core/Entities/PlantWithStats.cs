using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class PlantWithStats : Plant
    {
        public int ActiveProjectsCount { get; set; }
        public int ActiveTasksCount { get; set; }
        public int ProductCount { get; set; }
        public int TaskDueCount { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
    }
}
