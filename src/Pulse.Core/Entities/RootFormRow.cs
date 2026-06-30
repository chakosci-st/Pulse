using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class RootFormRow
    {
        public string FormRootKey { get; set; }
        public string FormRootId { get; set; }
        public string SysId { get; set; }
        public string FormName { get; set; }
        public string FormDescription { get; set; }
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
    }
}
