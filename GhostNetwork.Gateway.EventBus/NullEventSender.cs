using System.Threading.Tasks;
using GhostEventBus.Events;

namespace GhostEventBus
{
    public class NullEventSender : IEventSender
    {
        public Task PublishAsync(EventBase @event)
        {
            return Task.CompletedTask;
        }
    }
}