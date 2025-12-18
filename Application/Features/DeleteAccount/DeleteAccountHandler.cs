using Mediator;

public class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand>
{
    public ValueTask<Unit> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        var account = StaticDb.Accounts.FirstOrDefault(a => a.Id == command.Id);
        if (account == null)
        {
            throw new Exception("Account not found.");
        }

        try
        {
            account.Close();
            return ValueTask.FromResult(Unit.Value);
        }
        catch (Exception ex)
        {
            throw new Exception("Account closure failed: " + ex.Message);
        }
    }
}