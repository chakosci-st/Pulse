using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectCanceledEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string CanceledBy;
        public DateTime CanceledDate;
        public string Reason;
        public ProjectCanceledEventArgs(string projectNo, string canceledBy, DateTime canceledDate, string reason)
        {

            ProjectNo = projectNo;
            CanceledBy = canceledBy;
            CanceledDate = canceledDate;
            Reason = reason;

        }

    }
}