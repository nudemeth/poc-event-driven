using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyTransferredInHandler(EventContext notificationContext, ILambdaContext context) : INotificationHandler<MoneyTransferredIn>
{
    public ValueTask Handle(MoneyTransferredIn notification, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation($"Handling Event: {typeof(MoneyTransferredIn).Name}, Target Account ID: {notification.AccountId}, Source Account ID: {notification.FromAccountId}, Amount: {notification.Amount}");
        notificationContext.Account.Balance += notification.Amount;
        notificationContext.Account.Version = notification.Version;
        return ValueTask.CompletedTask;
    }
}
