using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
  public  class dtoStructRoadmapTreeNode
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }

        public string ParentType { get; set; }
        public string ParentId { get; set; }

        public dtoStructRoadmapNodeData Data { get; set; }

        public List<dtoStructRoadmapTreeNode> Children { get; set; }
        public List<dtoStructRoadmapForm> Forms { get; set; }
        public List<string> Prerequisites { get; set; }
        public bool Collapsed { get; set; }
    }
}
