using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a member of a user group.
/// </summary>
namespace Pulse.Core.Entities
{
  public  class PlantUserGroupMember : BaseEntity<string>
    {
        [Key]
        public string PlantUserGroupMemberSysId { get; set; }
        [Required]
        public string PlantCode { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int UserGroupId { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
