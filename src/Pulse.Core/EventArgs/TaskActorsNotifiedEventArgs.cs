using Pulse.Core.Enums;
using System;
using BaseEventArgs = System.EventArgs;

namespace Pulse.Core.EventArgs
{
    public class TaskActorsNotifiedEventArgs : BaseEventArgs
    {
        private string _task;
        private string _notifiedusers;
        public TaskActorsNotifiedEventArgs(string task, string notifiedusers)
        {
            _task = task;
            _notifiedusers = notifiedusers;
        }
    }
}
