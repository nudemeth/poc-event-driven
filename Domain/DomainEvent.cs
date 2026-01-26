namespace Domain;

public abstract record DomainEvent(Guid StreamId, string EventType)
{
    public DateTimeOffset Timestamp { get; init; } = DateTime.UtcNow;
}
