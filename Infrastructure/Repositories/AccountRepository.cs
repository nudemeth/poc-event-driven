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

    public async Task SaveAsync(AccountEntity account)
    {
        var request = new TransactWriteItemsRequest
        {
            TransactItems = account.Events.Select(e => new TransactWriteItem
            {
                Put = new Put
                {
                    TableName = "Accounts",
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "StreamId", new AttributeValue { S = e.StreamId.ToString() } },
                        { "EventType", new AttributeValue { S = e.GetType().Name } },
                        { "Data", new AttributeValue { S = JsonSerializer.Serialize(e) } },
                        { "Timestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
                    }
                }
            }).ToList()
        };
        await _dynamoDbClient.TransactWriteItemsAsync(request);
    }

    public async Task<AccountEntity?> GetAccountByIdAsync(Guid id)
    {
        // Implementation to get account by ID from DynamoDB
        return null;
    }
}