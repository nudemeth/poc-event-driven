variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Environment name (e.g., dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "outbox_publisher_schedule" {
  description = "Schedule expression for the Outbox Publisher Lambda (e.g., 'rate(1 minutes)')"
  type        = string
  default     = "rate(1 minutes)"
}
