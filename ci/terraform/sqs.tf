# SQS FIFO Dead Letter Queue for unprocessable Account Events
resource "aws_sqs_queue" "account_events_dlq" {
  name                        = "account-events-dlq.fifo"
  fifo_queue                  = true
  content_based_deduplication = false
  message_retention_seconds   = 1209600 # 14 days
}

# SQS FIFO Queue for Account Events (ordered message delivery)
resource "aws_sqs_queue" "account_events_queue" {
  name                        = "account-events-queue.fifo"
  fifo_queue                  = true
  content_based_deduplication = false
  deduplication_scope         = "messageGroup"
  fifo_throughput_limit       = "perMessageGroupId"
  message_retention_seconds   = 1209600 # 14 days
  visibility_timeout_seconds  = 180     # 6x Lambda timeout (30s) per AWS recommendation

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.account_events_dlq.arn
    maxReceiveCount     = 3
  })
}

# SQS Queue Policy to allow SNS to send messages
resource "aws_sqs_queue_policy" "account_events_queue_policy" {
  queue_url = aws_sqs_queue.account_events_queue.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "sns.amazonaws.com"
        }
        Action   = "sqs:SendMessage"
        Resource = aws_sqs_queue.account_events_queue.arn
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = aws_sns_topic.account_events.arn
          }
        }
      }
    ]
  })
}

# SNS to SQS subscription (FIFO SNS delivers to SQS FIFO queue)
resource "aws_sns_topic_subscription" "account_events_sqs_subscription" {
  topic_arn            = aws_sns_topic.account_events.arn
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.account_events_queue.arn
  raw_message_delivery = true
}
