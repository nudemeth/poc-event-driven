using Domain.Account;
using Mediator;

namespace Application.Features;

public record CreateAccountCommand(string AccountHolder, decimal InitialDeposit) : ICommand<AccountEntity>;
