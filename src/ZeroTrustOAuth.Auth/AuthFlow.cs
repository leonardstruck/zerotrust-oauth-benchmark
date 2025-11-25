namespace ZeroTrustOAuth.Auth;

public enum AuthFlow
{
    JwtPropagation = 0,
    OpaqueIntrospection = 1,
    TokenExchange = 2
}
