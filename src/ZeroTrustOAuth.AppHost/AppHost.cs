using ZeroTrustOAuth.AppHost.Hosting.Grafana;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var grafana = builder.AddGrafanaStack("grafana").WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<KeycloakResource> identity = builder
    .AddKeycloak("identity")
    .WithLifetime(ContainerLifetime.Persistent);

builder
    .AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
    })
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOtlpRouting(grafana);

DistributedApplication app = builder.Build();

await app.RunAsync();
