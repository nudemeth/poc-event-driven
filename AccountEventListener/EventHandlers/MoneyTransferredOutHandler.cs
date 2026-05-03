using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyTransferredOutHandler(EventContext notificationContext, ILambdaContext context) : INotificationHandler<MoneyTransferredOut>
{
    public ValueTask Handle(MoneyTransferredOut notification, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation($"Handling Event: {typeof(MoneyTransferredOut).Name}, Source Account ID: {notification.AccountId}, Target Account ID: {notification.ToAccountId}, Amount: {notification.Amount}");
        notificationContext.Account.Balance -= notification.Amount;
        notificationContext.Account.Version = notification.Version;
        return ValueTask.CompletedTask;
    }
}
