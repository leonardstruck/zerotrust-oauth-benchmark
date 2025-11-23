resource "keycloak_realm" "zerotrust_oauth" {
  realm        = "zerotrust-oauth"
  enabled      = true
  display_name = "ZeroTrust OAuth Realm"
}

