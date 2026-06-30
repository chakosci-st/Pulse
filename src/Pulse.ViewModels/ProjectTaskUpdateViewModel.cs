using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.ViewModels
{
    public class ProjectTaskUpdateViewModel
    {
        [Required]
        public string ProjectNo { get; set; }
        public string ProjectTaskSysId { get; set; }
        public string RoadmapActivitySysId { get; set; }
        public string TransactionKey { get; set; }
        public string Remarks { get; set; }
    }
}
