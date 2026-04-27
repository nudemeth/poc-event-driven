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

    context.Logger.LogInformation("Processing SQS event with the following records:");

    var failedMessageIds = new List<string>();

    foreach (var record in @event.Records)
    {
        try
        {
            context.Logger.LogInformation($"Processing SQS Message ID: {record.MessageId}");

            var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Parse domain event directly from SQS body (raw SNS message delivery)
            var domainEvent = JsonSerializer.Deserialize<DomainEvent>(record.Body, DomainEventJsonOptions.Instance);

            if (domainEvent == null)
            {
                context.Logger.LogWarning($"Failed to deserialize domain event from SQS message ID: {record.MessageId}");
                failedMessageIds.Add(record.MessageId);
                continue;
            }

            var notification = EventTypeFactory.CreateNotification(domainEvent);

            if (notification == null)
            {
                context.Logger.LogWarning($"No handler found for event type: {domainEvent.GetType().Name}");
                failedMessageIds.Add(record.MessageId);
                continue;
            }

            context.Logger.LogInformation($"Publishing notification for event type: {notification.GetType().Name}");
            await mediator.Publish(notification);

            context.Logger.LogInformation($"Successfully processed SQS message ID: {record.MessageId}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing SQS message ID {record.MessageId}: {ex.Message}");
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