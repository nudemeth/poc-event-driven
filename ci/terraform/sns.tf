# SNS Topic for Account Events
resource "aws_sns_topic" "account_events" {
  name = "account-events-sns"
}

# SNS Topic Policy to allow Lambda to publish
resource "aws_sns_topic_policy" "account_events_policy" {
  arn = aws_sns_topic.account_events.arn

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
        Action   = "SNS:Publish"
        Resource = aws_sns_topic.account_events.arn
      }
    ]
  })
}
