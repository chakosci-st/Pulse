using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class NodeFormRow
    {
        public string FormNodeKey { get; set; }
        public string FormNodeId { get; set; }
        public string SysId { get; set; }
        public string FormName { get; set; }
        public string FormDescription { get; set; }
        public string NodeKey { get; set; }
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        
    }
}
