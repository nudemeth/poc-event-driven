using AccountDataAccess;
using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyTransferredHandler : INotificationHandler<MoneyTransferred>
{
    private readonly ILambdaContext _context;
    private readonly AccountDbContext _dbContext;

    public MoneyTransferredHandler(ILambdaContext context, AccountDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async ValueTask Handle(MoneyTransferred notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyTransferred event");
        _context.Logger.LogInformation($"From Account ID: {notification.AccountId}, To Account ID: {notification.ToAccountId}, Amount: {notification.Amount}");

        try
        {
            // Update source account
            var sourceAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken: cancellationToken);

            if (sourceAccount == null)
            {
                _context.Logger.LogWarning($"Source account {notification.AccountId} not found in read-side database");
                return;
            }

            sourceAccount.Balance -= notification.Amount;
            _dbContext.Accounts.Update(sourceAccount);

            // Update target account
            var targetAccount = await _dbContext.Accounts.FindAsync(new object[] { notification.ToAccountId }, cancellationToken: cancellationToken);

            if (targetAccount == null)
            {
                _context.Logger.LogWarning($"Target account {notification.ToAccountId} not found in read-side database");
                return;
            }

            targetAccount.Balance += notification.Amount;
            _dbContext.Accounts.Update(targetAccount);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _context.Logger.LogInformation($"Transfer completed: {notification.AccountId} -> {notification.ToAccountId}, Amount: {notification.Amount}");
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"Error handling MoneyTransferred event: {ex.Message}");
            throw;
        }
    }
}
