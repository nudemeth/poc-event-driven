namespace Domain.Account;

/// <summary>
/// Snapshot of an AccountEntity at a specific version.
/// Contains all the state needed to restore the account without replaying all events.
/// </summary>
public record AccountSnapshot(
    Guid AccountId,
    int Version,
    string AccountHolder,
    decimal Balance,
    bool IsActive
) : Snapshot(AccountId, Version);
