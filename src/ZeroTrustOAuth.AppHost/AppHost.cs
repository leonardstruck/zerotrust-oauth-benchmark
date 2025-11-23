#pragma warning disable ASPIRECONTAINERSHELLEXECUTION001
using ZeroTrustOAuth.AppHost.Hosting.Grafana;
using ZeroTrustOAuth.AppHost.Hosting.OpenTofu;


IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var grafana = builder
    .AddGrafanaStack("grafana");


IResourceBuilder<KeycloakResource> identity = builder
    .AddKeycloak("identity");

builder.AddOpenTofuProvisioner("identity-provisioner", "./terraform/identity")
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
    .WithOtlpRouting(grafana);

await builder.Build().RunAsync();