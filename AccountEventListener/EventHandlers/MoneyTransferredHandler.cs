using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyTransferredHandler : INotificationHandler<MoneyTransferred>
{
    private readonly ILambdaContext _context;

    public MoneyTransferredHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(MoneyTransferred notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyTransferred event");
        _context.Logger.LogInformation($"Transfer data: {notification.EventData}");

        // Add your MoneyTransferred processing logic here

        return ValueTask.CompletedTask;
    }
}
