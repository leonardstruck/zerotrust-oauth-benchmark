using System.ComponentModel.DataAnnotations;

namespace ZeroTrustOAuth.Auth;

public sealed class DownstreamServiceOptions
{
    [Required] public string? Audience { get; init; }

    /// <summary>
    ///     Optional list of scopes that represent the minimal authorization needed for the downstream call.
    /// </summary>
    public List<string> Scopes { get; init; } = new();
}
