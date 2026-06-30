using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectTargetRevision
    {
        public string TargetRevisionSysId { get; set; }
        public string ProjectNo { get; set; }
        public string ProjectNodeSysId { get; set; }
        public string EntitySysId { get; set; }
        public string EntityType { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
