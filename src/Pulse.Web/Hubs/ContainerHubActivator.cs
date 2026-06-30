using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Pulse.Web.Hubs
{
    public class ContainerHubActivator : IHubActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ContainerHubActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            // Try resolve from container
            var hub = _serviceProvider.GetService(descriptor.HubType) as IHub;

            if (hub == null)
            {
                // Fallback: default constructor if not registered
                hub = Activator.CreateInstance(descriptor.HubType) as IHub;
            }

            return hub;
        }
    }
}