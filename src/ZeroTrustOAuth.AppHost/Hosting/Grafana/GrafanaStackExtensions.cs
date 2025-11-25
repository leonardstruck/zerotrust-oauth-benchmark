namespace ZeroTrustOAuth.AppHost.Hosting.Grafana;

internal static class GrafanaStackExtensions
{
    private const int DefaultContainerPort = 3000;
    private const int OtlpContainerPort = 4317;
    private const int OtlpHttpContainerPort = 4318;

    public static IResourceBuilder<GrafanaStackResource> AddGrafanaStack(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? port = null,
        int? otlpPort = null,
        int? otlpHttpPort = null,
        bool enableAspireDashboardForwarding = true
    )
    {
        GrafanaStackResource resource = new(name);
        IResourceBuilder<GrafanaStackResource> resourceBuilder = builder.AddResource(resource);

        resourceBuilder
            .WithImage(GrafanaStackContainerImageTags.Image, GrafanaStackContainerImageTags.Tag)
            .WithImageRegistry(GrafanaStackContainerImageTags.Registry)
            .WithHttpEndpoint(port, DefaultContainerPort, GrafanaStackResource.PrimaryEndpointName)
            .WithHttpEndpoint(otlpPort, OtlpContainerPort, GrafanaStackResource.OtlpEndpointName)
            .WithHttpEndpoint(
                otlpHttpPort,
                OtlpHttpContainerPort,
                GrafanaStackResource.OtlpHttpEndpointName
            )
            .WithHttpHealthCheck("/api/health");

        if (enableAspireDashboardForwarding)
        {
            resourceBuilder.WithAspireDashboardForwarding();
        }

        return resourceBuilder;
    }

    private static IResourceBuilder<GrafanaStackResource> WithAspireDashboardForwarding(
        this IResourceBuilder<GrafanaStackResource> builder
    )
    {
        return builder
            .WithOtlpExporter()
            .WithContainerFiles(
                "/otel-lgtm",
                [
                    new ContainerFile
                    {
                        Name = "otelcol-config-export-http.yaml",
                        Contents = """
                                   service:
                                     pipelines:
                                       traces:
                                         exporters: [otlphttp/traces, otlp/aspire]
                                       metrics:
                                         exporters: [otlphttp/metrics, otlp/aspire]
                                       logs:
                                         exporters: [otlphttp/logs, otlp/aspire]
                                   exporters:
                                     otlp/aspire:
                                       endpoint: ${env:OTEL_EXPORTER_OTLP_ENDPOINT}
                                   """
                    }
                ]
            );
    }

    public static IResourceBuilder<T> WithOtlpRouting<T, TTarget>(
        this IResourceBuilder<T> builder,
        IResourceBuilder<TTarget> grafanaBuilder
    )
        where T : IResourceWithEnvironment
        where TTarget : GrafanaStackResource
    {
        return builder
            .WithEnvironment(context =>
            {
                context.Resource.TryGetLastAnnotation<OtlpExporterAnnotation>(
                    out OtlpExporterAnnotation? otlpAnnotation
                );

                EndpointReference endpoint = otlpAnnotation?.RequiredProtocol switch
                {
                    OtlpProtocol.HttpProtobuf => grafanaBuilder.Resource.OtlpHttpEndpoint,
                    OtlpProtocol.Grpc => grafanaBuilder.Resource.OtlpEndpoint,
                    _ => grafanaBuilder.Resource.OtlpEndpoint
                };

                context.EnvironmentVariables["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint;
                return Task.CompletedTask;
            })
            .WithAnnotation(new WaitAnnotation(grafanaBuilder.Resource, WaitType.WaitUntilHealthy));
    }
}