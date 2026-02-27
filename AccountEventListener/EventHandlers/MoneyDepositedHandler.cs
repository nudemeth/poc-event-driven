using AccountDataAccess;
using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyDepositedHandler : INotificationHandler<MoneyDeposited>
{
    private readonly ILambdaContext _context;
    private readonly AccountDbContext _dbContext;

    public MoneyDepositedHandler(ILambdaContext context, AccountDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async ValueTask Handle(MoneyDeposited notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyDeposited event");
        _context.Logger.LogInformation($"Account ID: {notification.AccountId}, Amount: {notification.Amount}");

        try
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken: cancellationToken);

            if (account == null)
            {
                _context.Logger.LogWarning($"Account {notification.AccountId} not found in read-side database");
                return;
            }

            account.Balance += notification.Amount;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _context.Logger.LogInformation($"Account {notification.AccountId} balance updated to {account.Balance} in read-side database");
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"Error handling MoneyDeposited event: {ex.Message}");
            throw;
        }
    }
}
