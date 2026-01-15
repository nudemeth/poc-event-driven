using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Account;

public class AccountRepository : IAccountRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public AccountRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task AppendAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        // Implementation to append account to DynamoDB
        var json = JsonSerializer.Serialize(@event);
        var document = Document.FromJson(json);
        var attributes = document.ToAttributeMap();
        var request = new PutItemRequest
        {
            TableName = "Accounts",
            Item = attributes
        };
        await _dynamoDbClient.PutItemAsync(request);
    }

    public async Task<AccountEntity?> GetAccountByIdAsync(Guid id)
    {
        // Implementation to get account by ID from DynamoDB
        return null;
    }
}