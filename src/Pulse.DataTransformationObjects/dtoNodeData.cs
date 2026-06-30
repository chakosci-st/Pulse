using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoNodeData
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Maturity { get; set; }       // null for activity
        public string Mandays { get; set; }        // string to match sample JSON
        public bool? IsRequired { get; set; }
        public bool? IsActive { get; set; }
        public string TransactionKey { get; set; }
    }
}
