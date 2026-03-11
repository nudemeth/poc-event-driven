using Mediator;
using Domain.Account;
using Domain;

namespace AccountEventListener;

public static class EventTypeFactory
{
    public static INotification? CreateNotification(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            AccountOpened accountOpenedEvent => accountOpenedEvent,
            AccountClosed accountClosedEvent => accountClosedEvent,
            MoneyDeposited moneyDepositedEvent => moneyDepositedEvent,
            MoneyWithdrawn moneyWithdrawnEvent => moneyWithdrawnEvent,
            MoneyTransferredOut moneyTransferredOutEvent => moneyTransferredOutEvent,
            MoneyTransferredIn moneyTransferredInEvent => moneyTransferredInEvent,
            _ => null
        };
    }
}
