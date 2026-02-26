using Amazon.Lambda.Core;
using Domain.Account;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class MoneyDepositedHandler : INotificationHandler<MoneyDeposited>
{
    private readonly ILambdaContext _context;

    public MoneyDepositedHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(MoneyDeposited notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyDeposited event");
        _context.Logger.LogInformation($"Deposit data: {notification.EventData}");

        // Add your MoneyDeposited processing logic here

        return ValueTask.CompletedTask;
    }
}
