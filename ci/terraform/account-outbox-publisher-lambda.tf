# Lambda Function for Account Outbox Publisher
resource "aws_lambda_function" "account_outbox_publisher" {
  function_name    = "account-outbox-publisher"
  role             = "arn:aws:iam::000000000000:role/account-outbox-publisher-role"
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
