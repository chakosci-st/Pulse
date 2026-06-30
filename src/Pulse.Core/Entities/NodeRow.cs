using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class NodeRow
    {
        public string NodeId { get; set; }
        public string NodeKey { get; set; }
        public string RoadmapSysId { get; set; }
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string DataMaturityCode { get; set; }
        public string DataName { get; set; }
        public string DataDescription { get; set; }
        public decimal? DataMandays { get; set; }
        public int? DataIsRequired { get; set; }
        public int OrderIndex { get; set; }
        public int? IsActive { get; set; }
        public string NodeType { get; set; }
        public string TransactionKey { get; set; }
    }
}
