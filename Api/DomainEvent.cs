public abstract record DomainEvent(Guid StreamId)
{
    public DateTimeOffset Timestamp { get; init; } = DateTime.UtcNow;
}

public record AccountOpened(Guid AccountId, string AccountHolder, decimal InitialDeposit) : DomainEvent(AccountId);
public record MoneyDeposited(Guid AccountId, decimal Amount) : DomainEvent(AccountId);
public record MoneyWithdrawn(Guid AccountId, decimal Amount) : DomainEvent(AccountId);
public record MoneyTransferred(Guid AccountId, decimal Amount, Guid ToAccountId) : DomainEvent(AccountId);
public record AccountClosed(Guid AccountId) : DomainEvent(AccountId);
