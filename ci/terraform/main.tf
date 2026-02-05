terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region                      = var.aws_region
  access_key                  = "test"
  secret_key                  = "test"
  skip_credentials_validation = true
  skip_requesting_account_id  = true

  endpoints {
    dynamodb = "http://localstack:4566"
  }
}

# DynamoDB Table for Accounts
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

  tags = {
    Name        = "Accounts"
    Environment = var.environment
    Project     = "POC-Event-Driven"
  }
}

# Output the table name and ARN
output "dynamodb_table_name" {
  value       = aws_dynamodb_table.accounts.name
  description = "The name of the DynamoDB table"
}

output "dynamodb_table_arn" {
  value       = aws_dynamodb_table.accounts.arn
  description = "The ARN of the DynamoDB table"
}

output "dynamodb_table_stream_arn" {
  value       = aws_dynamodb_table.accounts.stream_arn
  description = "The Stream ARN of the DynamoDB table"
}
