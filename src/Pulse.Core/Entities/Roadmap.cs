using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class Roadmap
    {
        public Roadmap() {
            Forms = new HashSet<FormEntityLink>();
            Milestones = new HashSet<RoadmapMilestone>();
            Activities = new HashSet<RoadmapActivity>();
        }

        public string RoadmapSysId { get; set; }
        public string RoadmapName { get; set; }
        public string RoadmapDescription { get; set; }
        public string CategoryCode { get; set; }
        public int IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public Category Category { get; set; }

        public ICollection<FormEntityLink> Forms { get; set; }
        public ICollection<RoadmapMilestone> Milestones { get; set; }
        public ICollection<RoadmapActivity> Activities { get; set; }
    }
}
