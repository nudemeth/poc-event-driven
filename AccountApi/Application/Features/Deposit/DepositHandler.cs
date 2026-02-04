using Mediator;
using Domain.Account;

namespace Application.Features;

public class DepositHandler : ICommandHandler<DepositCommand, AccountEntity>
{
    private readonly IAccountRepository _accountRepository;

    public DepositHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<AccountEntity> Handle(DepositCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(command.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        try
        {
            account.Deposit(command.Amount);
            await _accountRepository.SaveAsync(account);
            return account;
        }
        catch (Exception ex)
        {
            throw new Exception("Deposit failed: " + ex.Message);
        }
    }
}
