using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectDeletedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string DeletedBy;
        public DateTime DeletedDate;

        public ProjectDeletedEventArgs(string projectNo, string deletedBy, DateTime deletedDate, string reason)
        {

            ProjectNo = projectNo;
            DeletedBy = deletedBy;
            DeletedDate = deletedDate; 

        }

    }
}