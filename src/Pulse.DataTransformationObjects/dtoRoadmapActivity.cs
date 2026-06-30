using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoRoadmapActivity
    {
        public dtoRoadmapActivity()
        {
            Prerequisites = new HashSet<dtoRoadmapActivityPrerequisite>();
            SubActivities = new HashSet<dtoRoadmapActivity>();
            Milestones = new HashSet<dtoRoadmapMilestone>();
            Forms = new HashSet<dtoFormEntityLink>();
        }

        public string RoadmapActivitySysId { get; set; }
        public string RoadmapSysId { get; set; } 
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDescription { get; set; } 
        public int EstimatedManDays { get; set; }
        public int OrderIndex { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public ICollection<dtoRoadmapActivity> SubActivities { get; set; }
        public ICollection<dtoRoadmapMilestone> Milestones { get; set; }
        public ICollection<dtoFormEntityLink> Forms { get; set; }
        public ICollection<dtoRoadmapActivityPrerequisite> Prerequisites { get; set; }
    }
}
