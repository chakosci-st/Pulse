using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectMilestoneCompletedEventArgs : BaseEventArgs
    {
        public string SysId;
        public string CompletedBy;
        public DateTime CompletedDate;
        public string Remarks;
        public ProjectMilestoneCompletedEventArgs(string sysid, string completedBy, DateTime completedDated, string remarks)
        {

            SysId = sysid;
            CompletedBy = completedBy;
            CompletedDate = completedDated;
            Remarks = remarks;
        }

    }
}