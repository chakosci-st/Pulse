using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskCanceledEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string CanceledBy;
        public DateTime CanceledDate;
        public Status PreviousStatus;
        public string Reason;
        public TaskCanceledEventArgs(string taskSysId, Status previousStatus, string canceledBy, DateTime canceledDate, string reason)
        {

            TaskSysId = taskSysId;
            CanceledBy = canceledBy;
            CanceledDate = canceledDate;
            PreviousStatus = previousStatus;
            Reason = reason;

        }

    }
}