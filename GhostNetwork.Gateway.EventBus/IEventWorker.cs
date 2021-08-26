using System;
using GhostNetwork.Gateway.EventBus.Events;

namespace GhostNetwork.Gateway.EventBus
{
    /// <summary>
    /// Exposes the functionality for starting event listening.
    /// </summary>
    public interface IEventWorker
    {
        /// <summary>
        /// Starts listening Queue and run handler on event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event that listens to.</typeparam>
        void Subscribe<TEvent>() where TEvent : EventBase, new();
        /// <summary>
        /// Starts listening Queue and run handler on event.
        /// </summary>
        /// <param name="type">The type of event that listens to represented by System.Type.</param>
        void Subscribe(string key, Type type);
    }
}
