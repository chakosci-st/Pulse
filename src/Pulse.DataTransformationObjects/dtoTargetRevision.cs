using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents a target revision of a Task.
/// </summary>
namespace Pulse.DataTransformationObjects
{
    public class dtoTargetRevision
    {
        [Key]
        public string TargetRevisionSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string MilestoneSysId { get; set; }
        public string TaskSysId { get; set; }
        public string TargetStart { get; set; }
        public string TargetCompletion { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 4)]
        public string Reason { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
