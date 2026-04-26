using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Mediator;
using AccountEventListener;
using System.Text.Json;
using Domain;

// The function handler that will be called for each Lambda event
var handler = async (SNSEvent @event, ILambdaContext context) =>
{
    // Configure dependency injection
    var services = new ServiceCollection()
        .ConfigureEventListenerServices(context);
    var serviceProvider = services.BuildServiceProvider();

    context.Logger.LogInformation("Processing SNS event with the following records:");

    foreach (var record in @event.Records)
    {
        context.Logger.LogInformation($"Received Message ID: {record.Sns.MessageId}, Subject: {record.Sns.Subject}");

        var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Extract EventType from message attributes
        var eventType = GetMessageAttribute(record.Sns.MessageAttributes, "EventType");

        if (string.IsNullOrWhiteSpace(eventType))
        {
            context.Logger.LogWarning($"EventType attribute not found in SNS message ID: {record.Sns.MessageId}");
            continue;
        }

        context.Logger.LogInformation($"Event type extracted from SNS message: {eventType}");

        // Parse domain event from SNS message body
        var domainEvent = JsonSerializer.Deserialize<DomainEvent>(record.Sns.Message, DomainEventJsonOptions.Instance);

        if (domainEvent == null)
        {
            context.Logger.LogWarning($"Failed to deserialize event data from SNS message ID: {record.Sns.MessageId}");
            continue;
        }

        var notification = EventTypeFactory.CreateNotification(domainEvent);

        if (notification == null)
        {
            context.Logger.LogWarning($"No handler found for event type: {eventType}");
            continue;
        }

        context.Logger.LogInformation($"Publishing notification for event type: {notification.GetType().Name}");
        await mediator.Publish(notification);
    }

    context.Logger.LogInformation("SNS event processing complete.");
};

// Helper method to extract message attribute value
static string? GetMessageAttribute(IDictionary<string, SNSEvent.MessageAttribute> attributes, string key)
{
    return attributes.TryGetValue(key, out var attribute) ? attribute.Value : null;
}

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();