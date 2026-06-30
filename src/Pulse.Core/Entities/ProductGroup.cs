using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a Product Group in the system.
/// </summary>
namespace Pulse.Core.Entities
{
    public class ProductGroup : BaseEntity<string>
    {
        [Key]
        [Required]
        [StringLength(10, MinimumLength = 2)]
        public string ProductGroupCode { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 3)]
        public string ProductGroupName { get; set; }
        [StringLength(200)]
        public string ProductGroupDescription { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
