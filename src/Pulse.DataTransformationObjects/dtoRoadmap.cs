using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoRoadmap
    {
        public dtoRoadmap()
        {
            Forms = new HashSet<dtoFormEntityLink>();
            Milestones = new HashSet<dtoRoadmapMilestone>();
            Activities = new HashSet<dtoRoadmapActivity>();
        }

        public string RoadmapSysId { get; set; }
        public string RoadmapName { get; set; }
        public string RoadmapDescription { get; set; }
        public string CategoryCode { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public ICollection<dtoFormEntityLink> Forms { get; set; }
        public ICollection<dtoRoadmapMilestone> Milestones { get; set; }
        public ICollection<dtoRoadmapActivity> Activities { get; set; }
    }
}
