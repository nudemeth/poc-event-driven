using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Mediator;
using AccountEventListener;
using System.Text.Json;
using Domain;

// The function handler that will be called for each Lambda event
var handler = async (DynamoDBEvent @event, ILambdaContext context) =>
{
    // Configure dependency injection
    var services = new ServiceCollection()
        .ConfigureEventListenerServices(context);
    var serviceProvider = services.BuildServiceProvider();

    context.Logger.LogInformation("Processing DynamoDB event with the following records:");

    foreach (var record in @event.Records)
    {
        context.Logger.LogInformation($"Event ID: {record.EventID}");
        context.Logger.LogInformation($"Event Name: {record.EventName}");

        var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (record.Dynamodb?.NewImage == null)
        {
            context.Logger.LogWarning($"No NewImage found for event ID: {record.EventID}");
            continue;
        }

        var jsonDocument = record.Dynamodb.NewImage.ToJson();
        var domainEvent = JsonSerializer.Deserialize<DomainEvent>(jsonDocument, DomainEventJsonOptions.Instance);

        if (domainEvent == null)
        {
            context.Logger.LogWarning($"Failed to deserialize event data for event ID: {record.EventID}");
            continue;
        }

        var notification = EventTypeFactory.CreateNotification(domainEvent);

        if (notification == null)
        {
            context.Logger.LogWarning($"No handler found for event type: {record.EventName}");
            continue;
        }

        context.Logger.LogInformation($"Publishing notification for event type: {record.EventName}");
        await mediator.Publish(notification);
    }

    context.Logger.LogInformation("DynamoDB event processing complete.");
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();