# Account Outbox Publisher

## Purpose

The Account Outbox Publisher is an AWS Lambda function responsible for publishing domain events from the DynamoDB outbox table to AWS SNS (Simple Notification Service). This implements the outbox pattern for reliable event publishing in an event-driven architecture.

## How It Works

1. **Retrieves Unpublished Items**: When triggered, the Lambda function scans the `AccountsOutbox` DynamoDB table for items where `IsPublished = 0`
2. **Publishes to SNS**: For each unpublished item, it publishes the event data to the SNS topic named `account-events-sns`
3. **Marks as Published**: After successful publication, it updates the outbox item to mark `IsPublished = 1` and sets the `PublishedAt` timestamp
4. **Tolerates Failures**: If publication fails for an item, the function logs the error and skips that item (can be retried on next invocation)

## Architecture

### Components

- **Function.cs**: Lambda handler entry point that initializes DI and orchestrates the publishing process
- **OutboxPublisher.cs**: Main service that handles the publishing workflow
  - Fetches unpublished items from the outbox repository
  - Publishes each item to SNS with message attributes
  - Marks items as published after successful SNS publication
- **OutboxRepository.cs**: Data access layer for the `AccountsOutbox` DynamoDB table
  - `GetUnpublishedItemsAsync()`: Scans for unpublished items
  - `MarkAsPublishedAsync()`: Updates the DynamoDB item after publication
- **OutboxPublisherConfigurator.cs**: Dependency injection configuration
  - Configures DynamoDB client with credentials from environment
  - Configures SNS client with credentials from environment
  - Registers services as scoped dependencies

## Data Model

### Outbox Item Structure

```csharp
{
    "MessageId": "guid-string",           // Unique identifier for the outbox message
    "EventType": "string",                // Name of the domain event (e.g., "AccountOpened")
    "EventData": "json-string",           // Serialized domain event object
    "CreatedAt": "iso-datetime",          // When the event was created
    "IsPublished": 0 | 1,                 // 0 = unpublished, 1 = published
    "PublishedAt": "iso-datetime" | null, // When the event was published
    "ExpiresAt": number | null            // TTL timestamp (optional)
}
```

## SNS Publishing

Each message published to SNS includes:

- **Subject**: `Domain Event: {EventType}`
- **Message**: The serialized event data
- **Message Attributes**:
  - `EventType`: The type of domain event
  - `MessageId`: The unique message identifier

Subscribers can use these attributes to filter and route events appropriately.

## Environment Requirements

The Lambda function requires the following to be configured:

- **AWS Credentials**: Via `EnvironmentVariablesAWSCredentials` (typically set by Lambda execution role)
- **SNS Topic**: Must have a topic named `account-events-sns` in the same region
- **DynamoDB Table**: Must have a table named `AccountsOutbox` with `MessageId` as the primary key

## Triggering the Lambda

This Lambda is typically triggered by:

- CloudWatch Events (EventBridge) on a schedule (e.g., every 5 minutes)
- Custom event sources
- Manual invocation for testing

## Logging

The function provides detailed logging at each step:

- When items are fetched
- When items are being published
- When items are marked as published
- Errors and their details

## Error Handling

- If SNS topic cannot be found, the function throws `InvalidOperationException`
- If publishing fails, the function logs the error and rethrows (allowing Lambda retry mechanisms)
- Failed items remain with `IsPublished = 0` in the table for retry on next execution
