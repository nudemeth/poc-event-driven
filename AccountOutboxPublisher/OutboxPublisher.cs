using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AccountOutboxPublisher;

public class OutboxPublisher
{
    private readonly string _snsTopicName;
    private readonly OutboxRepository _outboxRepository;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly ILambdaContext _context;
    private string? _topicArn;

    public OutboxPublisher(
        OutboxRepository outboxRepository,
        IAmazonSimpleNotificationService snsClient,
        ILambdaContext context,
        IConfiguration configuration)
    {
        _outboxRepository = outboxRepository;
        _snsClient = snsClient;
        _context = context;
        _snsTopicName = configuration["SNS_TOPIC_ARN"] ?? throw new InvalidOperationException("SNS_TOPIC_ARN environment variable is not set.");
    }

    public async Task PublishUnpublishedItemsAsync()
    {
        _context.Logger.LogInformation("Fetching unpublished outbox items...");

        var unpublishedItems = await _outboxRepository.GetUnpublishedItemsAsync();

        if (unpublishedItems.Count == 0)
        {
            _context.Logger.LogInformation("No unpublished items found in outbox.");
            return;
        }

        _context.Logger.LogInformation($"Found {unpublishedItems.Count} unpublished items to publish.");

        foreach (var item in unpublishedItems)
        {
            try
            {
                _context.Logger.LogInformation($"Publishing message {item.MessageId} with event type {item.EventType}");

                await PublishMessageToSnsAsync(item);

                _context.Logger.LogInformation($"Successfully published message {item.MessageId}.");

                await _outboxRepository.MarkAsPublishedAsync(item);

                _context.Logger.LogInformation($"Marked message {item.MessageId} as published.");
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, $"Failed to publish message {item.MessageId}: {ex.Message}");
                throw;
            }
        }

        _context.Logger.LogInformation($"Successfully published {unpublishedItems.Count} items.");
    }

    private async Task PublishMessageToSnsAsync(OutboxItem item)
    {
        var topicArn = await GetTopicArnAsync();

        var publishRequest = new PublishRequest
        {
            TopicArn = topicArn,
            Subject = $"Domain Event: {item.EventType}",
            Message = item.EventData,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "EventType", new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = item.EventType
                    }
                },
                {
                    "MessageId", new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = item.MessageId
                    }
                }
            }
        };

        await _snsClient.PublishAsync(publishRequest);
    }

    private async Task<string> GetTopicArnAsync()
    {
        if (!string.IsNullOrEmpty(_topicArn))
        {
            return _topicArn;
        }

        _context.Logger.LogInformation($"Using SNS topic ARN from configuration: {_snsTopicName}");
        _topicArn = _snsTopicName;

        return _topicArn;
    }
}
