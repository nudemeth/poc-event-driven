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
            TransactItems = account.Events
                .Select(e => JsonSerializer.Serialize(e, DomainEventJsonOptions.Instance))
                .Select(Document.FromJson)
                .Select(doc => doc.ToAttributeMap())
                .Select(attributeMap => new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = AccountTableName,
                        Item = attributeMap
                    }
                })
                .ToList()
        };

        await _dynamoDbClient.TransactWriteItemsAsync(request);
    }

    public async Task<AccountEntity?> GetAccountByIdAsync(Guid id)
    {
        var request = new QueryRequest
        {
            TableName = AccountTableName,
            KeyConditionExpression = "StreamId = :v_StreamId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_StreamId", new AttributeValue { S = id.ToString() } }
            }
        };
        var response = await _dynamoDbClient.QueryAsync(request);

        if (response.Count == 0)
        {
            return null;
        }

        var events = response.Items
            .Select(Document.FromAttributeMap)
            .Select(doc => doc.ToJson())
            .Select(json => JsonSerializer.Deserialize<DomainEvent>(json, DomainEventJsonOptions.Instance)!);
        var account = AccountEntity.ReplayEvents(events);

        return account;
    }
}