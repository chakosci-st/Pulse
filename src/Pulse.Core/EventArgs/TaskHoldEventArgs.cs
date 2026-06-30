using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskHoldEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string HoldBy;
        public DateTime HoldDate;
        public Status PreviousStatus;
        public string Reason;
        public TaskHoldEventArgs(string taskSysId, Status previousStatus, string holdby, DateTime holddate, string reason)
        {

            TaskSysId = taskSysId;
            PreviousStatus = previousStatus;
            HoldBy = holdby;
            HoldDate = holddate;
            Reason = reason;

        }

    }
}