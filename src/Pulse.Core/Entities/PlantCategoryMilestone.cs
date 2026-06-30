using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a milestone per Plant + Category. Will be used as structure
/// </summary>
namespace Pulse.Core.Entities
{
    public class PlantCategoryMilestone : BaseEntity<string>
    {
        [Key]
        public string PlantCategoryMilestoneSysId { get; set; }
        [Required]
        public string CategoryCode { get; set; }
        [Required]
        public string PlantCode { get; set; }
        [Required]
        public string MaturityCode { get; set; }
        public string ParentSysId { get; set; }
        [StringLength(40)]
        public string Alias { get; set; } 
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }

        public Category Category { get; set; }
        public Plant Plant { get; set; }
        public MaturityLevel Maturity { get; set; }
    }
}
