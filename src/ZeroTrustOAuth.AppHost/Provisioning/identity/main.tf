resource "keycloak_realm" "zerotrust_oauth" {
  realm        = "zerotrust-oauth"
  enabled      = false
  display_name = "ZeroTrust OAuth Realm"
}

