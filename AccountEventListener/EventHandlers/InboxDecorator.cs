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
        var item = new InboxRepository.InboxItem
        {
            MessageId = inboxContext.MessageId,
            EventType = notification.GetType().Name,
            Payload = inboxContext.Body,
            ReceiveCount = inboxContext.ReceiveCount
        };

        var created = await inboxRepository.TryCreateAsync(item, cancellationToken);

        if (!created)
        {
            var existing = await inboxRepository.GetInboxItemAsync(inboxContext.MessageId, cancellationToken);

            if (existing.Payload != inboxContext.Body)
            {
                throw new InvalidOperationException($"Message '{inboxContext.MessageId}' already exists with a different payload.");
            }

            if (existing.IsProcessed)
            {
                context.Logger.LogWarning($"Duplicate message skipped. Message ID: {inboxContext.MessageId}");
                return;
            }

            var canProcess = await inboxRepository.TryUpdateReceiveCountAsync(inboxContext.MessageId, inboxContext.ReceiveCount, cancellationToken);

            if (!canProcess)
            {
                context.Logger.LogWarning($"Duplicate message skipped. Message ID: {inboxContext.MessageId}");
                return;
            }
        }

        await inner.Handle(notification, cancellationToken);
        await inboxRepository.MarkProcessedAsync(inboxContext.MessageId, cancellationToken);
    }
}
