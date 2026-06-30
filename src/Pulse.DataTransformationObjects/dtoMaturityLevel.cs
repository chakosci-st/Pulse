using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoMaturityLevel
    {
        [Required]
        [StringLength(10, MinimumLength = 4)]
        public string MaturityCode { get; set; }
        [Required]
        public int MaturityNumber { get; set; }
        [Required]
        public int SequenceNo { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}