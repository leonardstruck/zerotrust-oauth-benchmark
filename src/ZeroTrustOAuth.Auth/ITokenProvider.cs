using System.Security.Claims;

namespace ZeroTrustOAuth.Auth;

public interface ITokenProvider
{
    Task<string?> GetTokenForAsync(
        string downstreamService,
        ClaimsPrincipal? currentUser,
        CancellationToken cancellationToken = default);
}
