namespace Domain.Account;

public record MoneyTransferredOut(Guid AccountId, decimal Amount, Guid ToAccountId) : DomainEvent(AccountId);
