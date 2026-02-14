resource "aws_lambda_function" "account_event_listener" {
  function_name    = "account-event-listener"
  role             = "arn:aws:iam::000000000000:role/account-event-listener-role"
  handler          = "AccountEventListener"
  runtime          = "dotnet8"
  filename         = "./AccountEventListener.zip"
  source_code_hash = filebase64sha256("./AccountEventListener.zip")
  publish          = true
}

resource "aws_lambda_event_source_mapping" "dynamodb_to_lambda" {
  event_source_arn  = aws_dynamodb_table.accounts.stream_arn
  function_name     = aws_lambda_function.account_event_listener.arn
  starting_position = "LATEST"
}
