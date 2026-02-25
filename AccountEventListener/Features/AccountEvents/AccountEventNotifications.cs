using Domain.Account;
using Mediator;

namespace AccountEventListener.Features.AccountEvents;

public record AccountOpenedNotification(AccountOpened EventData) : INotification;

public record AccountClosedNotification(AccountClosed EventData) : INotification;

public record MoneyDepositedNotification(MoneyDeposited EventData) : INotification;

public record MoneyWithdrawnNotification(MoneyWithdrawn EventData) : INotification;

public record MoneyTransferredNotification(MoneyTransferred EventData) : INotification;
