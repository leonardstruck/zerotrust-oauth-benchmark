using System.Security.Claims;

namespace ZeroTrustOAuth.Auth;

internal sealed class TokenExchangeTokenProvider : ITokenProvider
{
    public Task<string?> GetTokenForAsync(
        string downstreamService,
        ClaimsPrincipal? currentUser,
        CancellationToken cancellationToken = default)
    {
        // Placeholder implementation; will call Keycloak token endpoint with grant_type=token-exchange in a later iteration.
        return Task.FromResult<string?>(null);
    }
}
