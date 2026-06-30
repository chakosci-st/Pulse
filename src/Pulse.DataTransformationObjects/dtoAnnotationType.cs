using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoAnnotationType
    {
        public int AnnotationTypeId { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string AnnotationTypeName { get; set; }
        [StringLength(200)]
        public string AnnotationTypeDesc { get; set; }
        public string AnnotationTypeOption { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
