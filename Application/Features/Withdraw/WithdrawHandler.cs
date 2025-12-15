using Mediator;
using Domain.Account;

namespace Application.Features;

public class WithdrawHandler : ICommandHandler<WithdrawCommand, AccountEntity>
{
    public ValueTask<AccountEntity> Handle(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var account = StaticDb.Accounts.FirstOrDefault(a => a.Id == command.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        try
        {
            account.Withdraw(command.Amount);
            return ValueTask.FromResult(account);
        }
        catch (Exception ex)
        {
            throw new Exception("Withdrawal failed: " + ex.Message);
        }
    }
}
