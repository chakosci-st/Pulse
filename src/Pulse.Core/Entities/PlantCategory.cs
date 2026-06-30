using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a category in plant.
/// </summary>
namespace Pulse.Core.Entities
{
    public class PlantCategory : BaseEntity<string>
    {
        public PlantCategory()
        {
            Milestones = new HashSet<PlantCategoryMilestone>();
        }
        [Key]
        public string PlantCategorySysId { get; set; }
        [Required]
        public string CategoryCode { get; set; }
        [Required]
        public string PlantCode { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public Category Category { get; set; }
        public Plant Plant { get; set; }

        public IEnumerable<PlantCategoryMilestone> Milestones { get; set; }
    }
}
