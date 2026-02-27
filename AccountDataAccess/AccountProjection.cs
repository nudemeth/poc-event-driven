namespace AccountDataAccess;

public record AccountProjection(Guid Id, string AccountHolder, decimal Balance, bool IsActive);