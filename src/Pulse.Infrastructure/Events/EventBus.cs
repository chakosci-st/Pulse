using Autofac;

using Pulse.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Events
{
 

    public class EventBus : IEventPublisher
    {
        private readonly ILifetimeScope _scope;

        public EventBus(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async Task Publish<TEvent>(TEvent eventMessage)
        {
            var subscribers = _scope.Resolve<IEnumerable<IEventSubscriber<TEvent>>>();

            var tasks = subscribers.Select(s => s.Handle(eventMessage));
            await Task.WhenAll(tasks);
        }


        ////private readonly ConcurrentDictionary<Type, List<WeakReference>> _subscribers =
        ////    new ConcurrentDictionary<Type, List<WeakReference>>();

        ////public void Subscribe<TEvent>(IEventSubscriber<TEvent> subscriber)
        ////{
        ////    var eventType = typeof(TEvent);
        ////    var weakRef = new WeakReference(subscriber);

        ////    _subscribers.AddOrUpdate(
        ////        eventType,
        ////        _ => new List<WeakReference> { weakRef },
        ////        (_, list) =>
        ////        {
        ////            list.Add(weakRef);
        ////            return list;
        ////        });
        ////}

        ////public void Unsubscribe<TEvent>(IEventSubscriber<TEvent> subscriber)
        ////{
        ////    var eventType = typeof(TEvent);
        ////    List<WeakReference> list;
        ////    if (_subscribers.TryGetValue(eventType, out list))
        ////    {
        ////        lock (list)
        ////        {
        ////            list.RemoveAll(wr => !wr.IsAlive || wr.Target == subscriber);
        ////        }
        ////    }
        ////}

        ////public async Task Publish<TEvent>(TEvent eventToPublish)
        ////{
        ////    var eventType = typeof(TEvent);
        ////    List<WeakReference> list;
        ////    if (_subscribers.TryGetValue(eventType, out list))
        ////    {
        ////        var toRemove = new List<WeakReference>();
        ////        foreach (var weakRef in list.ToList())
        ////        {
        ////            var target = weakRef.Target as IEventSubscriber<TEvent>;
        ////            if (weakRef.IsAlive && target != null)
        ////            {
        ////                await target.Handle(eventToPublish);
        ////            }
        ////            else
        ////            {
        ////                toRemove.Add(weakRef);
        ////            }
        ////        }
        ////        // Clean up dead references
        ////        if (toRemove.Count > 0)
        ////        {
        ////            lock (list)
        ////            {
        ////                foreach (var wr in toRemove)
        ////                    list.Remove(wr);
        ////            }
        ////        }
        ////    }
        ////}
    }

    ///////// <summary>
    ///////// Event bus implementation for publishing and subscribing to events.
    ///////// </summary>
    //////public class EventBus : IEventPublisher
    //////{
    //////    //private readonly ConcurrentDictionary<Type, List<WeakReference>> _subscribers = new ConcurrentDictionary<Type, List<WeakReference>>();
    //////    private readonly ConcurrentDictionary<Type, List<object>> _subscribers = new ConcurrentDictionary<Type, List<object>>();

    //////    public void Subscribe<TEvent>(IEventSubscriber<TEvent> subscriber)
    //////    {
    //////        var eventType = typeof(TEvent);
    //////        _subscribers.AddOrUpdate(
    //////            eventType,
    //////            _ => new List<object> { subscriber },
    //////            (_, list) => { list.Add(subscriber); return list; }
    //////        );

    //////        ////var eventType = typeof(TEvent);
    //////        ////var weakReference = new WeakReference(subscriber);

    //////        ////_subscribers.AddOrUpdate(
    //////        ////    eventType,
    //////        ////    _ => new List<WeakReference> { weakReference },
    //////        ////    (_, existingSubscribers) =>
    //////        ////    {
    //////        ////        existingSubscribers.Add(weakReference);
    //////        ////        return existingSubscribers;
    //////        ////    });
    //////    }

    //////    ////public void Unsubscribe<TEvent>(IEventSubscriber<TEvent> subscriber)
    //////    ////{
    //////    ////    var eventType = typeof(TEvent);

    //////    ////    if (_subscribers.TryGetValue(eventType, out var subscribers))
    //////    ////    {
    //////    ////        subscribers.RemoveAll(wr => !wr.IsAlive || wr.Target == subscriber);
    //////    ////    }
    //////    ////}

    //////    public void Unsubscribe<TEvent>(IEventSubscriber<TEvent> subscriber)
    //////    {
    //////        var eventType = typeof(TEvent);
    //////        if (_subscribers.TryGetValue(eventType, out var list))
    //////        {
    //////            lock (list)
    //////            {
    //////                list.RemoveAll(s => ReferenceEquals(s, subscriber));
    //////            }
    //////        }
    //////    }

    //////    public async Task Publish<TEvent>(TEvent eventToPublish)
    //////    {
    //////        var eventType = typeof(TEvent);
    //////        if (_subscribers.TryGetValue(eventType, out var list))
    //////        {
    //////            foreach (var subscriber in list)
    //////            {
    //////                await((IEventSubscriber<TEvent>)subscriber).Handle(eventToPublish);
    //////            }
    //////        }

    //////        ////var eventType = typeof(TEvent);

    //////        ////if (_subscribers.TryGetValue(eventType, out var subscribers))
    //////        ////{
    //////        ////    var subscribersSnapshot = subscribers.ToList();

    //////        ////    foreach (var weakReference in subscribersSnapshot)
    //////        ////    {
    //////        ////        if (weakReference.IsAlive && weakReference.Target is IEventSubscriber<TEvent> subscriber)
    //////        ////        {
    //////        ////            try
    //////        ////            {
    //////        ////                Task.Run(() => subscriber.Handle(eventToPublish));
    //////        ////            }
    //////        ////            catch (Exception ex)
    //////        ////            {
    //////        ////                Console.WriteLine($"Error handling event {eventType.Name}: {ex.Message}");
    //////        ////            }
    //////        ////        }
    //////        ////        else
    //////        ////        {
    //////        ////            subscribers.Remove(weakReference);
    //////        ////        }
    //////        ////    }
    //////        ////}
    //////    }


    //////}
}
