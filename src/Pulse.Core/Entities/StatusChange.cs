using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a status change of a task in a project/milestone.
/// </summary>
namespace Pulse.Core.Entities
{
   public class StatusChange : BaseEntity<string>
    {
        [Key]
        public string StatusChangeSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string MilestoneSysId { get; set; }
        [Required]
        public string TaskSysId { get; set; }
        [Required]
        public string Status { get; set; }
        public string Reason { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
