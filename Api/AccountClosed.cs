public record AccountClosed(Guid AccountId) : DomainEvent(AccountId);
