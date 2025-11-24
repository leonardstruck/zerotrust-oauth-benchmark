using System.Diagnostics.CodeAnalysis;

using Carter.ModelBinding;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ZeroTrustOAuth.ServiceDefaults;

public static class CarterExtensions
{
    private static async ValueTask<object> RouteHandler<T>(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var param = (T?)context.Arguments.FirstOrDefault(x => x?.GetType() == typeof(T));

        var result = await context.HttpContext.Request.ValidateAsync(param);
        if (result.IsValid)
        {
            return await next(context) ?? Results.Ok();
        }

        var problemDetailsErrors = new Dictionary<string, object?> { { "errors", result.GetFormattedErrors() } };
        return Results.Problem(statusCode: 422, extensions: problemDetailsErrors);
    }

    /// <summary>
    /// Add a PATCH handler that will validate your model and return 422 if validation fails
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="pattern">The route path pattern</param>
    /// <param name="handler">The route handler</param>
    /// <typeparam name="T">The model to validate</typeparam>
    /// <returns></returns>
    public static RouteHandlerBuilder MapPatch<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        Delegate handler)
    {
        return endpoints.MapPatch(pattern, handler)
            .AddEndpointFilter(async (context, next) => await RouteHandler<T>(context, next));
    }
}
