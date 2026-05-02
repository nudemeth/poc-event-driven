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
      "INBOX_TABLE_NAME"  = aws_dynamodb_table.inbox.name
    }
  }
}

# SQS as event source for Lambda (maintains FIFO ordering)
resource "aws_lambda_event_source_mapping" "account_event_listener_sqs" {
  event_source_arn  = aws_sqs_queue.account_events_queue.arn
  function_name     = aws_lambda_function.account_event_listener.function_name
  batch_size        = 1  # Process one message at a time to maintain ordering
  function_response_types = ["ReportBatchItemFailures"]
}

# Lambda permission for SQS to invoke
resource "aws_lambda_permission" "account_event_listener_sqs_invoke" {
  statement_id  = "AllowExecutionFromSQS"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.account_event_listener.function_name
  principal     = "sqs.amazonaws.com"
  source_arn    = aws_sqs_queue.account_events_queue.arn
}

variable "connection_string" {
  type = string
}
