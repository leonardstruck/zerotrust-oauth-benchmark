using System.ComponentModel.DataAnnotations;

namespace ZeroTrustOAuth.Auth;

public sealed class SecurityOptions
{
    [Required]
    public AuthFlow AuthFlow { get; set; } = AuthFlow.JwtPropagation;
}
