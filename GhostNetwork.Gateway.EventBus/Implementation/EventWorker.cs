using GhostEventBus.Events;
using GhostEventBus.RedisMq.Extensions;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEventBus.RedisMq.Implementation
{
    internal class EventWorker : IEventWorker
    {
        private readonly IDatabase db;
        private readonly IServiceProvider serviceProvider;

        public EventWorker(IDatabase db, IServiceProvider serviceProvider)
        {
            this.db = db;
            this.serviceProvider = serviceProvider;
        }

        public async void Subscribe<TEvent>() where TEvent : EventBase, new()
        {
            await Task.Run(async () => 
            {
                while (true)
                {
                    try
                    {
                        var message = await db.ListLeftPopAsync(typeof(TEvent).Name);

                        if (message.HasValue)
                        {
                            foreach (var handler in serviceProvider.GetHandlers<TEvent>())
                            {
                                await Task.Run(() => handler.Handle(JsonSerializer.Deserialize<TEvent>(message)));
                            }
                        }
                        else Thread.Sleep(500);
                    }
                    catch (RedisConnectionException)
                    {
                        Thread.Sleep(5000);
                    }
                    catch (RedisTimeoutException)
                    {
                        Thread.Sleep(5000);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(5000);
                    }
                }
            });
        }

        public async void Subscribe(string key, Type type) 
        {
            await Task.Run(async () => 
            {
                while (true)
                {
                    try
                    {
                        var message = await db.ListLeftPopAsync(key);

                        if (message.HasValue)
                        {
                            foreach (var handler in serviceProvider.GetHandlers(type))
                            {
                                await Task.Run(() => handler.Handle(JsonSerializer.Deserialize(message, type) as EventBase));
                            }
                        }
                        else Thread.Sleep(500);
                    }
                    catch (RedisConnectionException)
                    {
                        Thread.Sleep(5000);
                    }
                    catch (RedisTimeoutException)
                    {
                        Thread.Sleep(5000);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(5000);
                    }
                }
            });
        }
    }
}
