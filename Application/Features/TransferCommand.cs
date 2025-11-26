namespace Application.Features;

public record TransferCommand(Guid ToAccountNumber, decimal Amount);