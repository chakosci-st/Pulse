

using System.Threading.Tasks;

namespace Pulse.Core.Interfaces
{
    /// <summary>
    /// Interface for publishing events to subscribers.
    /// </summary>
    public interface IEventPublisher
    {
        ////void Subscribe<TEvent>(IEventSubscriber<TEvent> subscriber);
        ////void Unsubscribe<TEvent>(IEventSubscriber<TEvent> subscriber);
        /// <summary>
        /// Publishes an event to all subscribers of the event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event being published.</typeparam>
        /// <param name="eventToPublish">The event instance to publish.</param>
        Task Publish<TEvent>(TEvent eventToPublish);


 
    }
}
