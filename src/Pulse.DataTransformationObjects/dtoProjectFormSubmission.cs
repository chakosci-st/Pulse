using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoProjectFormSubmission 
    {
        public string ProjectNo { get; set; }
        public string FormSysId { get; set; }
        public string FormName { get; set; }
        public string FormDescription { get; set; }
        public string FormEntityLinkSysId { get; set; }
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public string FormJson { get; set; }

         
    }
}
