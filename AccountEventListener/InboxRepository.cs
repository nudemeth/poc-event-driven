using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AccountEventListener;

public class InboxRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public InboxRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = Environment.GetEnvironmentVariable("INBOX_TABLE_NAME") ?? "AccountsInbox";
    }

    // Returns true if the message is new and should be processed.
    // Returns false if the message was already received (duplicate).
    // Uses a conditional write so the check-and-insert is atomic.
    public async Task<bool> TryRecordAsync(string messageId, string eventType, string payload, CancellationToken ct = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["MessageId"] = new AttributeValue { S = messageId },
            ["EventType"] = new AttributeValue { S = eventType },
            ["Payload"] = new AttributeValue { S = payload },
            ["ReceivedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") }
        };

        try
        {
            await _dynamoDbClient.PutItemAsync(new PutItemRequest
            {
                TableName = _tableName,
                Item = item,
                ConditionExpression = "attribute_not_exists(MessageId)"
            }, ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    public async Task MarkProcessedAsync(string messageId, CancellationToken ct = default)
    {
        await _dynamoDbClient.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["MessageId"] = new AttributeValue { S = messageId }
            },
            UpdateExpression = "SET ProcessedAt = :processedAt, ExpiresAt = :expiresAt",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":processedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
                [":expiresAt"] = new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() }
            }
        }, ct);
    }
}
