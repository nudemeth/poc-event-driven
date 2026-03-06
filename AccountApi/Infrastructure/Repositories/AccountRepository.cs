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
        var existingAccount = await GetAccountByIdAsync(account.Id);
        var lastStoredVersion = existingAccount?.Version ?? -1;
        var firstNewEventVersion = account.Events.First().Version;

        // Check if versions are sequential
        if (lastStoredVersion >= 0 && lastStoredVersion != firstNewEventVersion - 1)
        {
            throw new ConcurrencyException($"Concurrency conflict detected for account {account.Id}. Expected version {lastStoredVersion + 1} but got {firstNewEventVersion}.");
        }

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
                        Item = attributeMap,
                        // DynamoDB will check both StreamId and Version for uniqueness even though we only specify StreamId here.
                        // https://rory.horse/posts/dynamo-dissected-condition-expression-existence-check/
                        // https://www.alexdebrie.com/posts/dynamodb-condition-expressions/
                        ConditionExpression = "attribute_not_exists(StreamId)",
                    }
                })
                .ToList()
        };

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(request);
        }
        catch (ConditionalCheckFailedException ex)
        {
            throw new ConcurrencyException($"Concurrency conflict detected for account {account.Id}. Expected version {lastStoredVersion + 1} but got {firstNewEventVersion}.", ex);
        }
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