using System;
using System.Threading;
using System.Threading.Tasks;
using GhostNetwork.Gateway.EventBus.Extensions.RedisMq;
using GhostNetwork.Gateway.EventBus.RedisMq;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace GhostNetwork.Gateway.RedisMq
{
    internal class RedisHandlerHostedService : IHostedService
    {
        private const int Timeout = 5000;

        private readonly IServiceProvider serviceProvider;
        private readonly ConfigurationOptions redisConfiguration;
        private readonly string connectionString;
        private ConnectionMultiplexer conn;

        public RedisHandlerHostedService(IServiceProvider serviceProvider, ConfigurationOptions redisConfiguration)
        {
            this.serviceProvider = serviceProvider;
            this.redisConfiguration = redisConfiguration;
        }

        public RedisHandlerHostedService(IServiceProvider serviceProvider, string connectionString)
        {
            this.serviceProvider = serviceProvider;
            this.connectionString = connectionString;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (redisConfiguration != null)
                {
                    conn = await ConnectionMultiplexer.ConnectAsync(redisConfiguration);
                }
                else
                {
                    conn = await ConnectionMultiplexer.ConnectAsync(connectionString);
                }
            }
            catch (RedisConnectionException)
            {
                throw new ApplicationException("Redis server is unavailable");
            }

            RunSubsribers();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await conn.CloseAsync();
            conn.Dispose();
        }

        private void RunSubsribers()
        {
            var db = conn.GetDatabase();
            foreach (var eventType in serviceProvider.GetEventsType())
            {
                new EventWorker(db, serviceProvider).Subscribe(eventType.Name, eventType);
            }
        }
    }
}
