# IAM Role for AccountOutboxPublisher Lambda
resource "aws_iam_role" "account_outbox_publisher_role" {
  name = "account-outbox-publisher-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

# IAM Policy for DynamoDB access (read from outbox table, update for marking as published)
resource "aws_iam_role_policy" "outbox_publisher_dynamodb_policy" {
  name = "account-outbox-publisher-dynamodb-policy"
  role = aws_iam_role.account_outbox_publisher_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:Scan",
          "dynamodb:UpdateItem"
        ]
        Resource = aws_dynamodb_table.outbox.arn
      }
    ]
  })
}

# IAM Policy for SNS publish
resource "aws_iam_role_policy" "outbox_publisher_sns_policy" {
  name = "account-outbox-publisher-sns-policy"
  role = aws_iam_role.account_outbox_publisher_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "sns:Publish",
          "sns:ListTopics"
        ]
        Resource = aws_sns_topic.account_events.arn
      }
    ]
  })
}

# Lambda Function for Account Outbox Publisher
resource "aws_lambda_function" "account_outbox_publisher" {
  function_name    = "account-outbox-publisher"
  role             = aws_iam_role.account_outbox_publisher_role.arn
  handler          = "AccountOutboxPublisher"
  runtime          = "dotnet10"
  filename         = "./AccountOutboxPublisher.zip"
  source_code_hash = filebase64sha256("./AccountOutboxPublisher.zip")
  publish          = true
  timeout          = 60

  environment {
    variables = {
      "SNS_TOPIC_ARN" = aws_sns_topic.account_events.arn
    }
  }
}

# EventBridge Rule to trigger the lambda on a schedule
resource "aws_cloudwatch_event_rule" "outbox_publisher_schedule" {
  name                = "account-outbox-publisher-schedule"
  description         = "Trigger AccountOutboxPublisher Lambda on schedule"
  schedule_expression = var.outbox_publisher_schedule
}

# EventBridge Target
resource "aws_cloudwatch_event_target" "outbox_publisher_target" {
  rule      = aws_cloudwatch_event_rule.outbox_publisher_schedule.name
  target_id = "AccountOutboxPublisherLambda"
  arn       = aws_lambda_function.account_outbox_publisher.arn
}

# Lambda Permission for EventBridge
resource "aws_lambda_permission" "allow_eventbridge_invoke" {
  statement_id  = "AllowExecutionFromEventBridge"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.account_outbox_publisher.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.outbox_publisher_schedule.arn
}
