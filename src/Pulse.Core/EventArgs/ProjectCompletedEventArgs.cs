using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectCompletedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string CompletedBy;
        public DateTime CompletedDate;
        public string Remarks;
        public ProjectCompletedEventArgs(string projectNo, string completedBy, DateTime completedDated, string remarks)
        {

            ProjectNo = projectNo;
            CompletedBy = completedBy;
            CompletedDate = completedDated;
            Remarks = remarks;
        }

    }
}