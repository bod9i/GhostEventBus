using System.Threading.Tasks;
using GhostNetwork.Gateway.EventBus.Events;

namespace GhostNetwork.Gateway.EventBus
{
    /// <summary>
    /// Exposes the functionality to sending events to queue.
    /// </summary>
    public interface IEventSender
    {
        /// <summary>
        /// Publish a event to queue.
        /// </summary>
        Task PublishAsync(EventBase @event);
    }
}