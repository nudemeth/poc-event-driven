using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace AccountOutboxPublisher;

public record OutboxItem(
    string MessageId,
    string EventType,
    string EventData,
    string CreatedAt,
    bool IsPublished)
{
}

public class OutboxRepository
{
    private const string OutboxTableName = "AccountsOutbox";
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

    public async Task MarkAsPublishedAsync(string messageId)
    {
        var publishedAt = DateTime.UtcNow;
        var expiresAt = publishedAt.AddDays(90); // Set TTL to 90 days
        var expiresAtUnixTimestamp = ((DateTimeOffset)expiresAt).ToUnixTimeSeconds();

        var updateRequest = new UpdateItemRequest
        {
            TableName = OutboxTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "MessageId", new AttributeValue { S = messageId } }
            },
            UpdateExpression = "SET IsPublished = :v_IsPublished, PublishedAt = :v_PublishedAt, ExpiresAt = :v_ExpiresAt",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_IsPublished", new AttributeValue { N = "1" } },
                { ":v_PublishedAt", new AttributeValue { S = publishedAt.ToString("o") } },
                { ":v_ExpiresAt", new AttributeValue { N = expiresAtUnixTimestamp.ToString() } }
            }
        };

        await _dynamoDbClient.UpdateItemAsync(updateRequest);
    }
}
