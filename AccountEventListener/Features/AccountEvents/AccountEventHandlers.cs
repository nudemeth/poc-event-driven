using Amazon.Lambda.Core;
using Mediator;

namespace AccountEventListener.Features.AccountEvents;

public class AccountOpenedHandler : INotificationHandler<AccountOpenedNotification>
{
    private readonly ILambdaContext _context;

    public AccountOpenedHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(AccountOpenedNotification notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling AccountOpened event");
        _context.Logger.LogInformation($"Account data: {string.Join(", ", notification.EventData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        // Add your AccountOpened processing logic here

        return ValueTask.CompletedTask;
    }
}

public class AccountClosedHandler : INotificationHandler<AccountClosedNotification>
{
    private readonly ILambdaContext _context;

    public AccountClosedHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(AccountClosedNotification notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling AccountClosed event");
        _context.Logger.LogInformation($"Account data: {string.Join(", ", notification.EventData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        // Add your AccountClosed processing logic here

        return ValueTask.CompletedTask;
    }
}

public class MoneyDepositedHandler : INotificationHandler<MoneyDepositedNotification>
{
    private readonly ILambdaContext _context;

    public MoneyDepositedHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(MoneyDepositedNotification notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyDeposited event");
        _context.Logger.LogInformation($"Deposit data: {string.Join(", ", notification.EventData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        // Add your MoneyDeposited processing logic here

        return ValueTask.CompletedTask;
    }
}

public class MoneyWithdrawnHandler : INotificationHandler<MoneyWithdrawnNotification>
{
    private readonly ILambdaContext _context;

    public MoneyWithdrawnHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(MoneyWithdrawnNotification notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyWithdrawn event");
        _context.Logger.LogInformation($"Withdrawal data: {string.Join(", ", notification.EventData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        // Add your MoneyWithdrawn processing logic here

        return ValueTask.CompletedTask;
    }
}

public class MoneyTransferredHandler : INotificationHandler<MoneyTransferredNotification>
{
    private readonly ILambdaContext _context;

    public MoneyTransferredHandler(ILambdaContext context)
    {
        _context = context;
    }

    public ValueTask Handle(MoneyTransferredNotification notification, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("Handling MoneyTransferred event");
        _context.Logger.LogInformation($"Transfer data: {string.Join(", ", notification.EventData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        // Add your MoneyTransferred processing logic here

        return ValueTask.CompletedTask;
    }
}
