using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Account;

public class AccountRepository : IAccountRepository
{
    private const string AccountTableName = "Accounts";
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
                    TableName = AccountTableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "StreamId", new AttributeValue { S = e.StreamId.ToString() } },
                        { "EventType", new AttributeValue { S = e.GetType().Name } },
                        { "Data", new AttributeValue { S = JsonSerializer.Serialize(e, DomainEventJsonOptions.Instance) } },
                        { "Timestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
                    }
                }
            }).ToList()
        };
        await _dynamoDbClient.TransactWriteItemsAsync(request);
    }

    public async Task<AccountEntity?> GetAccountByIdAsync(Guid id)
    {
        var queryRequest = new QueryRequest
        {
            TableName = AccountTableName,
            KeyConditionExpression = "StreamId = :v_StreamId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_StreamId", new AttributeValue { S = id.ToString() } }
            }
        };
        var response = await _dynamoDbClient.QueryAsync(queryRequest);

        if (response.Count == 0)
        {
            return null;
        }

        var docs = response.Items.Select(Document.FromAttributeMap);
        var events = new List<DomainEvent>();

        foreach (var doc in docs)
        {
            var json = doc.ToJson();
            events.Add(JsonSerializer.Deserialize<DomainEvent>(json, DomainEventJsonOptions.Instance)!);
        }

        var account = AccountEntity.ReplayEvents(events!);

        return account;
    }
}