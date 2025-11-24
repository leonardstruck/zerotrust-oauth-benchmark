#pragma warning disable ASPIRECONTAINERSHELLEXECUTION001
using Scalar.Aspire;

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

var postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var inventoryDb = postgres.AddDatabase("inventorydb");

var inventory = builder.AddProject<Projects.ZeroTrustOAuth_Inventory>("inventory")
    .WithReference(inventoryDb)
    .WithOtlpRouting(grafana);

builder
    .AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
        yarp.AddRoute("inventory/{**catch-all}", inventory);
    })
    .WithOtlpRouting(grafana)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddScalarApiReference()
    .WithApiReference(inventory);

await builder.Build().RunAsync();