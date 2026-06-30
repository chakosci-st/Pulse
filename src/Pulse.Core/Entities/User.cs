using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a user in the system.
/// </summary>
namespace Pulse.Core.Entities
{
    public class User : BaseEntity<string>
    {
        public User() {
            PlantUserGroupAssigned = new HashSet<PlantUserGroupMember>();
            Projects = new HashSet<ProjectMember>();
        }

        [Key]
        [Required]
        public string UserId { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 4)]
        public string UserName { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public bool Registered { get; set; } = false;
        public int DashboardShowAllUsers { get; set; }
        public IEnumerable<PlantUserGroupMember> PlantUserGroupAssigned { get; set; }
        public IEnumerable<ProjectMember> Projects { get; set; }
    }
}
