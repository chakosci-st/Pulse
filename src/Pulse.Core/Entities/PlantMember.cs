using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class PlantMember
    {
        public string PlantMemberSysId { get; set; }
        public string PlantCode { get; set; }
        public string UserId { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public Plant PlantInfo { get; set; }

        public User UserInfo { get; set; }
    }
}
