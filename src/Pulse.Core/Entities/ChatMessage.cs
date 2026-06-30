using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ChatMessage
    {
        public string ChatSysId { get; set; }

        public string ProjectNo { get; set; }
         
        public string Message { get; set; }

        public string SenderDisplayName { get; set; }
         
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
