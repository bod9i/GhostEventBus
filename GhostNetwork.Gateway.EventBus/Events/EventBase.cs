using System;

namespace GhostNetwork.Gateway.EventBus.Events
{
    public abstract class EventBase
    {
        public string TriggeredBy { get; init; }
        public DateTimeOffset CreatedOn { get; } = DateTimeOffset.UtcNow;
    }
}