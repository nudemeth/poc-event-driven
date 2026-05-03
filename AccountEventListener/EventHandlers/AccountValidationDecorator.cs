using AccountProjection;
using Amazon.Lambda.Core;
using Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AccountEventListener.EventHandlers;

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

            if (account == null)
            {
                throw new InvalidOperationException($"Account {notification.StreamId} not found for in read-side database");
            }

            if (notification.Version != account.Version + 1)
            {
                throw new InvalidOperationException(
                    $"Out-of-order event: expected version {account.Version + 1} but got {notification.Version} for account {notification.StreamId} on event {typeof(TNotification).Name}");
            }

            notificationContext.Account = account;
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
