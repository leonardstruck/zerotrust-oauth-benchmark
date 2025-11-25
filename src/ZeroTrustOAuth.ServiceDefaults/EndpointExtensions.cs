using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ZeroTrustOAuth.ServiceDefaults;

public static class EndpointExtensions
{
    public static IApplicationBuilder MapEndpoint<T>(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null) where T : IEndpoint
    {
        IEndpointRouteBuilder builder =
            routeGroupBuilder is null ? app : routeGroupBuilder;

        T endpoint = Activator.CreateInstance<T>();
        endpoint.MapEndpoint(builder);

        return app;
    }
}