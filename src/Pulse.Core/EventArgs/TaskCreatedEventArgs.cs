using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskCreatedEventArgs : BaseEventArgs
    {
        public string TaskSysId;
        public string CreatedBy;
        public DateTime CreatedDate;

        public TaskCreatedEventArgs(string taskSysId, string createdby, DateTime createddate)
        {

            TaskSysId = taskSysId;
            CreatedBy = createdby;
            CreatedDate = createddate; 

        }

    }
}