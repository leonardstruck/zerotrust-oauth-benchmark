IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<OpenTelemetryCollectorResource> otlpCollector =
    builder.AddOpenTelemetryCollector("otel-collector");

IResourceBuilder<GrafanaResource> _ = builder.AddGrafana("grafana");

IResourceBuilder<KeycloakResource> identity = builder.AddKeycloak("identity")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
    })
    .WithOpenTelemetryCollectorRouting(otlpCollector)
    .WithLifetime(ContainerLifetime.Persistent);

await builder.Build().RunAsync();