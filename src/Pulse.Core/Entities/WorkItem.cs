using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a reference task for each project or milestone.
/// </summary>
namespace Pulse.Core.Entities
{
    public class WorkItem : BaseEntity<string>
    {
        public WorkItem() {
            Members = new HashSet<WorkItemMember>();
            Prerequisites = new HashSet<WorkItemPrerequisite>();
        }
        [Key]
        public string WorkItemSysId { get; set; }
        [Required]
        public string CategoryCode { get; set; }
        [Required]
        public string PlantCode { get; set; }
        [Required]
        public string MaturityCode { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string TaskName { get; set; }
        [Required]
        public string TaskType { get; set; }
        public int EstimatedManDays { get; set; }
        public int IsRequired { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public IEnumerable<WorkItemMember> Members { get; set; }
        public IEnumerable<WorkItemPrerequisite> Prerequisites { get; set; }
        
    }
}
