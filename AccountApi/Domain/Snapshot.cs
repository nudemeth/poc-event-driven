namespace Domain;

/// <summary>
/// Represents a snapshot of an entity's state at a specific version.
/// Snapshots are used to optimize the loading of entities by avoiding the need to replay all events.
/// </summary>
public abstract record Snapshot
{
    public string StreamId { get; }
    public int Version { get; }
    public DateTimeOffset Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Protected constructor that accepts a Guid. Child classes must use this to initialize.
    /// The StreamId string is computed internally, preventing direct string exposure.
    /// </summary>
    protected Snapshot(Guid streamId, int version)
    {
        StreamId = $"{streamId}-Snapshot";
        Version = version;
    }
}
