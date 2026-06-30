using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskContinuedEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string ContinuedBy;
        public DateTime ContinuedDate;
        public Status PreviousStatus;
        public string Reason;
        public TaskContinuedEventArgs(string taskSysId, Status previousStatus, string continuedBy, DateTime continuedDate, string reason)
        {

            TaskSysId = taskSysId;
            PreviousStatus = previousStatus;
            ContinuedBy = continuedBy;
            ContinuedDate = continuedDate;
            Reason = reason;

        }

    }
}