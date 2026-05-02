using AccountProjection;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Domain.Account;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountEventListener;

public static class EventListenerConfigurator
{
    public static IServiceCollection ConfigureEventListenerServices(this IServiceCollection services, ILambdaContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var awsOptions = configuration.GetAWSOptions();
        var credentials = new EnvironmentVariablesAWSCredentials();

        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = awsOptions.DefaultClientConfig.ServiceURL,
            AuthenticationRegion = awsOptions.Region.SystemName
        };
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(credentials, dynamoDbConfig));

        services.AddSingleton(context);

        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Scoped;
            opts.Assemblies = [typeof(EventListenerConfigurator).Assembly, typeof(AccountEntity).Assembly];
        });

        services.ConfigureAccountProjectionServices();
        services.AddScoped<InboxRepository>();

        return services;
    }
}
