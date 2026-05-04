# poc-event-driven

A proof-of-concept event-driven architecture using AWS Lambda, DynamoDB, and SNS.

## Architecture

- **AccountApi**: Main API service for account operations
- **AccountEventListener**: Lambda function triggered by DynamoDB Streams to listen for account events
- **AccountOutboxPublisher**: Lambda function that publishes outbox items from DynamoDB to SNS on a schedule
- **AccountProjectionDataAccess**: Read model/projection for account data
- **FraudDetectionService**: Service for fraud detection (placeholder)
- **NotificationService**: Service for notifications (placeholder)

## Building

### Prerequisites

- .NET 10.0 SDK
- AWS Lambda tools: `dotnet tool install -g Amazon.Lambda.Tools`

### Build Lambda Functions

#### AccountEventListener

```bash
cd AccountEventListener
dotnet lambda package --output-package ../ci/terraform/AccountEventListener.zip
```

#### AccountOutboxPublisher

```bash
cd AccountOutboxPublisher
dotnet lambda package --output-package ../ci/terraform/AccountOutboxPublisher.zip
```

## Deployment

### Prerequisites

- Terraform >= 1.0
- LocalStack (for local development) or AWS credentials for cloud deployment

### Local Development with LocalStack

Start LocalStack:

```bash
docker-compose up
```

Deploy infrastructure:

```bash
cd ci/terraform
terraform init
terraform plan
terraform apply
```

### Environment Variables

For the AccountOutboxPublisher Lambda:

- `SNS_TOPIC_ARN`: ARN of the SNS topic to publish events (set automatically by Terraform)

For the AccountEventListener Lambda:

- `CONNECTION_STRING`: Database connection string (set via Terraform variables)
