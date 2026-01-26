namespace Domain.Account;

public record MoneyWithdrawn(Guid AccountId, decimal Amount) : DomainEvent(AccountId, nameof(MoneyWithdrawn));
