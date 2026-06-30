using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class PlantRoadmapLinkExtended : PlantRoadmapLink
    {
        public int IsSelected { get; set; }
        public int RMIsActive { get; set; }
        public string RoadmapJson { get; set; }
    }
}
