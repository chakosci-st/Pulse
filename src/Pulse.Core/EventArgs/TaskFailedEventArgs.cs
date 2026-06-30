using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskFailedEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string FailedBy;
        public DateTime FailedDate;
        public Status PreviousStatus;
        public string Reason;
        public TaskFailedEventArgs(string taskSysId, Status previousStatus, string failedby, DateTime faileddate, string reason)
        {

            TaskSysId = taskSysId;
            PreviousStatus = previousStatus;
            FailedBy = failedby;
            FailedDate = faileddate;
            Reason = reason;

        }

    }
}