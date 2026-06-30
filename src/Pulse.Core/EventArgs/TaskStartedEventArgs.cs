using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskStartedEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string StartedBy;
        public DateTime StartedDate; 
        public string Remarks;
        public TaskStartedEventArgs(string taskSysId,  string startedBy, DateTime startedDate, string remarks)
        {

            TaskSysId = taskSysId;
            StartedBy = startedBy;
            StartedDate = startedDate;
            Remarks = remarks;
        }

    }
}