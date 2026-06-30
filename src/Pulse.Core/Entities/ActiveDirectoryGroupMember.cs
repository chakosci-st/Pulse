using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
   public class ActiveDirectoryGroupMember
    {
        public string ADGroupName { get; set; }
        public string UserId { get; set; }
        public string STEdUID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
