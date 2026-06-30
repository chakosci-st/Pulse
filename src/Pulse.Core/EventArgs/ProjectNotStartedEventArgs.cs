using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectNotStartedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string ProjectName;
        public string ProductCode;
        public string Plant;
        public string Category;
        public DateTime StartedDate;
        public string StartedBy;
        public string Remarks;
        public string RecipientEmail;
        public string CCEmail;
        public ProjectNotStartedEventArgs(string projectNo, string projectName, string productCode,
            string plant, string category, string startedBy, DateTime datecreated, string remarks,
            string recipientEmail, string ccEmail)
        {

            ProjectNo = projectNo;
            ProjectName = projectName;
            ProductCode = productCode;
            Category = category;
            Plant = plant;
            StartedBy = startedBy;
            StartedDate = datecreated;
            Remarks = remarks;
            RecipientEmail = recipientEmail;
            CCEmail = ccEmail;
        }

    }
}