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
    public interface IPlantNotificationService
    {
        Task NotifyMemberRegistered(PlantMemberRegisteredEventArgs eventMessage); 

    }
}
