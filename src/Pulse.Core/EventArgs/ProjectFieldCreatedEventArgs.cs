using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectFieldCreatedEventArgs : BaseEventArgs
    {
 
 
        public string CreatedBy;
        public DateTime CreatedDate; 
        public ProjectFieldCreatedEventArgs(string createdBy, DateTime createdDate)
        {
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }

    }
}