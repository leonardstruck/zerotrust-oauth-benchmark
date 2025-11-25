using System.ComponentModel.DataAnnotations;

namespace ZeroTrustOAuth.Auth;

public sealed class KeycloakOptions
{

    [Required]
    [Url]
    public string Authority { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     Explicit introspection endpoint that should be used when operating in opaque-token mode.
    /// </summary>
    public string? IntrospectionEndpoint { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    /// <summary>
    ///     Allows development environments to opt-out of HTTPS metadata requirements.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;
}
