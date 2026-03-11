namespace Domain.Account;

public record MoneyTransferredIn(Guid AccountId, Guid FromAccountId, decimal Amount) : DomainEvent(AccountId);
