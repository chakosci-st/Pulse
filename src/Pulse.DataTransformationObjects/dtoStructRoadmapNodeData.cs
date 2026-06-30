using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoStructRoadmapNodeData
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public bool IsRequired { get; set; }
        public string Maturity { get; set; }    // used by "milestone"
        public string Mandays { get; set; }     // used by "activity"

    }
}
