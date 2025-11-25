variable "keycloak_url" {
  description = "The URL of the Keycloak server."
  type        = string
}

variable "keycloak_client_id" {
  description = "The client ID to authenticate with Keycloak."
  type        = string
  default     = "admin-cli"
}

variable "keycloak_username" {
  description = "The username for Keycloak authentication."
  type        = string
  default     = "admin"
}

variable "keycloak_password" {
  description = "The password for Keycloak authentication."
  type        = string
  sensitive   = true
}

variable "inventory_client_secrets" {
  description = "Map of Keycloak client IDs to confidential client secrets provided by the Aspire AppHost."
  type        = map(string)
  sensitive   = true
}
