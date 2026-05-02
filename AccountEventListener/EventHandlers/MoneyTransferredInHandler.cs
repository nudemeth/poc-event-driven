using AccountProjection;
using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AccountEventListener.EventHandlers;

public class MoneyTransferredInHandler : INotificationHandler<MoneyTransferredIn>
{
    private readonly ILambdaContext _context;
    private readonly AccountProjectionDbContext _dbContext;

    public MoneyTransferredInHandler(ILambdaContext context, AccountProjectionDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async ValueTask Handle(MoneyTransferredIn notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyTransferredIn event");
        _context.Logger.LogInformation($"Target Account ID: {notification.AccountId}, Source Account ID: {notification.FromAccountId}, Amount: {notification.Amount}");

        try
        {
            // Update target account
            var targetAccount = await _dbContext.AccountSummaryProjections.FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken: cancellationToken);

            if (targetAccount == null)
            {
                _context.Logger.LogWarning($"Target account {notification.AccountId} not found in read-side database");
                return;
            }

            if (notification.Version != targetAccount.Version + 1)
            {
                throw new InvalidOperationException($"Out-of-order event: expected version {targetAccount.Version + 1} but got {notification.Version} for account {notification.AccountId}");
            }

            targetAccount.Balance += notification.Amount;
            targetAccount.Version = notification.Version;
            _dbContext.AccountSummaryProjections.Update(targetAccount);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _context.Logger.LogInformation($"Transfer in completed: {notification.AccountId}, Amount: {notification.Amount}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _context.Logger.LogError($"Concurrency conflict when updating projection for account {notification.AccountId}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"Error handling MoneyTransferredIn event: {ex.Message}");
            throw;
        }
    }
}
