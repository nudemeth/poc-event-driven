using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyDepositedHandler(EventContext notificationContext, ILambdaContext context) : INotificationHandler<MoneyDeposited>
{
    public ValueTask Handle(MoneyDeposited notification, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation($"Handling Event: {typeof(MoneyDeposited).Name}, Account ID: {notification.AccountId}, Amount: {notification.Amount}");
        notificationContext.Account.Balance += notification.Amount;
        notificationContext.Account.Version = notification.Version;
        return ValueTask.CompletedTask;
    }
}
