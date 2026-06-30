using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoFieldWithStats : dtoField
    {
        public int FormLinkedCount { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
    }
}
