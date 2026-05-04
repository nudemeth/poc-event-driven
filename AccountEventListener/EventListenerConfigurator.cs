using AccountEventListener.Decorators;
using AccountProjection;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Runtime;
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
                .Where(type => !type.Name.Contains("Decorator")))
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

        services.Decorate<INotificationHandler<AccountOpened>, InboxDecorator<AccountOpened>>();
        services.Decorate<INotificationHandler<MoneyDeposited>, InboxDecorator<MoneyDeposited>>();
        services.Decorate<INotificationHandler<MoneyWithdrawn>, InboxDecorator<MoneyWithdrawn>>();
        services.Decorate<INotificationHandler<AccountClosed>, InboxDecorator<AccountClosed>>();
        services.Decorate<INotificationHandler<MoneyTransferredIn>, InboxDecorator<MoneyTransferredIn>>();
        services.Decorate<INotificationHandler<MoneyTransferredOut>, InboxDecorator<MoneyTransferredOut>>();

        services.ConfigureAccountProjectionServices();

        return services;
    }
}
