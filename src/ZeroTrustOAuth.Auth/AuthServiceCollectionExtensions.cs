using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ZeroTrustOAuth.Auth;

public static class AuthServiceCollectionExtensions
{
    private const string ScopeClaimType = "scope";

    public static IServiceCollection AddZeroTrustAuthentication(
        this IServiceCollection services,
        IHostApplicationBuilder builder,
        string identityServiceName,
        string realm,
        string audience)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityServiceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);

        IConfigurationSection securitySection = builder.Configuration.GetSection("Security");
        services.AddOptions<SecurityOptions>()
            .Bind(securitySection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        SecurityOptions securityOptions = securitySection.Get<SecurityOptions>() ?? new();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(identityServiceName, realm, options =>
            {
                options.Audience = audience;
                options.MapInboundClaims = false;

                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });

        // Future flows can swap or extend authentication handlers.
        _ = securityOptions.AuthFlow;

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IClaimsTransformation, ScopeClaimsTransformation>());
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddScoped<JwtPropagationTokenProvider>();
        services.AddScoped<OpaqueIntrospectionTokenProvider>();
        services.AddScoped<TokenExchangeTokenProvider>();

        services.AddScoped<ITokenProvider>(sp => TokenProviderFactory.Create(sp));

        return services;
    }

    public static IServiceCollection AddZeroTrustAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ScopePolicies.InventoryProductRead,
                policy => policy.RequireScope(ScopeNames.Inventory.ProductRead));
            options.AddPolicy(ScopePolicies.InventoryCatalogSearch,
                policy => policy.RequireScope(ScopeNames.Inventory.CatalogSearch));
            options.AddPolicy(ScopePolicies.InventoryStockRead,
                policy => policy.RequireScope(ScopeNames.Inventory.StockRead));
            options.AddPolicy(ScopePolicies.InventoryStockAdjust,
                policy => policy.RequireScope(ScopeNames.Inventory.StockAdjust));
            options.AddPolicy(ScopePolicies.InventoryStockOverride,
                policy => policy.RequireScope(ScopeNames.Inventory.StockOverride));
            options.AddPolicy(ScopePolicies.InventoryProductManage,
                policy => policy.RequireScope(ScopeNames.Inventory.ProductManage));
        });

        return services;
    }

    private static void RequireScope(
        this AuthorizationPolicyBuilder builder,
        string scope)
    {
        builder.RequireClaim(ScopeClaimType, scope);
    }
}
