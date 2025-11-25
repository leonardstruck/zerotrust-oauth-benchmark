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

variable "gateway_api_client_secret" {
  description = "Confidential client secret for the gateway-api client."
  type        = string
  sensitive   = true
}

variable "inventory_api_client_secret" {
  description = "Confidential client secret for the inventory-api client."
  type        = string
  sensitive   = true
}

variable "shipping_api_client_secret" {
  description = "Confidential client secret for the shipping-api client."
  type        = string
  sensitive   = true
}

variable "backoffice_admin_api_client_secret" {
  description = "Confidential client secret for the backoffice-admin-api client."
  type        = string
  sensitive   = true
}

variable "load_tester_client_secret" {
  description = "Confidential client secret for the load-tester client."
  type        = string
  sensitive   = true
}
