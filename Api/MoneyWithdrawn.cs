public record MoneyWithdrawn(Guid AccountId, decimal Amount) : DomainEvent(AccountId);
