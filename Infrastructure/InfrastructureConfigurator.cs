using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureConfigurator
{
    public static async Task ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var awsOptions = configuration.GetAWSOptions();
        var credentials = new EnvironmentVariablesAWSCredentials();
        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = awsOptions.DefaultClientConfig.ServiceURL,
            AuthenticationRegion = awsOptions.Region.SystemName
        };
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(credentials, dynamoDbConfig));

        services.AddScoped<IAccountRepository, AccountRepository>();
    }
}
