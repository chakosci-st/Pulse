using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectTaskStartedEventArgs : BaseEventArgs
    {
        public string SysId;
        public string StartedBy;
        public DateTime StartedDate;
        public string Remarks;
        public ProjectTaskStartedEventArgs(string sysid, string startedBy, DateTime startedDate, string remarks)
        {

            SysId = sysid;
            StartedBy = startedBy;
            StartedDate = startedDate;
            Remarks = remarks;
        }

    }
}