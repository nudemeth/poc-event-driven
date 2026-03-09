using Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    private readonly List<DomainEvent> _committedEvents = [];
    private readonly List<DomainEvent> _uncommittedEvents = [];

    protected Entity(TId id)
    {
        Id = id;
        Version = 0;
    }

    public TId Id { get; }
    public int Version { get; protected set; }
    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
    public IReadOnlyList<DomainEvent> CommittedEvents => _committedEvents.AsReadOnly();

    protected void ApplyUncommittedEvent(DomainEvent @event)
    {
        ApplyEventState(@event);
        Version = @event.Version;
        _uncommittedEvents.Add(@event);
    }

    protected void ApplyCommittedEvent(DomainEvent @event)
    {
        ApplyEventState(@event);
        Version = @event.Version;
        _committedEvents.Add(@event);
    }

    protected abstract void ApplyEventState(DomainEvent @event);

    public void CommitEvents()
    {
        _committedEvents.AddRange(_uncommittedEvents);
        _uncommittedEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return Id.Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}