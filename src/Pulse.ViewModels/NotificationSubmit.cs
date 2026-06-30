using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.ViewModels
{
    public class NotificationSubmit
    {
        public string NotificationSysId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
        public DateTime NotificationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ProjectNo { get; set; }
        public string EntityType { get; set; }
        public string EntitySysId { get; set; }
    }
}
