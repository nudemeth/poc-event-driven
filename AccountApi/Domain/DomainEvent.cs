using Mediator;

namespace Domain;

public abstract record DomainEvent(Guid StreamId) : INotification
{
    public DateTimeOffset Timestamp { get; init; } = DateTime.UtcNow;
}
