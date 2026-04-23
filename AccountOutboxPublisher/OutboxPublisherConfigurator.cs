using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountOutboxPublisher;

public static class OutboxPublisherConfigurator
{
    public static IServiceCollection ConfigureOutboxPublisherServices(
        this IServiceCollection services,
         IConfiguration configuration,
         ILambdaContext context)
    {
        var awsOptions = configuration.GetAWSOptions();
        var credentials = new EnvironmentVariablesAWSCredentials();

        // Configure DynamoDB client
        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = awsOptions.DefaultClientConfig.ServiceURL,
            AuthenticationRegion = awsOptions.Region.SystemName
        };
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(credentials, dynamoDbConfig));

        // Configure SNS client
        var snsConfig = new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = awsOptions.DefaultClientConfig.ServiceURL,
            AuthenticationRegion = awsOptions.Region.SystemName
        };
        services.AddSingleton<IAmazonSimpleNotificationService>(
            new AmazonSimpleNotificationServiceClient(credentials, snsConfig));

        services.AddSingleton(configuration);
        services.AddSingleton(context);
        services.AddScoped<OutboxRepository>();
        services.AddScoped<OutboxPublisher>();

        return services;
    }
}
