using Amazon.Lambda.Core;
using Domain;
using Mediator;

namespace AccountEventListener.EventHandlers;

public class InboxDecorator<TNotification>(
    INotificationHandler<TNotification> inner,
    ILambdaContext context,
    InboxRepository inboxRepository,
    InboxContext inboxContext) : INotificationHandler<TNotification>
    where TNotification : DomainEvent
{
    public async ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        var isNew = await inboxRepository.TryRecordAsync(
            inboxContext.MessageId,
            notification.GetType().Name,
            inboxContext.Body,
            inboxContext.ReceiveCount,
            cancellationToken);

        if (!isNew)
        {
            context.Logger.LogWarning($"Duplicate message skipped. Message ID: {inboxContext.MessageId}");
            return;
        }

        await inner.Handle(notification, cancellationToken);
        await inboxRepository.MarkProcessedAsync(inboxContext.MessageId, cancellationToken);
    }
}
