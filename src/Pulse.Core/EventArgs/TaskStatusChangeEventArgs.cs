using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{

    public class TaskStatusChangeEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public Status FromStatus;
        public Status ToStatus;
        public string ChangedById;
        public DateTime DateChanged;
        public string Reason;
        public TaskStatusChangeEventArgs(string taskSysId, Status fromStatus, Status toStatus, string changedById, DateTime dateChanged, string reason)
        {

            TaskSysId = taskSysId;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            ChangedById = changedById;
            DateChanged = dateChanged;
            Reason = reason;

        }

    }

}