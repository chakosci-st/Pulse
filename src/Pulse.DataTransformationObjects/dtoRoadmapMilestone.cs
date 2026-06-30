using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoRoadmapMilestone
    {
        public dtoRoadmapMilestone()
        {
            SubMilestones = new HashSet<dtoRoadmapMilestone>();
            Activities = new HashSet<dtoRoadmapActivity>();
            Forms = new HashSet<dtoFormEntityLink>();
        }
        public string RoadmapMilestoneSysId { get; set; }
        public string RoadmapSysId { get; set; }
        public string MaturityCode { get; set; }
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string MilestoneAlias { get; set; }
        public string MilestoneDescription { get; set; }
        public int OrderIndex { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }

        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public dtoRoadmapMilestone ParentMilestone { get; set; }
        public ICollection<dtoRoadmapMilestone> SubMilestones { get; set; }
        public ICollection<dtoRoadmapActivity> Activities { get; set; }
        public ICollection<dtoFormEntityLink> Forms { get; set; }
        
    }
}
