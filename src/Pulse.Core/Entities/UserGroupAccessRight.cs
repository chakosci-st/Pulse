using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents an access right of a user group.
/// </summary>
namespace Pulse.Core.Entities
{
    public class UserGroupAccessRight : BaseEntity<string>
    {
        [Key]
        public string UserGroupAccessRightSysId { get; set; }
        [Required]
        public int UserGroupId { get; set; }
        [Required]
        public string ModuleCode { get; set; }
        public int AllowRead { get; set; }
        public int AllowWrite { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
