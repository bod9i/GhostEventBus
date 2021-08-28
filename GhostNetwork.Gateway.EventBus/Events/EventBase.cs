using System;

namespace GhostEventBus.Events
{
    public abstract class EventBase
    {
        public string TriggeredBy { get; init; }
        public DateTimeOffset CreatedOn { get; } = DateTimeOffset.UtcNow;
    }
}