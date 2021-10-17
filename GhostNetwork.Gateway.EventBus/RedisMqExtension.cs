using GhostEventBus.Events;
using GhostEventBus.RedisMq.Implementation;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GhostEventBus.RedisMq.Extensions
{
    public static class RedisEventBusExtension
    {
        /// <summary>
        /// Add IEventSender to your DI as Singletone.
        /// </summary>
        /// <returns>
        /// IService collection with added EventSender as IEventSender.
        /// </returns>
        public static IServiceCollection AddEventSenderAsSingletone(this IServiceCollection services, ConfigurationOptions redisConfiguration)
        {
            IDatabase redisDb = null;

            try
            {
                redisDb = ConnectionMultiplexer.Connect(redisConfiguration).GetDatabase();
            }
            catch (RedisConnectionException)
            {
                throw new ApplicationException("Redis server is unavailable");
            }

            services.AddSingleton<IEventSender>(new EventSender(redisDb));
            return services;
        }

        /// <summary>
        /// Add IEventSender to your DI as Singletone.
        /// </summary>
        /// <returns>
        /// IService collection with added EventSender as IEventSender.
        /// </returns>
        public static IServiceCollection AddEventSenderAsSingletone(this IServiceCollection services, string connectionString)
        {
            IDatabase redisDb = null;

            try
            {
                redisDb = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
            }
            catch (RedisConnectionException)
            {
                throw new ApplicationException("Redis server is unavailable");
            }

            services.AddSingleton<IEventSender>(new EventSender(redisDb));
            return services;
        }

        /// <summary>
        /// Add IEventSender as NullEventSender to your DI as Singletone.
        /// </summary>
        /// <remarks>
        /// NullEventSender does nothing. This is needed to simulate sending messages for different cases. 
        /// </remarks>
        /// <returns>
        /// IService collection with added EventSender as IEventSender.
        /// </returns>
        public static IServiceCollection AddNullEventSenderAsSingletone(this IServiceCollection services)
        {            
            services.AddSingleton<IEventSender>(new NullEventSender());
            return services;
        }

        /// <summary>
        /// Add RedisEventsHostedService for start event listening as IHostedService to your DI.
        /// </summary>
        /// <remarks>
        /// RedisHostedService works from start and to end working of application.
        /// Also pay your attention to added handlers that implemented IEventHandler interfaces to your DI. 
        /// It have decraled wich event workers will start after sucsesfull connecting to Redis-Server.
        /// </remarks>
        /// <returns>
        /// IService collection with added RedisEventsHostedService as IHostedService.
        /// </returns>
        public static IServiceCollection AddHostedWorkerService(this IServiceCollection services, ConfigurationOptions redisConfiguration)
        {
            services.AddHostedService(provider => new RedisEventsHostedService(provider, redisConfiguration));
            return services;
        }

        /// <summary>
        /// Add IHostedService that start redis event-bus to your DI.
        /// </summary>
        /// <remarks>
        /// RedisHostedService works from start and to end working of application.
        /// Also pay your attention to added handlers that implemented IEventHandler interfaces to your DI. 
        /// It have decraled wich event workers will start after sucsesfull connecting to Redis-Server.
        /// </remarks>
        /// <returns>
        /// IService collection with added RedisEventsHostedService as IHostedService.
        /// </returns>
        public static IServiceCollection AddHostedWorkerService(this IServiceCollection services, string connectionString)
        {
            services.AddHostedService(provider => new RedisEventsHostedService(provider, connectionString));
            return services;
        }
    }

    internal static class InitializeHelper
    {
        public static IEnumerable<Type> GetEventsType(this IServiceProvider serviceProvider)
        {
            var typeOfHandlers = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly != Assembly.GetExecutingAssembly())
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(IEventHandler).IsAssignableFrom(t))
                    .Where(types => types.GetTypeInfo().ImplementedInterfaces.Any(ii => ii.IsGenericType))
                );

            typeOfHandlers.Union(AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly != Assembly.GetExecutingAssembly())
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IEventHandler<>).IsAssignableFrom(t))
                .Where(types => types.GetTypeInfo().ImplementedInterfaces.Any(ii => ii.IsGenericType))
            );

            if (!typeOfHandlers.Any())
                throw new ArgumentException("Input type is not declared as inheritor of IEventHandler<Event>");

            var typeOfEvents = new List<Type>();

            using (var scope = serviceProvider.CreateScope())
            {
                foreach (var type in typeOfHandlers)
                {
                    var handler = scope.ServiceProvider.GetService(type) as IEventHandler;

                    if (handler == null)
                    {
                        continue;
                    }

                    typeOfEvents.Add(type.GetTypeInfo().ImplementedInterfaces.First().GetTypeInfo().GenericTypeArguments[0]);
                }
            }

            return typeOfEvents.Distinct();
        }

        public static IEnumerable<IEventHandler<TEvent>> GetHandlers<TEvent>(this IServiceProvider serviceProvider) where TEvent : EventBase, new()
        {
            var typeOfHandlers = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly != Assembly.GetExecutingAssembly())
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(IEventHandler<TEvent>).IsAssignableFrom(t))
                    .Where(types => types.GetTypeInfo().ImplementedInterfaces.Any(ii => ii.IsGenericType))
                );

            if (!typeOfHandlers.Any())
                throw new ArgumentException("Input type is not declared as inheritor of IEventHandler<Event>");

            var handlers = new List<IEventHandler<TEvent>>();

            using (var scope = serviceProvider.CreateScope())
            {
                foreach (var type in typeOfHandlers)
                {
                    var handler = scope.ServiceProvider.GetService(type) as IEventHandler<TEvent>;

                    if (handler == null)
                    {
                        continue;
                    }

                    handlers.Add(handler);
                }
            }

            return handlers;
        }

        public static IEnumerable<IEventHandler> GetHandlers(this IServiceProvider serviceProvider, Type type)
        {
            var typeOfHandlers = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly != Assembly.GetExecutingAssembly())
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(IEventHandler).IsAssignableFrom(t))
                    .Where(types => types.GetTypeInfo().ImplementedInterfaces.Any(ii => ii.IsGenericType &&
                        ii.GetTypeInfo().GenericTypeArguments.Any(arg => arg.FullName == type.FullName)))
                );

            if (!typeOfHandlers.Any())
                throw new ArgumentException("Input type is not declared as inheritor of IEventHandler");

            var handlers = new List<IEventHandler>();

            using (var scope = serviceProvider.CreateScope())
            {
                foreach (var handlerType in typeOfHandlers)
                {
                    var handler = scope.ServiceProvider.GetService(handlerType) as IEventHandler;

                    if (handler == null)
                    {
                        continue;
                    }

                    handlers.Add(handler);
                }
            }

            return handlers;
        }
    }
}