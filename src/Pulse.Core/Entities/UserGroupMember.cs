using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a user group in the system.
/// </summary>
namespace Pulse.Core.Entities
{
    public class UserGroupMember : BaseEntity<string>
    {
        [Key]
        public string UserGroupMemberSysId { get; set; }
        public int? UserGroupId { get; set; } 
        public string UserId { get; set; }

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
