using System.Diagnostics;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using ZeroTrustOAuth.Auth;

namespace ZeroTrustOAuth.ServiceDefaults;

/// <summary>
///     Enriches incoming requests with OAuth-related telemetry and simple scope-matching metrics.
/// </summary>
internal sealed class AuthTelemetryMiddleware(
    RequestDelegate next,
    IAuthorizationPolicyProvider policyProvider,
    IOptions<SecurityOptions> securityOptions)
{
    private const string ScopeClaimType = "scope";
    private const string BearerPrefix = "Bearer ";

    private static readonly char[] ScopeSeparators = [' ', ','];

    public async Task InvokeAsync(HttpContext context)
    {
        Activity? activity = Activity.Current;
        SecurityOptions options = securityOptions.Value;

        string? token = ExtractBearerToken(context);
        HashSet<string> providedScopes = GetProvidedScopes(context.User);
        HashSet<string> requiredScopes = await GetRequiredScopesAsync(context);

        int missingCount = requiredScopes.Except(providedScopes, StringComparer.Ordinal).Count();
        int excessCount = providedScopes.Except(requiredScopes, StringComparer.Ordinal).Count();

        activity?.SetTag("oauth.flow", ToFlowTag(options.AuthFlow));
        activity?.SetTag("oauth.token_type", InferTokenType(token));
        activity?.SetTag("auth.scopes.provided", string.Join(",", providedScopes));
        activity?.SetTag("auth.scopes.required", string.Join(",", requiredScopes));
        activity?.SetTag("auth.scopes.missing_count", missingCount);
        activity?.SetTag("auth.scopes.excess_count", excessCount);

        AuthMetrics.RecordScopeCounts(missingCount, excessCount);

        await next(context);
    }

    private static string ToFlowTag(AuthFlow flow) =>
        flow switch
        {
            AuthFlow.JwtPropagation => "jwt_propagation",
            AuthFlow.OpaqueIntrospection => "opaque_introspection",
            AuthFlow.TokenExchange => "token_exchange",
            _ => "unknown"
        };

    private static string InferTokenType(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return "missing";
        }

        // Heuristic: JWTs contain two dots separating header.payload.signature.
        return token.Count(c => c == '.') >= 2 ? "jwt" : "opaque";
    }

    private static string? ExtractBearerToken(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeaders))
        {
            return null;
        }

        foreach (string? headerValue in authorizationHeaders)
        {
            if (headerValue is null)
            {
                continue;
            }

            if (headerValue.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return headerValue[BearerPrefix.Length..].Trim();
            }
        }

        return null;
    }

    private static HashSet<string> GetProvidedScopes(ClaimsPrincipal user)
    {
        HashSet<string> scopes = new(StringComparer.Ordinal);
        foreach (Claim claim in user.FindAll(ScopeClaimType))
        {
            if (string.IsNullOrWhiteSpace(claim.Value))
            {
                continue;
            }

            if (claim.Value.IndexOfAny(ScopeSeparators) >= 0)
            {
                foreach (string scope in claim.Value.Split(ScopeSeparators,
                             StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    scopes.Add(scope);
                }
            }
            else
            {
                scopes.Add(claim.Value);
            }
        }

        return scopes;
    }

    private async Task<HashSet<string>> GetRequiredScopesAsync(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            return [];
        }

        IReadOnlyList<IAuthorizeData> authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        if (authorizeData.Count == 0)
        {
            return [];
        }

        AuthorizationPolicy? policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
        if (policy is null)
        {
            return [];
        }

        HashSet<string> requiredScopes = new(StringComparer.Ordinal);
        foreach (IAuthorizationRequirement requirement in policy.Requirements)
        {
            if (requirement is ClaimsAuthorizationRequirement claimsRequirement
                && string.Equals(claimsRequirement.ClaimType, ScopeClaimType, StringComparison.Ordinal)
                && claimsRequirement.AllowedValues is not null)
            {
                foreach (string allowedValue in claimsRequirement.AllowedValues)
                {
                    requiredScopes.Add(allowedValue);
                }
            }
        }

        return requiredScopes;
    }
}
