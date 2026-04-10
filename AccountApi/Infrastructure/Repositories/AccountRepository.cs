using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Account;

public class AccountRepository : IAccountRepository
{
    private const string AccountTableName = "Accounts";
    private const int SnapshotInterval = 3;
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public AccountRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task SaveAsync(AccountEntity account)
    {
        await SaveAsync([account]);
    }

    public async Task SaveAsync(IEnumerable<AccountEntity> accounts)
    {
        var accountList = accounts.ToList();

        if (!accountList.Any())
        {
            return;
        }

        var request = new TransactWriteItemsRequest
        {
            TransactItems = []
        };

        // Validate all accounts for concurrency conflicts
        foreach (var account in accountList)
        {
            var lastCommittedEventVersion = account.CommittedEvents.LastOrDefault()?.Version ?? -1;
            var firstUncommittedEventVersion = account.UncommittedEvents.FirstOrDefault()?.Version;

            // Skip validation if no uncommitted events
            if (firstUncommittedEventVersion == null)
            {
                continue;
            }

            // Check if versions are sequential
            if (lastCommittedEventVersion >= 0 && lastCommittedEventVersion != firstUncommittedEventVersion - 1)
            {
                throw new ConcurrencyException($"Concurrency conflict detected for account {account.Id}. Expected version {lastCommittedEventVersion + 1} but got {firstUncommittedEventVersion}.");
            }

            // Add events to transaction
            foreach (var uncommittedEvent in account.UncommittedEvents)
            {
                request.TransactItems.Add(BuildEventTransactionItem(uncommittedEvent));
                request.TransactItems.Add(BuildOutboxTransactionItem(uncommittedEvent));
            }

            // Add snapshot to transaction if needed
            var snapshotItem = BuildSnapshotTransactionItem(account);
            if (snapshotItem != null)
            {
                request.TransactItems.Add(snapshotItem);
            }
        }

        try
        {
            await _dynamoDbClient.TransactWriteItemsAsync(request);
            accountList.ForEach(account => account.CommitEvents());
        }
        catch (TransactionCanceledException ex) when (ex.CancellationReasons.Any(r => r.Code == "ConditionalCheckFailed"))
        {
            throw new ConcurrencyException($"Concurrency conflict detected. One or more accounts have conflicting versions.", ex);
        }
    }

    private TransactWriteItem BuildEventTransactionItem(DomainEvent @event)
    {
        var json = JsonSerializer.Serialize(@event, DomainEventJsonOptions.Instance);
        var doc = Document.FromJson(json);
        var attributeMap = doc.ToAttributeMap();

        return new TransactWriteItem
        {
            Put = new Put
            {
                TableName = AccountTableName,
                Item = attributeMap,
                // DynamoDB will check both PK and SK for uniqueness even though we only specify PK here.
                // https://rory.horse/posts/dynamo-dissected-condition-expression-existence-check/
                // https://www.alexdebrie.com/posts/dynamodb-condition-expressions/
                ConditionExpression = "attribute_not_exists(StreamId)"
            }
        };
    }

    private TransactWriteItem? BuildSnapshotTransactionItem(AccountEntity account)
    {
        // Create snapshot only if we cross an interval boundary
        if (account.Version <= 0 || account.Version % SnapshotInterval != 0)
        {
            return null;
        }

        var snapshot = account.CreateSnapshot();
        var snapshotJson = JsonSerializer.Serialize(snapshot, DomainEventJsonOptions.Instance);
        var snapshotDoc = Document.FromJson(snapshotJson);
        var snapshotAttributeMap = snapshotDoc.ToAttributeMap();

        return new TransactWriteItem
        {
            Put = new Put
            {
                TableName = AccountTableName,
                Item = snapshotAttributeMap,
                ConditionExpression = "attribute_not_exists(StreamId)"
            }
        };
    }

    private TransactWriteItem BuildOutboxTransactionItem(DomainEvent @event)
    {
        var outbox = new
        {
            OutboxId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow.ToString(),
            EventType = @event.GetType().Name,
            EventData = JsonSerializer.Serialize(@event, DomainEventJsonOptions.Instance),
            PublishedAt = null as DateTimeOffset?,
            ExpiresAt = null as DateTimeOffset?
        };
        var json = JsonSerializer.Serialize(outbox);
        var doc = Document.FromJson(json);
        var attributeMap = doc.ToAttributeMap();

        return new TransactWriteItem
        {
            Put = new Put
            {
                TableName = "AccountsOutbox",
                Item = attributeMap,
            }
        };
    }

    public async Task<AccountEntity?> GetAccountByIdAsync(Guid id)
    {
        var snapshot = await GetAccountSnapshotAsync(id);

        if (snapshot == null)
        {
            var events = await GetEventsAfterVersionAsync(id, 0);
            return AccountEntity.ReplayEvents(events);
        }

        var eventsAfterSnapshot = await GetEventsAfterVersionAsync(id, snapshot.Version);

        if (eventsAfterSnapshot.Count == 0)
        {
            return AccountEntity.RestoreFromSnapshot(snapshot, []);
        }

        return AccountEntity.RestoreFromSnapshot(snapshot, eventsAfterSnapshot);
    }

    private async Task<IList<DomainEvent>> GetEventsAfterVersionAsync(Guid id, int version)
    {
        var request = new QueryRequest
        {
            TableName = AccountTableName,
            KeyConditionExpression = "StreamId = :v_StreamId AND Version > :v_Version",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_StreamId", new AttributeValue { S = id.ToString() } },
                { ":v_Version", new AttributeValue { N = version.ToString() } }
            }
        };
        var response = await _dynamoDbClient.QueryAsync(request);

        if (response.Count == 0)
        {
            return [];
        }

        var events = response.Items
            .Select(Document.FromAttributeMap)
            .Select(doc => doc.ToJson())
            .Select(json => JsonSerializer.Deserialize<DomainEvent>(json, DomainEventJsonOptions.Instance)!)
            .ToList();

        return events;
    }

    private async Task<AccountSnapshot?> GetAccountSnapshotAsync(Guid id)
    {
        var request = new QueryRequest
        {
            TableName = AccountTableName,
            KeyConditionExpression = "StreamId = :v_StreamId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_StreamId", new AttributeValue { S = $"{id}-Snapshot" } }
            },
            ScanIndexForward = false, // Get the latest snapshot first
            Limit = 1
        };
        var response = await _dynamoDbClient.QueryAsync(request);

        if (response.Count == 0)
        {
            return null;
        }

        var snapshotDoc = Document.FromAttributeMap(response.Items[0]);
        var snapshotJson = snapshotDoc.ToJson();
        var snapshot = JsonSerializer.Deserialize<Snapshot>(snapshotJson, DomainEventJsonOptions.Instance)!;

        return snapshot as AccountSnapshot;
    }
}