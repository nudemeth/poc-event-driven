using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class AccountClosedHandler : INotificationHandler<AccountClosed>
{
    private readonly ILambdaContext _context;

    public AccountClosedHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(AccountClosed notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling AccountClosed event");
        _context.Logger.LogInformation($"Account data: {notification.EventData}");

        // Add your AccountClosed processing logic here

        return ValueTask.CompletedTask;
    }
}
