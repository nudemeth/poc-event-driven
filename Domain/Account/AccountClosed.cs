namespace Domain.Account;

public record AccountClosed(Guid AccountId) : DomainEvent(AccountId, nameof(AccountClosed));
