using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectChat
    {
        public string ChatSysId { get; set; }
        public string ProjectNo { get; set; }
        public string RoomName { get; set; }
        public string RoomIcon { get; set; }
        public string RoomColor { get; set; }
        public string Message { get; set; }
        public string SenderDisplayName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Viewed { get; set; }
        public string MetaJson { get; set; }
        public User CreatedByMeta { get; set; }
    }

    public class RoomMeta
    {
        public string Room { get; set; }
        public string RoomName { get; set; }
        public string RoomIcon { get; set; }
        public string RoomColor { get; set; }
        public string LoggedUser { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LastMessagePreview { get; set; }
    }
    
}
