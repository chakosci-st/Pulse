using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class PlantRoadmapLink
    {
        public string PlantRoadmapLinkSysId { get; set; }
        public string PlantCode { get; set; }
        public string RoadmapSysId { get; set; }
        public int? IsActive { get; set; }
        public Roadmap Roadmap { get; set; }
        public Plant Plant { get; set; }
    }
}
