using System;
using System.Threading.Tasks;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;

namespace Pulse.Infrastructure.Services
{
    public class PlantEventSubscribersService : IEventSubscriber<PlantMemberRegisteredEventArgs>
    {
        private readonly IEmailSender _emailSender; 

        public PlantEventSubscribersService(IEmailSender emailSender )
        {
            _emailSender = emailSender; 
        }

        public async Task Handle(PlantMemberRegisteredEventArgs eventArgs)
        {
            await _emailSender.SendPlantMemberRegisteredNotificationAsync(
                        eventArgs.PlantCode,
                        eventArgs.PlantName,
                        eventArgs.NewMemberCompleteName,
                        eventArgs.CreatedBy,
                        eventArgs.CreatedDate,
                        eventArgs.NewMemberEmail.Split(','),
                        eventArgs.CreatedByEmail.Split(',')
                    );
        }

    }
}
