using Domain.Account;
using Mediator;

namespace Application.Features;

public record DepositCommand(Guid Id, decimal Amount) : ICommand<AccountEntity>;
