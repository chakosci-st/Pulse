using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    /// <summary>
    /// Represents a field per plant.
    /// </summary>
    public class PlantField : BaseEntity<string>
    {
        [Key]
        public string PlantFieldSysId { get; set; }
        public string PlantCode { get; set; }
        public string CategoryCode { get; set; }
        public string MaturityCode { get; set; }
        public string WorkItemSysId { get; set; }
        public int? FieldId { get; set; }
        public int IsLocalOptions { get; set; }
        public string Options { get; set; }
        public int SequenceNo { get; set; }
        public int IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
    }
}
