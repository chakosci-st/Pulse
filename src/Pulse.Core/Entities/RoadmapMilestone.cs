using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class RoadmapMilestone
    {
        public RoadmapMilestone() {
            Activities = new HashSet<RoadmapActivity>();
            SubMilestones = new HashSet<RoadmapMilestone>();
            Forms = new HashSet<FormEntityLink>();
        }

        public string RoadmapMilestoneSysId { get; set; }
        public string RoadmapSysId { get; set; }
        public string MaturityCode { get; set; }
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string MilestoneAlias { get; set; }
        public string MilestoneDescription { get; set; }
        public int OrderIndex { get; set; }
        public int IsActive { get; set; }
        public int IsRequired { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; } 
        public RoadmapMilestone ParentMilestone { get; set; }
        public ICollection<RoadmapActivity> Activities { get; set; }
        public ICollection<RoadmapMilestone> SubMilestones { get; set; }
        public ICollection<FormEntityLink> Forms { get; set; }
    }
}
