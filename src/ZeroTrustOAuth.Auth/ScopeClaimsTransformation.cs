using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace ZeroTrustOAuth.Auth;

/// <summary>
///     Normalizes scope claims so downstream authorization policies can rely on consistent claim types and formats.
/// </summary>
public sealed class ScopeClaimsTransformation : IClaimsTransformation
{
    private static readonly char[] ScopeSeparators = [' ', ','];

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(principal);
        }

        List<Claim> scopeClaims = identity.FindAll("scope").ToList();
        List<Claim> scpClaims = identity.FindAll("scp").ToList();

        bool requiresNormalization = scpClaims.Count > 0 || scopeClaims.Any(claim => ContainsSeparator(claim.Value));
        if (!requiresNormalization)
        {
            return Task.FromResult(principal);
        }

        foreach (Claim claim in scopeClaims)
        {
            identity.RemoveClaim(claim);
        }

        HashSet<string> normalizedScopes = new(StringComparer.Ordinal);
        foreach (Claim claim in scopeClaims)
        {
            foreach (string scope in SplitScopes(claim.Value))
            {
                normalizedScopes.Add(scope);
            }
        }

        foreach (Claim claim in scpClaims)
        {
            identity.RemoveClaim(claim);
            foreach (string scope in SplitScopes(claim.Value))
            {
                normalizedScopes.Add(scope);
            }
        }

        foreach (string scope in normalizedScopes)
        {
            identity.AddClaim(new Claim("scope", scope));
        }

        return Task.FromResult(principal);
    }

    private static IEnumerable<string> SplitScopes(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        string[] parts = value.Split(ScopeSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (string part in parts)
        {
            if (!string.IsNullOrWhiteSpace(part))
            {
                yield return part;
            }
        }
    }

    private static bool ContainsSeparator(string value) => value.IndexOfAny(ScopeSeparators) >= 0;
}
