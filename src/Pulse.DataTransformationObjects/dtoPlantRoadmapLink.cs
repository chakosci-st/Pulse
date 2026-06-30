using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoPlantRoadmapLink
    {
        public string PlantRoadmapLinkSysId { get; set; }
        public string PlantCode { get; set; }
        public string RoadmapSysId { get; set; }
        public bool IsSelected { get; set; }
    }
}
