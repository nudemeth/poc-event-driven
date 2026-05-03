using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Mediator;
using AccountEventListener;
using System.Text.Json;
using Domain;

// The function handler that will be called for each Lambda event
var handler = async (SQSEvent @event, ILambdaContext context) =>
{
    // Configure dependency injection
    var services = new ServiceCollection()
        .ConfigureEventListenerServices(context);
    var serviceProvider = services.BuildServiceProvider();
    var failedMessageIds = new List<string>();

    context.Logger.LogInformation("Processing SQS event with the following records:");

    foreach (var record in @event.Records)
    {
        // Use the outbox MessageId (forwarded as an SNS message attribute) so the inbox
        // key matches the same ID stored in AccountsOutbox, not the SQS-assigned one.
        record.MessageAttributes.TryGetValue("MessageId", out var messageIdAttr);
        var messageId = messageIdAttr?.StringValue;

        if (string.IsNullOrEmpty(messageId))
        {
            context.Logger.LogWarning($"MessageId attribute is missing or empty for SQS message ID: {record.MessageId}");
            continue;
        }

        context.Logger.LogInformation($"Processing Message ID: {messageId}, SQS Message ID: {record.MessageId}");

        var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var domainEvent = TryDeserialize(record.Body);

        if (domainEvent == null)
        {
            context.Logger.LogWarning($"Failed to deserialize domain event from message ID: {messageId}");
            continue;
        }

        var classifiedEvent = EventTypeFactory.Classify(domainEvent);

        if (classifiedEvent == null)
        {
            context.Logger.LogWarning($"No handler found for event type: {domainEvent.GetType().Name}");
            continue;
        }

        try
        {
            var inboxRepository = scope.ServiceProvider.GetRequiredService<InboxRepository>();
            record.Attributes.TryGetValue("ApproximateReceiveCount", out var receiveCountAttr);
            var receiveCount = int.TryParse(receiveCountAttr, out var count) ? count : 1;
            var isNew = await inboxRepository.TryRecordAsync(messageId, domainEvent.GetType().Name, record.Body, receiveCount);

            if (!isNew)
            {
                context.Logger.LogWarning($"Duplicate message skipped. Message ID: {messageId}, SQS message ID: {record.MessageId}");
                continue;
            }

            context.Logger.LogInformation($"Publishing notification for event type: {classifiedEvent.GetType().Name}");

            await mediator.Publish(classifiedEvent);
            await inboxRepository.MarkProcessedAsync(messageId);

            context.Logger.LogInformation($"Successfully processed Message ID: {messageId}, SQS message ID: {record.MessageId}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, $"Error processing Message ID: {messageId}, SQS message ID {record.MessageId}: {ex.Message}");
            failedMessageIds.Add(record.MessageId);
        }
    }

    context.Logger.LogInformation("SQS event processing complete.");

    // Return failed message IDs so SQS will retry them
    return new SQSBatchResponse
    {
        BatchItemFailures = failedMessageIds
            .Select(id => new SQSBatchResponse.BatchItemFailure { ItemIdentifier = id })
            .ToList()
    };
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();

static DomainEvent? TryDeserialize(string body)
{
    try
    {
        return JsonSerializer.Deserialize<DomainEvent>(body, DomainEventJsonOptions.Instance);
    }
    catch
    {
        return null;
    }
}