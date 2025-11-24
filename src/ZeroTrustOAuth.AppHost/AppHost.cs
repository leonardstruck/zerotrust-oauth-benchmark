#pragma warning disable ASPIRECONTAINERSHELLEXECUTION001

using Projects;

using Scalar.Aspire;

using ZeroTrustOAuth.AppHost.Hosting.Grafana;
using ZeroTrustOAuth.AppHost.Hosting.OpenTofu;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<GrafanaStackResource> grafana = builder
    .AddGrafanaStack("grafana", 58731)
    .WithEnvironment("ENABLE_LOGS_OTELCOL", "true")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<KeycloakResource> identity = builder
    .AddKeycloak("identity")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddOpenTofuProvisioner("identity-provisioner", "./Provisioning/identity")
    .WithParentRelationship(identity)
    .WaitFor(identity)
    .WithVariable("keycloak_url", identity.GetEndpoint("http"))
    .WithVariable("keycloak_password", identity.Resource.AdminPasswordParameter);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> inventoryDb = postgres.AddDatabase("inventorydb");

IResourceBuilder<ProjectResource> inventory = builder.AddProject<ZeroTrustOAuth_Inventory>("inventory")
    .WithHttpHealthCheck("health")
    .WithReference(inventoryDb);

builder
    .AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
        yarp.AddRoute("inventory/{**catch-all}", inventory);
    })
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddScalarApiReference()
    .WithApiReference(inventory);

await builder.Build().RunAsync();