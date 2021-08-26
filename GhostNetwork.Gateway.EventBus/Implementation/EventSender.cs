using GhostNetwork.Gateway.EventBus.Events;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace GhostNetwork.Gateway.EventBus.RedisMq
{
    internal class EventSender : IEventSender
    {
        private readonly IDatabase db;
        private bool IsAvailable { get; set; }

        public EventSender(IDatabase db)
        {
            this.db = db;
            
            if (this.db != null)
            {
                IsAvailable = true;
            }
        }

        public async Task PublishAsync(EventBase @event)
        {
            if (!(await CheckAndRestoreConnection()))
            {
                return;
            }

            try
            {
               await db.ListRightPushAsync(@event.GetType().Name, JsonSerializer.Serialize(@event, @event.GetType()));
            }
            catch (RedisConnectionException)
            {
                IsAvailable = false;
            }
        }

        private async Task<bool> CheckAndRestoreConnection()
        {
            if (!IsAvailable)
            {
                try
                {
                    await db.PingAsync();
                }
                catch (RedisConnectionException)
                {
                    return false;
                }

                IsAvailable = true;
            }

            return true;
        }
    }
}
