using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoTreeResponse
    {
        public List<dtoNode> TreeData { get; set; } = new List<dtoNode>();
        public List<dtoNodeForm> RootForms { get; set; } = new List<dtoNodeForm>();
    }
}
