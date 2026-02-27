using AccountDataAccess;
using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AccountEventListener.EventHandlers;

public class AccountClosedHandler : INotificationHandler<AccountClosed>
{
    private readonly ILambdaContext _context;
    private readonly AccountDbContext _dbContext;

    public AccountClosedHandler(ILambdaContext context, AccountDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async ValueTask Handle(AccountClosed notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling AccountClosed event");
        _context.Logger.LogInformation($"Account ID: {notification.AccountId}");

        try
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken: cancellationToken);
            if (account == null)
            {
                _context.Logger.LogWarning($"Account {notification.AccountId} not found in read-side database");
                return;
            }

            account.IsActive = false;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _context.Logger.LogInformation($"Account {notification.AccountId} marked as closed in read-side database");
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"Error handling AccountClosed event: {ex.Message}");
            throw;
        }
    }
}
