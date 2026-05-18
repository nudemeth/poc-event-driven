using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using AccountOutboxPublisher;

// The function handler that will be called when the Lambda is triggered
var handler = async (JsonElement input, ILambdaContext context) =>
{
    // Configure dependency injection
    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", optional: true)
        .Build();

    var services = new ServiceCollection()
        .ConfigureOutboxPublisherServices(configuration, context);

    var serviceProvider = services.BuildServiceProvider();

    var triggerSource = IsDynamoDbStreamEvent(input) ? "DynamoDB Stream (CDC)" : "EventBridge schedule";
    context.Logger.LogInformation($"Outbox Publisher Lambda triggered by {triggerSource}.");

    try
    {
        var outboxPublisher = serviceProvider.GetRequiredService<OutboxPublisher>();
        await outboxPublisher.PublishUnpublishedItemsAsync();
        context.Logger.LogInformation("Outbox Publisher Lambda execution completed successfully.");
    }
    catch (Exception ex)
    {
        context.Logger.LogError($"Outbox Publisher Lambda failed with error: {ex.Message}");
        throw;
    }
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();

static bool IsDynamoDbStreamEvent(JsonElement input) =>
    input.ValueKind == JsonValueKind.Object &&
    input.TryGetProperty("Records", out var records) &&
    records.ValueKind == JsonValueKind.Array &&
    records.GetArrayLength() > 0 &&
    records[0].TryGetProperty("dynamodb", out _);
