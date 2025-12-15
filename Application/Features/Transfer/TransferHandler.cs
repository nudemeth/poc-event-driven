using Mediator;
using Domain.Account;

namespace Application.Features;

public class TransferHandler : ICommandHandler<TransferCommand, AccountEntity>
{
    public ValueTask<AccountEntity> Handle(TransferCommand command, CancellationToken cancellationToken)
    {
        var account = StaticDb.Accounts.FirstOrDefault(a => a.Id == command.Id);
        var toAccount = StaticDb.Accounts.FirstOrDefault(a => a.Id == command.ToAccountNumber);
        if (account == null || toAccount == null)
        {
            throw new Exception("One or both accounts not found.");
        }

        try
        {
            account.Transfer(command.ToAccountNumber, command.Amount);
            return ValueTask.FromResult(account);
        }
        catch (Exception ex)
        {
            throw new Exception("Transfer failed: " + ex.Message);
        }
    }
}
