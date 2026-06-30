using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectCreatedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string CreatedBy;
        public DateTime CreatedDate;

        public ProjectCreatedEventArgs(string projectno, string createdby, DateTime createddate)
        {

            ProjectNo = projectno;
            CreatedBy = createdby;
            CreatedDate = createddate; 

        }

    }
}