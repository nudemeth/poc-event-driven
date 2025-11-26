namespace Domain.Account;

public record MoneyTransferred(Guid AccountId, decimal Amount, Guid ToAccountId) : DomainEvent(AccountId);
