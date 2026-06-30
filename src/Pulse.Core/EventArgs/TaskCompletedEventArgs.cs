using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskCompletedEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string CompletedBy;
        public DateTime CompletedDate;
        public Status PreviousStatus;
        public string Remarks;
        public TaskCompletedEventArgs(string taskSysId, Status previousStatus, string completedBy, DateTime completedDated, string remarks)
        {

            TaskSysId = taskSysId;
            CompletedBy = completedBy;
            CompletedDate = completedDated;
            PreviousStatus = previousStatus;
            Remarks = remarks;
        }

    }
}