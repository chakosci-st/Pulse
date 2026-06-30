using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectFieldDeletedEventArgs : BaseEventArgs
    {
 
 
        public string DeletedBy;
        public DateTime DeletedDate; 
        public ProjectFieldDeletedEventArgs(string deletedBy, DateTime deletedDate)
        {


            DeletedBy = deletedBy;
            DeletedDate = deletedDate;

        }

    }
}