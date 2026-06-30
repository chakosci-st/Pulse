using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoNode
    {
        public string Key { get; set; }
        public string Id { get; set; }             // "node_<nodeid>"
        public string Type { get; set; }           // "milestone" | "activity"
        public dtoNodeData Data { get; set; } = new dtoNodeData();
        public List<dtoNode> Children { get; set; } = new List<dtoNode>();
        public List<dtoNodeForm> Forms { get; set; } = new List<dtoNodeForm>();
        public List<string> Prerequisites { get; set; } = new List<string>();
        public bool Collapsed { get; set; } = false;
    }
}
