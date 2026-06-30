using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    /// <summary>
    /// Represents a field in a project/milestone/task.
    /// </summary>
    public class ProjectField : BaseEntity<string>
    {
        [Key]
        public string ProjectFieldSysId { get; set; }
        [Required]
        public string ProjectNo { get; set; }
        public string MilestoneSysId { get; set; }
        public string TaskSysId { get; set; }
        [Required]
        public string PlantFieldSysId { get; set; }
        public string FieldValue { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string TransactionKey { get; set; }
        public Field Field { get; set; }
    }
}
