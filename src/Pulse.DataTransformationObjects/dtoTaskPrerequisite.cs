using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a link between a task and a prerequisite task in a project.
/// </summary>
namespace Pulse.DataTransformationObjects
{
    public class dtoTaskPrerequisite
    {
        [Required]
        public string ProjectNo { get; set; }
        [Required]
        public string TaskSysId { get; set; }
        [Required]
        public string PrerequisiteTaskSysId { get; set; }
    }
}
