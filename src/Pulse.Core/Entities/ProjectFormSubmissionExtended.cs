using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectFormSubmissionExtended : ProjectFormSubmission
    { 
        public string FormName { get; set; }
        public string FormDescription { get; set; } 
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public int OrderIndex { get; set; } 
        public string FormJson { get; set; }
    }
}
