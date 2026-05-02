using AccountProjection;
using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AccountEventListener.EventHandlers;

public class MoneyTransferredOutHandler : INotificationHandler<MoneyTransferredOut>
{
    private readonly ILambdaContext _context;
    private readonly AccountProjectionDbContext _dbContext;

    public MoneyTransferredOutHandler(ILambdaContext context, AccountProjectionDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async ValueTask Handle(MoneyTransferredOut notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyTransferredOut event");
        _context.Logger.LogInformation($"Source Account ID: {notification.AccountId}, Target Account ID: {notification.ToAccountId}, Amount: {notification.Amount}");

        try
        {
            // Update source account
            var sourceAccount = await _dbContext.AccountSummaryProjections.FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken: cancellationToken);

            if (sourceAccount == null)
            {
                _context.Logger.LogWarning($"Source account {notification.AccountId} not found in read-side database");
                return;
            }

            if (notification.Version != sourceAccount.Version + 1)
            {
                throw new InvalidOperationException($"Out-of-order event: expected version {sourceAccount.Version + 1} but got {notification.Version} for account {notification.AccountId}");
            }

            sourceAccount.Balance -= notification.Amount;
            sourceAccount.Version = notification.Version;
            _dbContext.AccountSummaryProjections.Update(sourceAccount);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _context.Logger.LogInformation($"Transfer out completed: {notification.AccountId}, Amount: {notification.Amount}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _context.Logger.LogError($"Concurrency conflict when updating projection for account {notification.AccountId}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"Error handling MoneyTransferredOut event: {ex.Message}");
            throw;
        }
    }
}
