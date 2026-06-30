

using System.Threading.Tasks;

namespace Pulse.Core.Interfaces
{
    /// <summary>
    /// Interface for subscribing to events and handling them.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to subscribe to.</typeparam>
    public interface IEventSubscriber<TEvent>
    {
        /// <summary>
        /// Handles the event when it is published.
        /// </summary>
        /// <param name="eventMessage">The event instance containing event data.</param>
        Task Handle(TEvent eventMessage);
    }
}
