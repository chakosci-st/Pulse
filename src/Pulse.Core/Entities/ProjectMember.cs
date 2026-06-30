using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a member of a project.
/// </summary>
namespace Pulse.Core.Entities
{
    public class ProjectMember : BaseEntity<string>
    {
        [Key]
        public string ProjectMemberSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        [Required]
        public string UserId { get; set; }
        public int IsOwner { get; set; }

        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public User User { get; set; }
    }
}
