using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectTaskResumedEventArgs : BaseEventArgs
    {
        public string SysId;
        public string ContinuedBy;
        public DateTime ContinuedDate;
        public Status PreviousStatus;
        public string Reason;
        public ProjectTaskResumedEventArgs(string sysid, Status previousStatus, string continuedBy, DateTime continuedDate, string reason)
        {

            SysId = sysid;
            PreviousStatus = previousStatus;
            ContinuedBy = continuedBy;
            ContinuedDate = continuedDate;
            Reason = reason;

        }

    }
}