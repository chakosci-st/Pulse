using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectUpdatedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string ProjectName;
        public string UpdatedBy;
        public DateTime UpdatedDate;
        public string RecipientEmail;
        public string CCEmail;
        public string URLPath;
        public ProjectUpdatedEventArgs(string projectNo, string projectName, string updatedBy, DateTime updatedDate, string urlpath, string recipientEmail, string ccEmail)
        {

            ProjectNo = projectNo;
            ProjectName = projectName;
            UpdatedBy = updatedBy;
            UpdatedDate = updatedDate;
            RecipientEmail = recipientEmail;
            CCEmail = ccEmail;
            URLPath = urlpath;

        }

    }
}