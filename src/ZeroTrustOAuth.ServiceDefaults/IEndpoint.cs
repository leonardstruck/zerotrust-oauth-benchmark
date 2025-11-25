using Microsoft.AspNetCore.Routing;

namespace ZeroTrustOAuth.ServiceDefaults;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}