using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class ProjectMilestoneCanceledEventArgs : BaseEventArgs
    {
        public string SysId;
        public string CanceledBy;
        public DateTime CanceledDate;
        public string Reason;
        public ProjectMilestoneCanceledEventArgs(string sysid, string canceledBy, DateTime canceledDate, string reason)
        {

            SysId = sysid;
            CanceledBy = canceledBy;
            CanceledDate = canceledDate;
            Reason = reason;

        }

    }
}