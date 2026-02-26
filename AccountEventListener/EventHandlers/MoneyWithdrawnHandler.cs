using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyWithdrawnHandler : INotificationHandler<MoneyWithdrawn>
{
    private readonly ILambdaContext _context;

    public MoneyWithdrawnHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(MoneyWithdrawn notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyWithdrawn event");
        _context.Logger.LogInformation($"Withdrawal data: {notification.EventData}");

        // Add your MoneyWithdrawn processing logic here

        return ValueTask.CompletedTask;
    }
}
