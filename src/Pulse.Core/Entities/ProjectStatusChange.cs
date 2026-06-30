using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a status change of a project.
/// </summary>
namespace Pulse.Core.Entities
{
   public class ProjectStatusChange : BaseEntity<string>
    {
        [Key]
        public string StatusChangeSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
        public DateTime ActualDate { get; set; }
        [Required]
        public string Status { get; set; }
        public string Remarks { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
