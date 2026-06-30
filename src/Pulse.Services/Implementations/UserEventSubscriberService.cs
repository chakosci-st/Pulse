using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core.EventArgs;
using Pulse.Core.Interfaces;

namespace Pulse.Infrastructure.Services
{
    public class UserEventSubscriberService : IEventSubscriber<UserCreatedEventArgs>, IEventSubscriber<UserUpdatedEventArgs>
    {
        private readonly IEmailService _emailService;
        public UserEventSubscriberService(IEmailService emailService)
        {
            _emailService = emailService;
        }


        public async Task Handle(UserCreatedEventArgs eventMessage)
        {
            //SEND EMAIL NOTIFICATION
           await _emailService.SendEmailAsync(eventMessage.Email, "", "Account now available", $"<p>Hi {eventMessage.UserCompleteName}, <br />Your account is now created.</p>");
        }

        public async Task Handle(UserUpdatedEventArgs eventMessage)
        {
            //SEND EMAIL NOTIFICATION
            await _emailService.SendEmailAsync(eventMessage.Email, "", "Account is updated", $"<p>Hi {eventMessage.UserCompleteName}, <br />Your account is now updated.</p>");
        }
    }


}
