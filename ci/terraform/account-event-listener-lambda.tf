resource "aws_lambda_function" "account_event_listener" {
  function_name    = "account-event-listener"
  role             = "arn:aws:iam::000000000000:role/account-event-listener-role"
  handler          = "AccountEventListener"
  runtime          = "dotnet10"
  filename         = "./AccountEventListener.zip"
  source_code_hash = filebase64sha256("./AccountEventListener.zip")
  publish          = true
  environment {
    variables = {
      "CONNECTION_STRING" = var.connection_string
    }
  }
}

# SNS subscription to trigger Lambda
resource "aws_sns_topic_subscription" "account_event_listener_subscription" {
  topic_arn = aws_sns_topic.account_events.arn
  protocol  = "lambda"
  endpoint  = aws_lambda_function.account_event_listener.arn
}

# Lambda permission for SNS to invoke
resource "aws_lambda_permission" "account_event_listener_sns_invoke" {
  statement_id  = "AllowExecutionFromSNS"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.account_event_listener.function_name
  principal     = "sns.amazonaws.com"
  source_arn    = aws_sns_topic.account_events.arn
}

variable "connection_string" {
  type = string
}
