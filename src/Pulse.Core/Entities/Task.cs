using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a task in a project/milestone.
/// </summary>
namespace Pulse.Core.Entities
{
    public class Task : BaseEntity<string>
    {
        public Task()
        {
            Prerequisites = new HashSet<TaskPrerequisite>();
            Members = new HashSet<TaskMember>();
            StatusChanges = new HashSet<StatusChange>();
            TargetRevisions = new HashSet<TargetRevision>();
        }
        [Key]
        public string TaskSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string MilestoneSysId { get; set; }
        public string WorkItemSysId { get; set; }
        [Required]
        public string TaskName { get; set; }
        [Required]
        public string TaskType { get; set; }
        [StringLength(200)]
        public string TaskValue { get; set; }
        public string TargetSart { get; set; }
        public string TargetCompletion { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string Status { get; set; }
        [StringLength(200)]
        public string Remarks { get; set; }
        public int IsRequired { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public IEnumerable<TaskPrerequisite> Prerequisites { get; set; }
        public IEnumerable<TaskMember> Members { get; set; }
        public IEnumerable<StatusChange> StatusChanges { get; set; }
        public IEnumerable<TargetRevision> TargetRevisions { get; set; }
    }



}
