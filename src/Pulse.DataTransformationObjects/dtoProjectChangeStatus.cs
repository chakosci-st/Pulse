using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoProjectChangeStatus
    {
        public string ProjectNo { get; set; }
        public string TransactionKey { get; set; }
        public string Reason { get; set; }
        public int IsActive { get; set; }
    }
}
