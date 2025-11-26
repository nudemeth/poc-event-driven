namespace Domain;

public abstract record DomainEvent(Guid StreamId)
{
    public DateTimeOffset Timestamp { get; init; } = DateTime.UtcNow;
}
