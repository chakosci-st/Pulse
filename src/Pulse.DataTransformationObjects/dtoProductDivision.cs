using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pulse.DataTransformationObjects
{
    public class dtoProductDivision
    {
        [Required]
        [StringLength(10, MinimumLength = 2)]
        public string ProductDivisionCode { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 3)]
        public string ProductDivisionName { get; set; }
        [StringLength(200)]
        public string ProductDivisionDescription { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}