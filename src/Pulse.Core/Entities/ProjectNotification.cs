using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectNotification
    {
        public string NotificationSysId { get; set; }
        public string ProjectNo { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
        public DateTime NotificationDate { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MetaJson { get; set; }
        public User CreatedByMeta { get; set; }
    }
}
