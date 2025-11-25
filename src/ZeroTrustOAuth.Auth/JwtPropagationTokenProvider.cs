using System.Security.Claims;

using Microsoft.AspNetCore.Http;

namespace ZeroTrustOAuth.Auth;

internal sealed class JwtPropagationTokenProvider(IHttpContextAccessor httpContextAccessor) : ITokenProvider
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    public Task<string?> GetTokenForAsync(
        string downstreamService,
        ClaimsPrincipal? currentUser,
        CancellationToken cancellationToken = default)
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return Task.FromResult<string?>(null);
        }

        if (!context.Request.Headers.TryGetValue(AuthorizationHeader, out var authorizationHeaders))
        {
            return Task.FromResult<string?>(null);
        }

        foreach (string? headerValue in authorizationHeaders)
        {
            if (headerValue != null && headerValue.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<string?>(headerValue[BearerPrefix.Length..].Trim());
            }
        }

        return Task.FromResult<string?>(null);
    }
}
