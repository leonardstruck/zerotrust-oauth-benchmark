using Aspire.Hosting.ApplicationModel;

#pragma warning disable ASPIRECERTIFICATES001

namespace Aspire.Hosting;

/// <summary>
///     Extension methods for configuring and wiring an embedded OpenTelemetry Collector
///     within an Aspire distributed application.
/// </summary>
public static class OpenTelemetryCollectorExtensions
{
    private const int ContainerGrpcPort = 4317;
    private const int ContainerHttpPort = 4318;
    private const int ContainerHealthCheckPort = 13133;

    private const string HealthCheckEndpointName = "health";

    /// <summary>
    ///     Adds an OpenTelemetry Collector container resource to the distributed application and
    ///     configures common receivers, exporters, and health checks based on the provided settings.
    /// </summary>
    /// <param name="builder">The distributed application builder to add the collector resource to.</param>
    /// <param name="name">The unique resource name for the collector.</param>
    /// <param name="configure">Optional delegate to configure <see cref="OpenTelemetryCollectorSettings" />.</param>
    /// <returns>
    ///     A resource builder for the created <see cref="OpenTelemetryCollectorResource" /> allowing further customization.
    /// </returns>
    public static IResourceBuilder<OpenTelemetryCollectorResource> AddOpenTelemetryCollector(
        this IDistributedApplicationBuilder builder, [ResourceName] string name,
        Action<OpenTelemetryCollectorSettings>? configure = null)
    {
        OpenTelemetryCollectorResource resource = new(name);
        OpenTelemetryCollectorSettings settings = new();
        configure?.Invoke(settings);

        IResourceBuilder<OpenTelemetryCollectorResource> resourceBuilder = builder.AddResource(resource)
            .WithImage(OpenTelemetryCollectorImageTags.Image, OpenTelemetryCollectorImageTags.Tag)
            .WithImageRegistry(OpenTelemetryCollectorImageTags.Registry)
            .WithArgs("--feature-gates=confmap.enableMergeAppendOption");

        if (settings.EnableAspireDashboardOtlpExport)
        {
            resourceBuilder
                .WithOtlpExporter()
                .WithArgs(
                    "--config=yaml:exporters::otlp::endpoint: ${env:OTEL_EXPORTER_OTLP_ENDPOINT}",
                    "--config=yaml:exporters::otlp::auth::authenticator: headers_setter",
                    "--config=yaml:extensions::headers_setter::headers: [{action: upsert, key: x-otlp-api-key, from_context: x-otlp-api-key}]",
                    "--config=yaml:service::extensions: [ headers_setter ]",
                    "--config=yaml:service::pipelines::traces::exporters: [ otlp ]",
                    "--config=yaml:service::pipelines::logs::exporters: [ otlp ]",
                    "--config=yaml:service::pipelines::metrics::exporters: [ otlp ]"
                )
                .WithCertificateKeyPairConfiguration(context =>
                {
                    context.Arguments.Add(ReferenceExpression.Create(
                        $"--config=yaml:exporters::otlp::tls::cert_file: {context.CertificatePath}"));
                    context.Arguments.Add(
                        ReferenceExpression.Create($"--config=yaml:exporters::otlp::tls::key_file: {context.KeyPath}")
                    );
                    return Task.CompletedTask;
                })
                .WithCertificateTrustConfiguration(context =>
                {
                    context.Arguments.Add(
                        ReferenceExpression.Create(
                            $"--config=yaml:exporters::otlp::tls::ca_file: {context.CertificateBundlePath}")
                    );

                    return Task.CompletedTask;
                });
        }

        if (settings.EnableGrpcEndpoint)
        {
            resourceBuilder.WithHttpEndpoint(targetPort: ContainerGrpcPort,
                    name: OpenTelemetryCollectorResource.GrpcEndpointName)
                .WithArgs($"--config=yaml:receivers::otlp::protocols::grpc::endpoint: 0.0.0.0:{ContainerGrpcPort}",
                    "--config=yaml:receivers::otlp::protocols::grpc::include_metadata: true");
        }

        if (settings.EnableHttpEndpoint)
        {
            resourceBuilder.WithHttpEndpoint(targetPort: ContainerHttpPort,
                    name: OpenTelemetryCollectorResource.HttpEndpointName)
                .WithArgs($"--config=yaml:receivers::otlp::protocols::http::endpoint: 0.0.0.0:{ContainerHttpPort}",
                    "--config=yaml:receivers::otlp::protocols::http::include_metadata: true");
        }

        if (settings.EnableGrpcEndpoint || settings.EnableHttpEndpoint)
        {
            resourceBuilder.WithArgs(
                "--config=yaml:service::pipelines::traces::receivers: [ otlp ]",
                "--config=yaml:service::pipelines::logs::receivers: [ otlp ]",
                "--config=yaml:service::pipelines::metrics::receivers: [ otlp ]"
            );
        }

        if (settings.EnableHealthCheck)
        {
            resourceBuilder
                .WithHttpEndpoint(targetPort: ContainerHealthCheckPort, name: HealthCheckEndpointName)
                .WithHttpHealthCheck("/health", endpointName: HealthCheckEndpointName)
                .WithArgs(
                    $"--config=yaml:extensions::health_check/aspire::endpoint: 0.0.0.0:{ContainerHealthCheckPort}",
                    "--config=yaml:service::extensions: [ health_check/aspire ]"
                );
        }

        return resourceBuilder;
    }

    /// <summary>
    ///     Configures a resource to route its OTLP export to the provided OpenTelemetry Collector,
    ///     selecting the correct endpoint (gRPC or HTTP/Protobuf) based on the resource's
    ///     <see cref="OtlpExporterAnnotation" /> requirements. Also ensures the resource waits for the
    ///     collector to be ready.
    /// </summary>
    /// <typeparam name="T">
    ///     The resource type that supports environment configuration and wait semantics.
    /// </typeparam>
    /// <param name="builder">The resource builder being configured.</param>
    /// <param name="collector">The collector resource to route exports to.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IResourceBuilder<T> WithOpenTelemetryCollectorRouting<T>(this IResourceBuilder<T> builder,
        IResourceBuilder<OpenTelemetryCollectorResource> collector)
        where T : IResourceWithEnvironment, IResourceWithWaitSupport
    {
        return builder
            .WithEnvironment(context =>
            {
                context.Resource.TryGetLastAnnotation(out OtlpExporterAnnotation? annotation);

                OtlpProtocol requiredProtocol = annotation?.RequiredProtocol ?? OtlpProtocol.Grpc;

                EndpointReference endpoint = requiredProtocol switch
                {
                    OtlpProtocol.HttpProtobuf => collector.Resource.HttpEndpoint,
                    _ => collector.Resource.GrpcEndpoint
                };

                context.EnvironmentVariables["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint;
            })
            .WaitFor(collector);
    }
}