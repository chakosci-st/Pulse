using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class dtoNodeForm
    {
        public string Key { get; set; }
        public string Id { get; set; }             // "form_<formX>"
        public string Sysid { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
    }
}
