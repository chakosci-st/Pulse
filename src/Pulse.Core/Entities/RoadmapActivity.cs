using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class RoadmapActivity
    {
        public RoadmapActivity()
        {
            Prerequisites = new HashSet<RoadmapActivityPrerequisite>();
            SubActivities = new HashSet<RoadmapActivity>();
            Milestones = new HashSet<RoadmapMilestone>();
            Forms = new HashSet<FormEntityLink>();
        }

        public string RoadmapActivitySysId { get; set; }
        public string RoadmapSysId { get; set; } 
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDescription { get; set; } 
        public int EstimatedManDays { get; set; } 
        public int IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public ICollection<RoadmapMilestone> Milestones { get; set; }
        public ICollection<RoadmapActivity> SubActivities { get; set; }
        public ICollection<RoadmapActivityPrerequisite> Prerequisites { get; set; }
        public ICollection<FormEntityLink> Forms { get; set; }
    }
}
