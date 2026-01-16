using Mediator;
using Domain.Account;

namespace Application.Features;

public class CreateAccountHandler : ICommandHandler<CreateAccountCommand, AccountEntity>
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<AccountEntity> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        // Handler creates a new AccountEntity and returns it.
        var account = AccountEntity.Open(command.AccountHolder, command.InitialDeposit);
        await _accountRepository.AppendAsync(account.Events[0]);
        return account;
    }
}
