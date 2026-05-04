using AccountProjection;
using Amazon.Lambda.Core;
using Domain;
using Domain.Account;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AccountEventListener.Decorators;

public class AccountValidationDecorator<TNotification>(
    INotificationHandler<TNotification> inner,
    ILambdaContext context,
    AccountProjectionDbContext dbContext,
    EventContext notificationContext) : INotificationHandler<TNotification>
    where TNotification : DomainEvent
{
    public async ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation($"Handling {typeof(TNotification).Name} event");

        try
        {
            var account = await dbContext.AccountSummaryProjections
                .FirstOrDefaultAsync(a => a.Id == notification.StreamId);

            context.Logger.LogInformation($"Found account {account?.Id} for event {typeof(TNotification).Name}");

            if (account == null && notification is not AccountOpened)
            {
                throw new InvalidOperationException($"Account {notification.StreamId} not found in read-side database");
            }

            if (account != null && notification.Version <= account.Version)
            {
                context.Logger.LogWarning($"Duplicate business event skipped: {typeof(TNotification).Name} v{notification.Version} already applied to account {notification.StreamId} with current version {account.Version}");
                return;
            }

            var expectedVersion = account?.Version + 1 ?? 1;
            if (notification.Version != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Out-of-order event: expected version {expectedVersion} but got {notification.Version} for account {notification.StreamId} on event {typeof(TNotification).Name}");
            }

            if (account is not null)
            {
                notificationContext.Account = account;
            }
            await inner.Handle(notification, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            context.Logger.LogInformation($"Account {notification.StreamId} projection updated successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                $"Concurrency conflict when updating projection for account {notification.StreamId}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error handling {typeof(TNotification).Name} event for account {notification.StreamId}", ex);
        }
    }
}
