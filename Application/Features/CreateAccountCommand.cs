namespace Application.Features;

public record CreateAccountCommand(string AccountHolder, decimal InitialDeposit);