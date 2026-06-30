using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoUserGroup
    {
        public int UserGroupId { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 4)]
        public string UserGroupName { get; set; }
        [StringLength(200)]
        public string UserGroupDescription { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}