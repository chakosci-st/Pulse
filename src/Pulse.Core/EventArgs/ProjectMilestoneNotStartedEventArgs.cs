using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectMilestoneNotStartedEventArgs : BaseEventArgs
    {
        public string SysId;
        public string NotStartedBy;
        public DateTime NotStartedDate;
        public string Reason;
        public ProjectMilestoneNotStartedEventArgs(string sysid, string notStartedBy, DateTime notStartedDate, string reason)
        {

            SysId = sysid;
            NotStartedBy = notStartedBy;
            NotStartedDate = notStartedDate;
            Reason = reason;
        }

    }
}