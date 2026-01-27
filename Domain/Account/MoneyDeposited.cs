namespace Domain.Account;

public record MoneyDeposited(Guid AccountId, decimal Amount) : DomainEvent(AccountId);
