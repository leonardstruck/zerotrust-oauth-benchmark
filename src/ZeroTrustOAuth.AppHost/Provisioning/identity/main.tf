locals {
  inventory_scope_definitions = {
    product_read = {
      name        = "inventory.product.read"
      description = "Read product metadata."
    }
    catalog_search = {
      name        = "inventory.catalog.search"
      description = "Search or list product catalog entries."
    }
    stock_read = {
      name        = "inventory.stock.read"
      description = "Read stock levels and reorder thresholds."
    }
    stock_adjust = {
      name        = "inventory.stock.adjust"
      description = "Adjust stock levels incrementally during fulfillment."
    }
    stock_override = {
      name        = "inventory.stock.override"
      description = "Override stock and reorder data for administrative corrections."
    }
    product_manage = {
      name        = "inventory.product.manage"
      description = "Create, update, or delete product master data."
    }
  }

  inventory_scope_names = {
    for key, scope in local.inventory_scope_definitions : key => scope.name
  }

  inventory_role_profiles = {
    role_public_api = {
      description = "Public API role limited to customer-facing catalog operations."
      scope_keys  = ["product_read", "catalog_search"]
    }
    role_shipping_worker = {
      description = "Shipping worker role for fulfillment services."
      scope_keys  = ["product_read", "stock_read", "stock_adjust"]
    }
    role_inventory_admin = {
      description = "Inventory administrator with full privileges."
      scope_keys = [
        "product_read",
        "catalog_search",
        "stock_read",
        "stock_adjust",
        "stock_override",
        "product_manage"
      ]
    }
  }

  builtin_default_scopes  = ["profile", "email", "roles", "web-origins"]
  builtin_optional_scopes = ["offline_access"]

  inventory_clients = {
    "gateway-api" = {
      name                         = "Gateway API"
      description                  = "Public edge gateway calling downstream services."
      service_accounts_enabled     = true
      standard_flow_enabled        = true
      direct_access_grants_enabled = false
      default_scope_keys           = ["product_read", "catalog_search"]
      optional_scope_keys          = ["stock_read", "stock_adjust", "stock_override", "product_manage"]
      valid_redirect_uris          = ["https://localhost/*"]
      service_account_role_key     = "role_public_api"
    }
    "inventory-api" = {
      name                         = "Inventory API"
      description                  = "Inventory resource server"
      service_accounts_enabled     = true
      standard_flow_enabled        = false
      direct_access_grants_enabled = false
      default_scope_keys           = []
      optional_scope_keys = [
        "product_read",
        "catalog_search",
        "stock_read",
        "stock_adjust",
        "stock_override",
        "product_manage"
      ]
      service_account_role_key = "role_inventory_admin"
    }
    "shipping-api" = {
      name                         = "Shipping API"
      description                  = "Shipping service calling inventory operations."
      service_accounts_enabled     = true
      standard_flow_enabled        = true
      direct_access_grants_enabled = false
      default_scope_keys           = ["product_read", "stock_read", "stock_adjust"]
      optional_scope_keys          = ["stock_override", "product_manage"]
      service_account_role_key     = "role_shipping_worker"
    }
    "backoffice-admin-api" = {
      name                         = "Backoffice Admin API"
      description                  = "Administrative backoffice client."
      service_accounts_enabled     = true
      standard_flow_enabled        = true
      direct_access_grants_enabled = true
      default_scope_keys = [
        "product_read",
        "catalog_search",
        "stock_read",
        "stock_adjust",
        "stock_override",
        "product_manage"
      ]
      optional_scope_keys      = []
      service_account_role_key = "role_inventory_admin"
    }
    "load-tester" = {
      name                         = "Load Tester"
      description                  = "Automated benchmarking client."
      service_accounts_enabled     = true
      standard_flow_enabled        = false
      direct_access_grants_enabled = true
      default_scope_keys = [
        "product_read",
        "catalog_search",
        "stock_read",
        "stock_adjust",
        "stock_override",
        "product_manage"
      ]
      optional_scope_keys      = []
      service_account_role_key = "role_inventory_admin"
    }
  }

  inventory_client_secrets = {
    "gateway-api"          = var.gateway_api_client_secret
    "inventory-api"        = var.inventory_api_client_secret
    "shipping-api"         = var.shipping_api_client_secret
    "backoffice-admin-api" = var.backoffice_admin_api_client_secret
    "load-tester"          = var.load_tester_client_secret
  }
}

resource "keycloak_realm" "zerotrust_oauth" {
  realm        = "zerotrust-oauth"
  enabled      = true
  display_name = "ZeroTrust OAuth Realm"
}

resource "keycloak_openid_client_scope" "inventory" {
  for_each = local.inventory_scope_definitions

  realm_id               = keycloak_realm.zerotrust_oauth.id
  name                   = each.value.name
  description            = each.value.description
  include_in_token_scope = true
  consent_screen_text    = each.value.description
}

resource "keycloak_role" "inventory_scope_roles" {
  for_each = local.inventory_scope_definitions

  realm_id    = keycloak_realm.zerotrust_oauth.id
  name        = each.value.name
  description = "Scope role for ${each.value.description}"
}

resource "keycloak_role" "inventory_role_profiles" {
  for_each = local.inventory_role_profiles

  realm_id    = keycloak_realm.zerotrust_oauth.id
  name        = each.key
  description = each.value.description
  composite_roles = [
    for scope_key in each.value.scope_keys :
    keycloak_role.inventory_scope_roles[scope_key].id
  ]
}

resource "keycloak_openid_client" "inventory_clients" {
  for_each = local.inventory_clients

  realm_id                     = keycloak_realm.zerotrust_oauth.id
  client_id                    = each.key
  name                         = each.value.name
  description                  = each.value.description
  access_type                  = "CONFIDENTIAL"
  standard_flow_enabled        = each.value.standard_flow_enabled
  direct_access_grants_enabled = each.value.direct_access_grants_enabled
  service_accounts_enabled     = each.value.service_accounts_enabled
  valid_redirect_uris          = lookup(each.value, "valid_redirect_uris", ["*"])
  web_origins                  = ["+"]
  root_url                     = lookup(each.value, "root_url", "")
  client_secret                = local.inventory_client_secrets[each.key]
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "inventory" {
  for_each = local.inventory_clients

  realm_id  = keycloak_realm.zerotrust_oauth.id
  client_id = keycloak_openid_client.inventory_clients[each.key].id
  default_scopes = concat(
    local.builtin_default_scopes,
    [for scope_key in lookup(each.value, "default_scope_keys", []) : local.inventory_scope_names[scope_key]]
  )
}

resource "keycloak_openid_client_optional_scopes" "inventory" {
  for_each = local.inventory_clients

  realm_id  = keycloak_realm.zerotrust_oauth.id
  client_id = keycloak_openid_client.inventory_clients[each.key].id
  optional_scopes = concat(
    local.builtin_optional_scopes,
    [for scope_key in lookup(each.value, "optional_scope_keys", []) : local.inventory_scope_names[scope_key]]
  )
}

resource "keycloak_openid_client_service_account_realm_role" "client_role_grants" {
  for_each = {
    for client, cfg in local.inventory_clients :
    client => cfg
    if contains(keys(cfg), "service_account_role_key")
  }

  realm_id                = keycloak_realm.zerotrust_oauth.id
  service_account_user_id = keycloak_openid_client.inventory_clients[each.key].service_account_user_id
  role                    = keycloak_role.inventory_role_profiles[each.value.service_account_role_key].name
}

