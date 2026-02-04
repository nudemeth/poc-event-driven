using Mediator;

public record DeleteAccountCommand(Guid Id) : ICommand;