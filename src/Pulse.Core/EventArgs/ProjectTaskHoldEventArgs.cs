using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectTaskHoldEventArgs : BaseEventArgs
    {
        public string SysId;
        public string HoldBy;
        public DateTime HoldDate;
        public string Reason;
        public ProjectTaskHoldEventArgs(string sysid, string holdby, DateTime holddate, string reason)
        {

            SysId = sysid;
            HoldBy = holdby;
            HoldDate = holddate;
            Reason = reason;

        }

    }
}