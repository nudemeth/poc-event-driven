using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AccountEventListener;

public class InboxRepository(IAmazonDynamoDB dynamoDbClient)
{
    private readonly string _tableName = Environment.GetEnvironmentVariable("INBOX_TABLE_NAME") ?? "AccountsInbox";

    // Returns true if newly inserted, false if already exists.
    public async Task<bool> TryCreateAsync(InboxItem item, CancellationToken ct = default)
    {
        try
        {
            await dynamoDbClient.PutItemAsync(new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    ["MessageId"] = new() { S = item.MessageId },
                    ["EventType"] = new() { S = item.EventType },
                    ["Payload"] = new() { S = item.Payload },
                    ["ReceiveCount"] = new() { N = item.ReceiveCount.ToString() },
                    ["ReceiveTimes"] = new() { L = [new() { S = DateTime.UtcNow.ToString("o") }] }
                },
                ConditionExpression = "attribute_not_exists(MessageId)"
            }, ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    public async Task<InboxItem> GetInboxItemAsync(string messageId, CancellationToken ct = default)
    {
        var response = await dynamoDbClient.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { ["MessageId"] = new() { S = messageId } },
            ProjectionExpression = "EventType, Payload, ReceiveCount, ProcessedAt"
        }, ct);

        return new InboxItem
        {
            MessageId = messageId,
            EventType = response.Item.TryGetValue("EventType", out var et) ? et.S : string.Empty,
            Payload = response.Item.TryGetValue("Payload", out var p) ? p.S : string.Empty,
            ReceiveCount = response.Item.TryGetValue("ReceiveCount", out var rc) && int.TryParse(rc.N, out var count) ? count : 0,
            IsProcessed = response.Item.ContainsKey("ProcessedAt")
        };
    }

    // Returns false if the item was already marked processed (atomic check).
    public async Task<bool> TryUpdateReceiveCountAsync(string messageId, int receiveCount, CancellationToken ct = default)
    {
        try
        {
            await dynamoDbClient.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue> { ["MessageId"] = new() { S = messageId } },
                UpdateExpression = "SET ReceiveCount = :count, ReceiveTimes = list_append(ReceiveTimes, :ts)",
                ConditionExpression = "attribute_not_exists(ProcessedAt)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":count"] = new() { N = receiveCount.ToString() },
                    [":ts"] = new() { L = [new() { S = DateTime.UtcNow.ToString("o") }] }
                }
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
        await dynamoDbClient.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { ["MessageId"] = new() { S = messageId } },
            UpdateExpression = "SET ProcessedAt = :processedAt, ExpiresAt = :expiresAt",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":processedAt"] = new() { S = DateTime.UtcNow.ToString("o") },
                [":expiresAt"] = new() { N = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() }
            }
        }, ct);
    }

    public class InboxItem
    {
        public required string MessageId { get; init; }
        public required string EventType { get; init; }
        public required string Payload { get; init; }
        public required int ReceiveCount { get; init; }
        public bool IsProcessed { get; init; }
    }
}
