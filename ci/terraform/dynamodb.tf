resource "aws_dynamodb_table" "accounts" {
  name             = "Accounts"
  billing_mode     = "PAY_PER_REQUEST"
  hash_key         = "StreamId"
  range_key        = "Version"
  stream_enabled   = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "StreamId"
    type = "S"
  }

  attribute {
    name = "Version"
    type = "N"
  }
}

resource "aws_dynamodb_table" "outbox" {
  name         = "AccountsOutbox"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "OutboxId"
  range_key    = "CreatedAt"

  ttl {
    attribute_name = "ExpiresAt"
    enabled        = true
  }

  attribute {
    name = "OutboxId"
    type = "S"
  }

  attribute {
    name = "CreatedAt"
    type = "S"
  }
}
