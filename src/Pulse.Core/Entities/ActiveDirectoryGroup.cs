using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
   public class ActiveDirectoryGroup : BaseEntity<string>
    {
        public ActiveDirectoryGroup()
        { 
            Members = new HashSet<ActiveDirectoryGroupMember>();
        }

        [Key]
        public string ADGroup { get; set; }
        public string Email { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public AnnotationType AnnotationType { get; set; }
        public ICollection<ActiveDirectoryGroupMember> Members { get; set; }
    }
}
