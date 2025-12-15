using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Domain.Account;

namespace Application.Features;

public class DepositHandler : ICommandHandler<DepositCommand, AccountEntity>
{
    public ValueTask<AccountEntity> Handle(DepositCommand command, CancellationToken cancellationToken)
    {
        var account = StaticDb.Accounts.FirstOrDefault(a => a.Id == command.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        try
        {
            account.Deposit(command.Amount);
            return ValueTask.FromResult(account);
        }
        catch (Exception ex)
        {
            throw new Exception("Deposit failed: " + ex.Message);
        }
    }
}
