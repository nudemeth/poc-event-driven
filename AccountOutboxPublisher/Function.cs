using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccountOutboxPublisher;

// The function handler that will be called when the Lambda is triggered
var handler = async (object? input, ILambdaContext context) =>
{
    // Configure dependency injection
    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    var services = new ServiceCollection()
        .ConfigureOutboxPublisherServices(configuration);

    var serviceProvider = services.BuildServiceProvider();

    context.Logger.LogInformation("Outbox Publisher Lambda triggered.");

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
