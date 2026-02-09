resource "aws_lambda_function" "account_event_listener" {
  function_name = "account-event-listener"
  role          = aws_iam_role.account_event_listener_role.arn
  handler       = "AccountEventListener"
  runtime       = "dotnet10"
  filename      = "./AccountEventListener.zip"
  publish       = true
}

resource "aws_iam_role" "account_event_listener_role" {
  name = "account-event-listener-role"

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
