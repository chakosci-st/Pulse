using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a reference if task is a prerequisite of another task.
/// </summary>
namespace Pulse.Core.Entities
{
    public class WorkItemMember : BaseEntity<string>
    {
        [Key]
        public string WorkItemMemberSysId { get; set; }
        [Required]
        public string WorkItemSysId { get; set; }
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
    }
}
