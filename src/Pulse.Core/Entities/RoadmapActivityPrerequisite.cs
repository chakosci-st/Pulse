using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class RoadmapActivityPrerequisite
    {
        public string RoadmapActivityPrereqSysId { get; set; }
        public string RoadMapActivitySysId { get; set; }
        public string PrerequisiteSysId { get; set; }
    }

    public class RoadmapActivityPrerequisiteExt : RoadmapActivityPrerequisite
    {
        public string RoadMapSysId { get; set; }
    }
}
