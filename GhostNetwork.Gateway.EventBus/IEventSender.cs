using GhostEventBus.Events;
using System.Threading.Tasks;

namespace GhostEventBus
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