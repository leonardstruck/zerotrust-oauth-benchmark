using Microsoft.AspNetCore.Builder;

namespace ZeroTrustOAuth.ServiceDefaults;

public static class AuthTelemetryExtensions
{
    public static IApplicationBuilder UseAuthTelemetry(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<AuthTelemetryMiddleware>();
    }
}
