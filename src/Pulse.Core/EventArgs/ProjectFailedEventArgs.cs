using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectFailedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string FailedBy;
        public DateTime FailedDate;
        public string Reason;
        public ProjectFailedEventArgs(string projectno, string failedby, DateTime faileddate, string reason)
        {

            ProjectNo = projectno;
            FailedBy = failedby;
            FailedDate = faileddate;
            Reason = reason;

        }

    }
}