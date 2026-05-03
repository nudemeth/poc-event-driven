using AccountProjection;
using AccountEventListener.EventHandlers;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Domain;
using Domain.Account;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

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
            opts.Assemblies = [typeof(AccountEntity).Assembly];
        });

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(EventListenerConfigurator), typeof(AccountEntity))
            .AddClasses(classes => classes
                .AssignableTo(typeof(INotificationHandler<>))
                .Where(type => !type.Name.StartsWith("AccountValidationDecorator")))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<EventContext>();
        services.AddScoped<InboxRepository>();

        services.Decorate<INotificationHandler<MoneyDeposited>, AccountValidationDecorator<MoneyDeposited>>();
        services.Decorate<INotificationHandler<MoneyWithdrawn>, AccountValidationDecorator<MoneyWithdrawn>>();
        services.Decorate<INotificationHandler<AccountClosed>, AccountValidationDecorator<AccountClosed>>();
        services.Decorate<INotificationHandler<MoneyTransferredIn>, AccountValidationDecorator<MoneyTransferredIn>>();
        services.Decorate<INotificationHandler<MoneyTransferredOut>, AccountValidationDecorator<MoneyTransferredOut>>();

        services.ConfigureAccountProjectionServices();

        return services;
    }
}
