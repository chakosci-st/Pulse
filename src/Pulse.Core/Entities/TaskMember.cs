using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a task in a project/milestone.
/// </summary>
namespace Pulse.Core.Entities
{
    public class TaskMember : BaseEntity<string>
    {
        public TaskMember()
        {


        }
        [Key]
        public string TaskMemberSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string TaskSysId { get; set; }
        public string UserId { get; set; }
        public int? UserGroupId { get; set; }
        public string ADGroup { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public User User { get; set; }
        public UserGroup UserGroup { get; set; }
    }
}
