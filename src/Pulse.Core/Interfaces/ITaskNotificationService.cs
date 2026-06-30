using Pulse.Core.Entities;
using Pulse.Core.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
namespace Pulse.Core.Interfaces
{
    public interface ITaskNotificationService
    {
        Task NotifyTaskCreated(TaskCreatedEventArgs eventMessage);
        Task NotifyCompletedCompleted(TaskCompletedEventArgs eventMessage);
        Task NotifyTaskCancelled(TaskCanceledEventArgs eventMessage);
        Task NotifyTaskHold(TaskHoldEventArgs eventMessage);
        Task NotifyHoldFailed(TaskFailedEventArgs eventMessage);
        Task NotifyTaskStarted(TaskStartedEventArgs eventMessage);
        Task NotifyTaskContinued(TaskContinuedEventArgs eventMessage);


    }
}
