using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents an audittrail of a project.
/// </summary>
namespace Pulse.Core.Entities
{
  public  class ProjectAuditTrail : BaseEntity<string>
    {
        [Key]
        public string AudiTrailSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        [Required]
        public string ActionDetails { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
    }
}
