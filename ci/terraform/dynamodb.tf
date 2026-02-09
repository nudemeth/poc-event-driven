resource "aws_dynamodb_table" "accounts" {
  name             = "Accounts"
  billing_mode     = "PAY_PER_REQUEST"
  hash_key         = "StreamId"
  range_key        = "Timestamp"
  stream_enabled   = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "StreamId"
    type = "S"
  }

  attribute {
    name = "Timestamp"
    type = "S"
  }
}
