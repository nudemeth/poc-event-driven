namespace Domain.Account;

public record AccountOpened(Guid AccountId, string AccountHolder, decimal InitialDeposit) : DomainEvent(AccountId, nameof(AccountOpened));
