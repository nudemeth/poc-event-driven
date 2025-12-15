using Domain.Account;
using Mediator;

namespace Application.Features;

public record TransferCommand(Guid Id, Guid ToAccountNumber, decimal Amount) : ICommand<AccountEntity>;
