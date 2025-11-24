using Aspire.Hosting.Lifecycle;

using Microsoft.Extensions.Configuration;

namespace ZeroTrustOAuth.AppHost.Hosting.Grafana;

internal static class GrafanaStackExtensions
{
    private const int DefaultContainerPort = 3000;
    private const int OtlpContainerPort = 4317;
    private const int OtlpHttpContainerPort = 4318;

    private const string DashboardOtlpUrlVariableName = "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL";
    private const string DashboardOtlpApiKeyVariableName = "AppHost:OtlpApiKey";

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

        builder.Services.TryAddEventingSubscriber<GrafanaEventingSubscriber>();

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
        string? apiKey =
            builder.ApplicationBuilder.Configuration.GetValue<string>(DashboardOtlpApiKeyVariableName);
        string? otlpUrl =
            builder.ApplicationBuilder.Configuration.GetValue<string>(DashboardOtlpUrlVariableName);

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(otlpUrl))
        {
            throw new InvalidOperationException("Aspire Dashboard OTLP configuration is missing.");
        }

        return builder
            .WithEnvironment("ASPIRE_OTLP_ENDPOINT", new HostUrl(otlpUrl))
            .WithEnvironment("ASPIRE_OTLP_API_KEY", apiKey)
            .WithContainerFiles(
                "/otel-lgtm",
                [
                    new ContainerFile
                    {
                        Name = "otelcol-config.yaml",
                        Contents = """
                                   receivers:
                                     otlp:
                                       protocols:
                                         grpc:
                                           endpoint: 0.0.0.0:4317
                                           include_metadata: true
                                         http:
                                           endpoint: 0.0.0.0:4318
                                           include_metadata: true

                                   extensions:
                                     health_check:
                                       endpoint: 0.0.0.0:13133
                                       path: "/ready"
                                       
                                   processors:
                                     batch:

                                   exporters:
                                     otlphttp/metrics:
                                       endpoint: http://127.0.0.1:9090/api/v1/otlp
                                       tls:
                                         insecure: true
                                     otlphttp/traces:
                                       endpoint: http://127.0.0.1:4418
                                       tls:
                                         insecure: true
                                     otlphttp/logs:
                                       endpoint: http://127.0.0.1:3100/otlp
                                       tls:
                                         insecure: true
                                     otlp/profiles:
                                       endpoint: http://127.0.0.1:4040
                                       tls:
                                         insecure: true
                                     otlp/aspire:
                                       endpoint: ${env:ASPIRE_OTLP_ENDPOINT}
                                       headers:
                                         x-otlp-api-key: ${env:ASPIRE_OTLP_API_KEY}
                                     debug/metrics:
                                       verbosity: detailed
                                     debug/traces:
                                       verbosity: detailed
                                     debug/logs:
                                       verbosity: detailed

                                   service:
                                     extensions: [health_check]
                                     pipelines:
                                       traces:
                                         receivers: [otlp]
                                         processors: [batch]
                                         exporters: [otlphttp/traces,otlp/aspire]
                                         #exporters: [otlphttp/traces,otlp/aspire,debug/traces]
                                       metrics:
                                         receivers: [otlp]
                                         processors: [batch]
                                         exporters: [otlphttp/metrics,otlp/aspire]
                                         #exporters: [otlphttp/metrics,otlp/aspire,debug/metrics]
                                       logs:
                                         receivers: [otlp]
                                         processors: [batch]
                                         exporters: [otlphttp/logs,otlp/aspire]
                                         #exporters: [otlphttp/logs,otlp/aspire,debug/logs]
                                       profiles:
                                         receivers: [otlp]
                                         exporters: [otlp/profiles]
                                   """
                    }
                ]
            );
    }
}