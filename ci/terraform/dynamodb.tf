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

resource "aws_dynamodb_table" "inbox" {
  name         = "AccountsInbox"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "MessageId"

  ttl {
    attribute_name = "ExpiresAt"
    enabled        = true
  }

  attribute {
    name = "MessageId"
    type = "S"
  }
}

resource "aws_dynamodb_table" "outbox" {
  name         = "AccountsOutbox"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "AccountId"
  range_key    = "CreatedAt"

  ttl {
    attribute_name = "ExpiresAt"
    enabled        = true
  }

  attribute {
    name = "AccountId"
    type = "S"
  }

  attribute {
    name = "CreatedAt"
    type = "S"
  }

  attribute {
    name = "IsPublished"
    type = "N" // Use 0/1 for boolean because DynamoDB does not support boolean on keys
  }

  global_secondary_index {
    name            = "IsPublished-CreatedAt-Index"
    projection_type = "ALL"
    key_schema {
      attribute_name = "IsPublished"
      key_type       = "HASH"
    }
    key_schema {
      attribute_name = "CreatedAt"
      key_type       = "RANGE"
    }
  }
}
