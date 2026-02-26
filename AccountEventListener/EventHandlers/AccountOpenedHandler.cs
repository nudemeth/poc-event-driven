using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class AccountOpenedHandler : INotificationHandler<AccountOpened>
{
    private readonly ILambdaContext _context;

    public AccountOpenedHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(AccountOpened notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling AccountOpened event");
        _context.Logger.LogInformation($"Account data: {notification.EventData}");

        // Add your AccountOpened processing logic here

        return ValueTask.CompletedTask;
    }
}
