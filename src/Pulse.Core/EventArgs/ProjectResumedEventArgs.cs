using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectResumedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string ContinuedBy;
        public DateTime ContinuedDate;
        public string PreviousStatus;
        public string Reason;
        public ProjectResumedEventArgs(string projectNo, string previousStatus, string continuedBy, DateTime continuedDate, string reason)
        {

            ProjectNo = projectNo;
            PreviousStatus = previousStatus;
            ContinuedBy = continuedBy;
            ContinuedDate = continuedDate;
            Reason = reason;

        }

    }
}