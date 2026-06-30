using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoRoadmapExtended : dtoRoadmap
    {
        public string CategoryName { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string RoadmapJson { get; set; }
    }
}
