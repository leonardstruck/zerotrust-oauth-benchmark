using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ZeroTrustOAuth.Auth;

public static class AuthServiceCollectionExtensions
{
    private const string ScopeClaimType = "scope";

    public static IServiceCollection AddZeroTrustAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        KeycloakOptions keycloakOptions = configuration.GetSection("Keycloak").Get<KeycloakOptions>()
                                       ?? throw new InvalidOperationException(
                                           "Keycloak configuration is missing. Please configure the 'Keycloak' section.");

        SecurityOptions securityOptions = configuration.GetSection("Security").Get<SecurityOptions>() ?? new();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => ConfigureJwtBearer(options, keycloakOptions));

        // Future flows can swap or extend authentication handlers.
        _ = securityOptions.AuthFlow;

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IClaimsTransformation, ScopeClaimsTransformation>());

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

    private static void ConfigureJwtBearer(JwtBearerOptions options, KeycloakOptions keycloakOptions)
    {
        options.Authority = keycloakOptions.Authority;
        options.Audience = keycloakOptions.Audience;
        options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
        options.MapInboundClaims = false; // keep canonical claim types for scope policies
    }

    private static AuthorizationPolicyBuilder RequireScope(
        this AuthorizationPolicyBuilder builder,
        string scope)
    {
        builder.RequireClaim(ScopeClaimType, scope);
        return builder;
    }
}
