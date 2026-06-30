using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class PlantMemberRegisteredEventArgs : BaseEventArgs
    {
        public string PlantCode;
        public string PlantName;
        public string NewMemberCompleteName;
        public string NewMemberEmail;
        public string CreatedBy;
        public string CreatedByEmail;
        public DateTime CreatedDate;

        public PlantMemberRegisteredEventArgs(string plantCode, string plantName, string newMemberCompleteName, string newMemberEmail,  string createdby, string createdbyEmail, DateTime createddate)
        {

            PlantCode = plantCode;
            PlantName = plantName;
            NewMemberCompleteName = newMemberCompleteName;
            NewMemberEmail = newMemberEmail;
            CreatedBy = createdby;
            CreatedByEmail = createdbyEmail;
            CreatedDate = createddate; 

        }

    }
}