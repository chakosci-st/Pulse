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
  public  class UserGroup : BaseEntity<int>
    {
        [Key]
        public int UserGroupId { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 4)]
        public string UserGroupName { get; set; }
        [StringLength(200)]
        public string UserGroupDescription { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
