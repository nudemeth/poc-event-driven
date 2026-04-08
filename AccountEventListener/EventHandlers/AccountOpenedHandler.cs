using AccountProjection;
using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AccountEventListener.EventHandlers;

public class AccountOpenedHandler : INotificationHandler<AccountOpened>
{
    private readonly ILambdaContext _context;
    private readonly AccountProjectionDbContext _dbContext;

    public AccountOpenedHandler(ILambdaContext context, AccountProjectionDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async ValueTask Handle(AccountOpened notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling AccountOpened event");
        _context.Logger.LogInformation($"Account ID: {notification.AccountId}, Holder: {notification.AccountHolder}, Initial Deposit: {notification.InitialDeposit}");

        try
        {
            // Create new account entity for read-side projection
            var account = new AccountSummaryProjection
            {
                Id = notification.AccountId,
                AccountHolder = notification.AccountHolder,
                Balance = notification.InitialDeposit,
                IsActive = true,
                Version = 0
            };

            _dbContext.AccountSummaryProjections.Add(account);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _context.Logger.LogInformation($"Account {notification.AccountId} projection created in read-side database");
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"Error handling AccountOpened event: {ex.Message}");
            throw;
        }
    }
}
