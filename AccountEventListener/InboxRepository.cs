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

    // Returns true if the message is new or has not yet been successfully processed (should be processed/retried).
    // Returns false if the message was already successfully processed (duplicate).
    // Uses conditional writes so check-and-insert is atomic.
    // receiveCount is ApproximateReceiveCount from the SQS record (1 = first delivery, 2 = first retry, …).
    public async Task<bool> TryRecordAsync(string messageId, string eventType, string payload, int receiveCount, CancellationToken ct = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["MessageId"] = new AttributeValue { S = messageId },
            ["EventType"] = new AttributeValue { S = eventType },
            ["Payload"] = new AttributeValue { S = payload },
            ["ReceiveTimes"] = new AttributeValue { L = [new() { S = DateTime.UtcNow.ToString("o") }] },
            ["ReceiveCount"] = new AttributeValue { N = receiveCount.ToString() }
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
            var existing = await _dynamoDbClient.GetItemAsync(new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["MessageId"] = new AttributeValue { S = messageId }
                },
                ProjectionExpression = "Payload"
            }, ct);

            if (existing.Item.TryGetValue("Payload", out var storedPayload) && storedPayload.S != payload)
            {
                throw new InvalidOperationException(
                    $"Message '{messageId}' already exists with a different payload.");
            }

            // Message already recorded — update receive count only if not yet processed.
            try
            {
                await _dynamoDbClient.UpdateItemAsync(new UpdateItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["MessageId"] = new AttributeValue { S = messageId }
                    },
                    UpdateExpression = "SET ReceiveCount = :count, ReceiveTimes = list_append(ReceiveTimes, :ts)",
                    ConditionExpression = "attribute_not_exists(ProcessedAt)",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        [":count"] = new AttributeValue { N = receiveCount.ToString() },
                        [":ts"] = new AttributeValue { L = [new() { S = DateTime.UtcNow.ToString("o") }] }
                    }
                }, ct);

                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                return false;
            }
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
