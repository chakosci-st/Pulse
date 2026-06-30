using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a milestone of a project
/// </summary>
namespace Pulse.Core.Entities
{
  public  class ProjectMilestone : BaseEntity<string>
    {
        public ProjectMilestone()
        {
            Owners = new HashSet<ProjectOwner>();
            Tasks = new HashSet<ProjectTask>();
            StatusChanges = new HashSet<StatusChange>();
            TargetRevisions = new HashSet<TargetRevision>();

        }
        [Key]
        public string MilestoneSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string PlantRoadmapLinkSysId { get; set; }
        public string RoadmapSysId { get; set; }
        public string RoadmapMilestoneSysId { get; set; }  
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
        public int IsActive { get; set; }
        public int IsRequired { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
         
        public ICollection<ProjectOwner> Owners { get; set; }
        public ICollection<ProjectTask> Tasks { get; set; }
        public ICollection<StatusChange> StatusChanges { get; set; }
        public ICollection<TargetRevision> TargetRevisions { get; set; }
    }
}
