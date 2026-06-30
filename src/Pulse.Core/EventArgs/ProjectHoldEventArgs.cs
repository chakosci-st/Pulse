using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectHoldEventArgs : BaseEventArgs
    {
        public string ProjectNo; 
        public string FromStatus;
        public string ToStatus;
        public string Reason;
        public string UpdatedBy;


        public ProjectHoldEventArgs(string projectNo, string fromStatus, string toStatus, string reason, string updatedBy)
        {

            ProjectNo = projectNo; 
            FromStatus = fromStatus;
            ToStatus = toStatus;
            Reason = reason;
            UpdatedBy = updatedBy;
        }

    }
}