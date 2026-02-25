using Mediator;
using AccountEventListener.Features.AccountEvents;
using Domain.Account;
using Domain;

namespace AccountEventListener;

public static class EventTypeFactory
{
    public static INotification? CreateNotification(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            AccountOpened accountOpenedEvent => new AccountOpenedNotification(accountOpenedEvent),
            AccountClosed accountClosedEvent => new AccountClosedNotification(accountClosedEvent),
            MoneyDeposited moneyDepositedEvent => new MoneyDepositedNotification(moneyDepositedEvent),
            MoneyWithdrawn moneyWithdrawnEvent => new MoneyWithdrawnNotification(moneyWithdrawnEvent),
            MoneyTransferred moneyTransferredEvent => new MoneyTransferredNotification(moneyTransferredEvent),
            _ => null
        };
    }
}
