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
