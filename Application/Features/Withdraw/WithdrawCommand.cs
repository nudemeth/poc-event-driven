using Domain.Account;
using Mediator;

namespace Application.Features;

public record WithdrawCommand(Guid Id, decimal Amount) : ICommand<AccountEntity>;
