using Mediator;
using Domain.Account;

namespace Application.Features;

public class WithdrawHandler : ICommandHandler<WithdrawCommand, AccountEntity>
{
    private readonly IAccountRepository _accountRepository;

    public WithdrawHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async ValueTask<AccountEntity> Handle(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(command.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        try
        {
            account.Withdraw(command.Amount);
            await _accountRepository.SaveAsync(account);
            return account;
        }
        catch (Exception ex)
        {
            throw new Exception("Withdrawal failed: " + ex.Message);
        }
    }
}
