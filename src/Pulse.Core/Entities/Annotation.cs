using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents an annotation in a project/milestone/task.
/// </summary>
namespace Pulse.Core.Entities
{
    public class Annotation : BaseEntity<string>
    {
        [Key]
        public string AnnotationSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string MilestoneSysId { get; set; }
        public string TaskSysId { get; set; }
        public int AnnotationTypeId { get; set; }
        public string AnnotationValue { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public AnnotationType AnnotationType { get; set; }
    }
}
