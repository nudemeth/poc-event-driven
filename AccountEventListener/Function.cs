using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

// The function handler that will be called for each Lambda event
var handler = async (DynamoDBEvent @event, ILambdaContext context) =>
{
    context.Logger.LogInformation("Processing DynamoDB event with the following records:");

    foreach (var record in @event.Records)
    {
        context.Logger.LogInformation($"Event ID: {record.EventID}");
        context.Logger.LogInformation($"Event Name: {record.EventName}");
        context.Logger.LogInformation($"Old DynamoDB Record: {record.Dynamodb.OldImage.ToJson()}");
        context.Logger.LogInformation($"New DynamoDB Record: {record.Dynamodb.NewImage.ToJson()}");
    }

    context.Logger.LogInformation("DynamoDB event processing complete.");
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();