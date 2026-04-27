output "dynamodb_table_name" {
  value       = aws_dynamodb_table.accounts.name
  description = "The name of the DynamoDB table"
}

output "dynamodb_table_arn" {
  value       = aws_dynamodb_table.accounts.arn
  description = "The ARN of the DynamoDB table"
}

output "dynamodb_table_stream_arn" {
  value       = aws_dynamodb_table.accounts.stream_arn
  description = "The Stream ARN of the DynamoDB table"
}

output "sns_topic_name" {
  value       = aws_sns_topic.account_events.name
  description = "The name of the SNS topic for account events"
}

output "sns_topic_arn" {
  value       = aws_sns_topic.account_events.arn
  description = "The ARN of the SNS topic for account events"
}

output "account_events_queue_arn" {
  value       = aws_sqs_queue.account_events_queue.arn
  description = "ARN of the account events SQS FIFO queue"
}

output "account_events_queue_url" {
  value       = aws_sqs_queue.account_events_queue.url
  description = "URL of the account events SQS FIFO queue"
}

output "account_outbox_publisher_lambda_arn" {
  value       = aws_lambda_function.account_outbox_publisher.arn
  description = "The ARN of the Account Outbox Publisher Lambda function"
}
