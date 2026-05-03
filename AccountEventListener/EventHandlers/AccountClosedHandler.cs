using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class AccountClosedHandler(EventContext notificationContext, ILambdaContext context) : INotificationHandler<AccountClosed>
{
    public ValueTask Handle(AccountClosed notification, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation($"Handling Event: {typeof(AccountClosed).Name}, Account ID: {notification.AccountId}");
        notificationContext.Account.IsActive = false;
        notificationContext.Account.Version = notification.Version;
        return ValueTask.CompletedTask;
    }
}
