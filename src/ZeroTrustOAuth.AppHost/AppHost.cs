#pragma warning disable ASPIRECONTAINERSHELLEXECUTION001
using ZeroTrustOAuth.AppHost.Hosting.Grafana;
using ZeroTrustOAuth.AppHost.Hosting.OpenTofu;


IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var grafana = builder
    .AddGrafanaStack("grafana")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<KeycloakResource> identity = builder
    .AddKeycloak("identity")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddOpenTofuProvisioner("identity-provisioner", "./Provisioning/identity")
    .WithParentRelationship(identity)
    .WaitFor(identity)
    .WithVariable("keycloak_url", identity.GetEndpoint("http"))
    .WithVariable("keycloak_password", identity.Resource.AdminPasswordParameter);


builder
    .AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
    })
    .WithOtlpRouting(grafana)
    .WithLifetime(ContainerLifetime.Persistent);

await builder.Build().RunAsync();