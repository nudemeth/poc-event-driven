using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace AccountOutboxPublisher;

public record OutboxItem(
    string AccountId,
    string MessageId,
    string EventType,
    string EventData,
    string CreatedAt,
    int IsPublished,
    string? PublishedAt,
    long? ExpiresAt)
{
}

public class OutboxRepository
{
    private const string OutboxTableName = "AccountsOutbox";
    private const string IsPublishedGSIName = "IsPublished-CreatedAt-Index";
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public OutboxRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<IList<OutboxItem>> GetUnpublishedItemsAsync()
    {
        var request = new QueryRequest
        {
            TableName = OutboxTableName,
            IndexName = IsPublishedGSIName,
            KeyConditionExpression = "IsPublished = :v_IsPublished",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_IsPublished", new AttributeValue { N = "0" } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request);

        if (response.Count == 0)
        {
            return [];
        }

        var items = response.Items
            .Select(Document.FromAttributeMap)
            .Select(doc => doc.ToJson())
            .Select(json => JsonSerializer.Deserialize<OutboxItem>(json)!)
            .ToList();

        return items;
    }

    public async Task SaveAsync(OutboxItem item)
    {
        var doc = Document.FromJson(JsonSerializer.Serialize(item));
        var putRequest = new PutItemRequest
        {
            TableName = OutboxTableName,
            Item = doc.ToAttributeMap()
        };

        await _dynamoDbClient.PutItemAsync(putRequest);
    }
}
