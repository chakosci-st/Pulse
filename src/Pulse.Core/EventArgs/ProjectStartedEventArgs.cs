using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectStartedEventArgs : BaseEventArgs
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
        public string Milestone;
        public ProjectStartedEventArgs(string projectNo, string projectName, string productCode,
            string plant, string category, string startedBy, string milestone, DateTime datecreated, string remarks,
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
            Milestone = milestone;
            CCEmail = ccEmail;
        }

    }
}