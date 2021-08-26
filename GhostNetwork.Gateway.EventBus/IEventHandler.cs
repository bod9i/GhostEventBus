using System.Threading.Tasks;
using GhostNetwork.Gateway.EventBus.Events;

namespace GhostNetwork.Gateway.EventBus
{
    /// <summary>
    /// Exposes the functionality to handle events.
    /// </summary>
    public interface IEventHandler 
    {
        /// <summary>
        /// Starts a handling of event represented by parent class of event-model.
        /// </summary>   
        Task Handle(EventBase value);
    }

    /// <summary>
    /// Exposes the generic variant of functionality to handle events.
    /// </summary>
    /// <typeparam name="TEvent">The type of event that needs to handle to.</typeparam>
    public interface IEventHandler<TEvent> : IEventHandler where TEvent : EventBase 
    {
        /// <summary>
        /// Starts a handling of event represented by generic-type.
        /// </summary>
        Task Handle(TEvent value);
    }
}