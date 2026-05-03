using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyWithdrawnHandler(EventContext notificationContext, ILambdaContext context) : INotificationHandler<MoneyWithdrawn>
{
    public ValueTask Handle(MoneyWithdrawn notification, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation($"Handling Event: {typeof(MoneyWithdrawn).Name}, Account ID: {notification.AccountId}, Amount: {notification.Amount}");
        notificationContext.Account.Balance -= notification.Amount;
        notificationContext.Account.Version = notification.Version;
        return ValueTask.CompletedTask;
    }
}
