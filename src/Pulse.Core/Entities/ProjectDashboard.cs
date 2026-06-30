using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Entities
{
    public class ProjectDashboardCounter
    {
        public string UserId { get; set; }
        public int ActiveProjects { get; set; }
        public int InProgress { get; set; }
        public int CompletedTasks { get; set; }
        public int Overdue { get; set; }
    }
}
