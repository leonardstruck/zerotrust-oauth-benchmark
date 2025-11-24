#pragma warning disable ASPIRECONTAINERSHELLEXECUTION001

using Projects;

using Scalar.Aspire;

using ZeroTrustOAuth.AppHost.Hosting.Grafana;
using ZeroTrustOAuth.AppHost.Hosting.OpenTofu;
using ZeroTrustOAuth.ServiceDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<GrafanaStackResource> grafana = builder
    .AddGrafanaStack(ServiceNames.Grafana)
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<KeycloakResource> identity = builder
    .AddKeycloak(ServiceNames.Identity)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddOpenTofuProvisioner("identity-provisioner", "./Provisioning/identity")
    .WithParentRelationship(identity)
    .WaitFor(identity)
    .WithVariable("keycloak_url", identity.GetEndpoint("http"))
    .WithVariable("keycloak_password", identity.Resource.AdminPasswordParameter);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres(ServiceNames.Postgres)
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> inventoryDb = postgres.AddDatabase(ServiceNames.InventoryDb);

IResourceBuilder<ProjectResource> inventory = builder.AddProject<ZeroTrustOAuth_Inventory>(ServiceNames.Inventory)
    .WithHttpHealthCheck("health")
    .WithReference(inventoryDb)
    .WithReference(identity)
    .WithOtlpRouting(grafana);

builder
    .AddYarp(ServiceNames.Gateway)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
        yarp.AddRoute("inventory/{**catch-all}", inventory);
    })
    .WithOtlpRouting(grafana)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddScalarApiReference()
    .WithApiReference(inventory, options =>
    {
        options
            .AddDocument("v1")
            .AddDocument("internal");
    });


await builder.Build().RunAsync();