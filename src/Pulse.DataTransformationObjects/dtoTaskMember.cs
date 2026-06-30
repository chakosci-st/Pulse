using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a task in a project/milestone.
/// </summary>
namespace Pulse.DataTransformationObjects
{
    public class dtoTaskMember 
    {
 
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

        public dtoUser User { get; set; }
        public dtoUserGroup UserGroup { get; set; }
    }
}
