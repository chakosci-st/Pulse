using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.DataTransformationObjects
{
    public class UpdateMemberTasksRequest
    {
        public string ProjectNo { get; set; }

        public string MemberId { get; set; } 

        public string MilestoneId { get; set; }   // the milestone NODEID in UI

        public List<string> NewlySelectedTaskIds { get; set; } = new List<string>();

        public List<string> NewlyUnselectedTaskIds { get; set; } = new List<string>();
    }
}
