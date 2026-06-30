using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Represents an annotation type in the system. This contains how the annotation will be filled-out and displayed
/// </summary>
namespace Pulse.Core.Entities
{
    public class AnnotationType : BaseEntity<int>
    {
        [Key]
        public int AnnotationTypeId { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string AnnotationTypeName { get; set; }
        [StringLength(200)]
        public string AnnotationTypeDesc { get; set; }
        public string AnnotationTypeOption { get; set; }
        public int IsPrivate { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
