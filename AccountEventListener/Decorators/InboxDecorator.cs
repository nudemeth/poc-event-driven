using Amazon.Lambda.Core;
using Domain;
using Mediator;

namespace AccountEventListener.Decorators;

public class InboxDecorator<TNotification>(
    INotificationHandler<TNotification> inner,
    ILambdaContext context,
    InboxRepository inboxRepository,
    EventContext eventContext) : INotificationHandler<TNotification>
    where TNotification : DomainEvent
{
    public async ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        var item = new InboxRepository.InboxItem
        {
            MessageId = eventContext.MessageId,
            EventType = notification.GetType().Name,
            Payload = eventContext.Body,
            ReceiveCount = eventContext.ReceiveCount
        };

        var created = await inboxRepository.TryCreateAsync(item, cancellationToken);

        if (!created)
        {
            var existing = await inboxRepository.GetInboxItemAsync(eventContext.MessageId, cancellationToken);

            if (existing.Payload != eventContext.Body)
            {
                throw new InvalidOperationException($"Message '{eventContext.MessageId}' already exists with a different payload.");
            }

            if (existing.IsProcessed)
            {
                context.Logger.LogWarning($"Duplicate message skipped. Message ID: {eventContext.MessageId}");
                return;
            }

            var canProcess = await inboxRepository.TryUpdateReceiveCountAsync(eventContext.MessageId, eventContext.ReceiveCount, cancellationToken);

            if (!canProcess)
            {
                context.Logger.LogWarning($"Duplicate message skipped. Message ID: {eventContext.MessageId}");
                return;
            }
        }

        await inner.Handle(notification, cancellationToken);
        await inboxRepository.MarkProcessedAsync(eventContext.MessageId, cancellationToken);
    }
}
