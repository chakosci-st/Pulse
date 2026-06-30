using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pulse.Api.Models
{
    public class TargetRevision
    {
        public string ProjectNo { get; set; }
        public string ProjectNodeSysId { get; set; }
        public string NodeType { get; set; }
        public string NodeId { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public string Remarks { get; set; }
    }
}