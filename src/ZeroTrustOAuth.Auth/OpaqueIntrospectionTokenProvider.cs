using System.Security.Claims;

namespace ZeroTrustOAuth.Auth;

internal sealed class OpaqueIntrospectionTokenProvider : ITokenProvider
{
    public Task<string?> GetTokenForAsync(
        string downstreamService,
        ClaimsPrincipal? currentUser,
        CancellationToken cancellationToken = default)
    {
        // Placeholder implementation; future work will retrieve opaque tokens and perform token refresh/introspection.
        return Task.FromResult<string?>(null);
    }
}
