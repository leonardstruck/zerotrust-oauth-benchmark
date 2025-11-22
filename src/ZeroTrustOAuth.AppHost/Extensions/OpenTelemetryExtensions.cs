namespace ZeroTrustOAuth.AppHost.Extensions;

internal static class OpenTelemetryExtensions
{
    public static IResourceBuilder<T> WithOpenTelemetryRouting<T>(this IResourceBuilder<T> builder,
        EndpointReference target) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment(context =>
            {
                context.EnvironmentVariables["OTEL_EXPORTER_OTLP_ENDPOINT"] = target;
                return Task.CompletedTask;
            })
            .WithAnnotation(new WaitAnnotation(target.Resource, WaitType.WaitUntilHealthy));
    }
}