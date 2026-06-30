using Autofac;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.Events;
using System;
using System.Linq;

public class EventBusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<EventBus>()
            .As<IEventPublisher>()
            .SingleInstance()
            .OnActivated(e =>
            {
                var context = e.Context;

                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return new Type[0]; }
                    })
                    .Where(t => t.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubscriber<>)))
                    .ToList();

                foreach (var type in allTypes)
                {
                    foreach (var iface in type.GetInterfaces().Where(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubscriber<>)))
                    {
                        try
                        {
                            var eventType = iface.GetGenericArguments()[0];
                            var subscriber = context.Resolve(iface); // resolve subscriber via DI

                            var subscribeMethod = typeof(IEventPublisher)
                                .GetMethod("Subscribe")
                                .MakeGenericMethod(eventType);

                            subscribeMethod.Invoke(e.Instance, new[] { subscriber });
                        }
                        catch
                        {
                            // Optionally log error
                        }
                    }
                }
            });

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .ToArray();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IEventSubscriber<>))
            .InstancePerDependency();
    }
}


//////using Autofac;
//////using Pulse.Core.Interfaces;
//////using Pulse.Infrastructure.Events;
//////using System;
//////using System.Linq;

//////public class EventBusModule : Module
//////{
//////    protected override void Load(ContainerBuilder builder)
//////    {
//////        builder.RegisterType<EventBus>()
//////            .As<IEventPublisher>()
//////            .SingleInstance()
//////            .OnActivated(e =>
//////            {
//////                var context = e.Context;
//////                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
//////                    .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
//////                    .SelectMany(a =>
//////                    {
//////                        try { return a.GetTypes(); }
//////                        catch { return new Type[0]; }
//////                    })
//////                    .Where(t => t.GetInterfaces().Any(i =>
//////                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubscriber<>)))
//////                    .ToList();

//////                foreach (var type in allTypes)
//////                {
//////                    foreach (var iface in type.GetInterfaces().Where(i =>
//////                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubscriber<>)))
//////                    {
//////                        try {
//////                            var eventType = iface.GetGenericArguments()[0];
//////                            var subscriber = context.Resolve(iface);
//////                            var subscribeMethod = typeof(IEventPublisher)
//////                                .GetMethod("Subscribe")
//////                                .MakeGenericMethod(eventType);
//////                            subscribeMethod.Invoke(e.Instance, new[] { subscriber });
//////                        }
//////                        catch(Exception ex) {

//////                        }

//////                    }
//////                }
//////            });

//////        // Register all subscribers from all loaded assemblies
//////        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
//////            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
//////            .ToArray();

//////        builder.RegisterAssemblyTypes(assemblies)
//////            .AsClosedTypesOf(typeof(IEventSubscriber<>))
//////            .InstancePerDependency();
//////    }
//////}