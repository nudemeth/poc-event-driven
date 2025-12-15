using Mediator;
using Domain.Account;

namespace Application.Features;

public class CreateAccountHandler : ICommandHandler<CreateAccountCommand, AccountEntity>
{
    public ValueTask<AccountEntity> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        // Handler creates a new AccountEntity and returns it.
        var account = AccountEntity.Open(command.AccountHolder, command.InitialDeposit);
        StaticDb.Accounts.Add(account);
        return ValueTask.FromResult(account);
    }
}
