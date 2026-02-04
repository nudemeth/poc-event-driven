namespace Api.Requests;

public record TransferRequest(Guid ToAccountNumber, decimal Amount);
