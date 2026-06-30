using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectFieldUpdatedEventArgs : BaseEventArgs
    {
 
 
        public string UpdatedBy;
        public DateTime UpdatedDate; 
        public ProjectFieldUpdatedEventArgs(string updatedBy, DateTime updatedDate)
        {
             
 
            UpdatedBy = updatedBy;
            UpdatedDate = updatedDate;

        }

    }
}