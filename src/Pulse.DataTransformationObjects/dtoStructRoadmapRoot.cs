using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoStructRoadmapRoot
    {
        public List<dtoStructRoadmapTreeNode> TreeData { get; set; }
        public List<dtoStructRoadmapForm> RootForms { get; set; }
    }
}
