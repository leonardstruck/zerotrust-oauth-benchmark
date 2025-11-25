using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ZeroTrustOAuth.Auth;

internal static class TokenProviderFactory
{
    public static ITokenProvider Create(IServiceProvider serviceProvider)
    {
        SecurityOptions options = serviceProvider.GetRequiredService<IOptions<SecurityOptions>>().Value;

        return options.AuthFlow switch
        {
            AuthFlow.JwtPropagation => serviceProvider.GetRequiredService<JwtPropagationTokenProvider>(),
            AuthFlow.OpaqueIntrospection => serviceProvider.GetRequiredService<OpaqueIntrospectionTokenProvider>(),
            AuthFlow.TokenExchange => serviceProvider.GetRequiredService<TokenExchangeTokenProvider>(),
            _ => throw new NotSupportedException($"Auth flow '{options.AuthFlow}' is not supported.")
        };
    }
}
