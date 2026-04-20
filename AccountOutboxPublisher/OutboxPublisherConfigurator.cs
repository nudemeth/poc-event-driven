using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AccountOutboxPublisher;

public static class OutboxPublisherConfigurator
{
    public static IServiceCollection ConfigureOutboxPublisherServices(this IServiceCollection services, IConfiguration configuration)
    {
        var awsOptions = configuration.GetAWSOptions();
        var credentials = new EnvironmentVariablesAWSCredentials();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configure DynamoDB client
        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = awsOptions.DefaultClientConfig?.ServiceURL,
            AuthenticationRegion = awsOptions.Region?.SystemName
        };
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(credentials, dynamoDbConfig));

        // Configure SNS client
        var snsConfig = new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = awsOptions.DefaultClientConfig?.ServiceURL,
            AuthenticationRegion = awsOptions.Region?.SystemName
        };
        services.AddSingleton<IAmazonSimpleNotificationService>(
            new AmazonSimpleNotificationServiceClient(credentials, snsConfig));

        services.AddScoped<OutboxRepository>();
        services.AddScoped(sp => new OutboxPublisher(
            sp.GetRequiredService<OutboxRepository>(),
            sp.GetRequiredService<IAmazonSimpleNotificationService>(),
            sp.GetRequiredService<ILogger<OutboxPublisher>>(),
            configuration));

        return services;
    }
}
