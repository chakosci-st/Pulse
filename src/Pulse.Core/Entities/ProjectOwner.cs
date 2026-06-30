using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectOwner
    {
        public string ProjectOwnerSysId { get; set; }
        public string ProjectNo { get; set; }
        public string UserId { get; set; }
        public string ParentType { get; set; }
        public string ParentSysId { get; set; }
        public User OwnerMeta { get; set; }
    }
}
