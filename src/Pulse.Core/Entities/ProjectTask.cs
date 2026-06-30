using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectTask
    {
        public ProjectTask()
        {
            Owners = new HashSet<ProjectOwner>();
            SubTasks = new HashSet<ProjectTask>();
            Milestones = new HashSet<ProjectMilestone>();
        }
        public string ProjectTaskSysId { get; set; }
        public string ProjectNo { get; set; }
        public string RoadmapActivitySysId { get; set; }
        public string PlantRoadmapLinkSysId { get; set; }
        public string RoadmapSysId { get; set; }
        
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public string AltTaskName { get; set; }
        public string AltTaskDescription { get; set; }

        public int EstimatedMandays { get; set; }
        public int? TargetStartYear { get; set; }
        [StringLength(2)]
        public string TargetStartWorkWeek { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public string TargetStartedBy { get; set; }
        public int? TargetCompletionYear { get; set; }
        [StringLength(2)]
        public string TargetCompletionWorkWeek { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public string TargetCompletedBy { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public string ActualStartedBy { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string ActualCompletedBy { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public int IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public ICollection<ProjectOwner> Owners { get; set; }
        public ICollection<ProjectTask> SubTasks { get; set; }
        public ICollection<ProjectMilestone> Milestones { get; set; }
    }
}
