using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectPromotedEventArgs : BaseEventArgs
    {
        public string ProjectNo;
        public string MaturityCode;
        public string PromotedBy;
        public DateTime PromotedDate; 
        public ProjectPromotedEventArgs(string projectNo, string maturityCode,  string promotedBy, DateTime promotedDate)
        {

            ProjectNo = projectNo;
            MaturityCode = maturityCode;
            PromotedBy = promotedBy;
            PromotedDate = promotedDate; 
        }

    }
}