using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class PlantMemberRegisteredSubscriber : IEventSubscriber<PlantMemberRegisteredEventArgs>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public PlantMemberRegisteredSubscriber(IUserRepository userRepository, IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
        }

        public async Task Handle(PlantMemberRegisteredEventArgs eventArgs)
        {

            var createdby = await _userRepository.GetAsync(eventArgs.CreatedBy);

            await _emailSender.SendPlantMemberRegisteredNotificationAsync(eventArgs.PlantCode, eventArgs.PlantName, eventArgs.NewMemberCompleteName, createdby.FirstName + " " + createdby.LastName, eventArgs.CreatedDate, eventArgs.NewMemberEmail.Split(','), createdby.Email.Split(','));

        }
    }
}
