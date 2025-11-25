using System.ComponentModel.DataAnnotations;

namespace ZeroTrustOAuth.Auth;

public sealed class SecurityOptions
{
    [Required] public AuthFlow AuthFlow { get; set; } = AuthFlow.JwtPropagation;

    /// <summary>
    ///     Optional per-service overrides describing which scopes/audience to request when calling downstream services.
    /// </summary>
    [Required]
    public Dictionary<string, DownstreamServiceOptions> DownstreamServices { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}
