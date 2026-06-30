using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class PrereqRow
    {
        public string PrereqLinkKey { get; set; }
        public string NodeKey { get; set; }
        public string PrereqKey { get; set; } 

        public string RoadmapActivitySysId { get; set; }
    }
}
