#pragma warning disable ASPIRECONTAINERSHELLEXECUTION001

using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

using Projects;

using Scalar.Aspire;

using ZeroTrustOAuth.AppHost.Hosting.Grafana;
using ZeroTrustOAuth.AppHost.Hosting.Keycloak;
using ZeroTrustOAuth.AppHost.Hosting.OpenTofu;
using ZeroTrustOAuth.ServiceDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<GrafanaStackResource> grafana = builder
    .AddGrafanaStack(ServiceNames.Grafana);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres(ServiceNames.Postgres);

IResourceBuilder<PostgresDatabaseResource> identityDb = postgres.AddDatabase(ServiceNames.IdentityDb);

IResourceBuilder<KeycloakResource> identity = builder
    .AddKeycloak(ServiceNames.Identity)
    .WithPostgres(identityDb)
    .WithTracing()
    .WithOtlpRouting(grafana);

builder.AddOpenTofuProvisioner("identity-provisioner", "./Provisioning/identity")
    .WithParentRelationship(identity)
    .WaitFor(identity)
    .WithVariable("keycloak_url", identity.GetEndpoint("http"))
    .WithVariable("keycloak_password", identity.Resource.AdminPasswordParameter)
    .WithOtlpRouting(grafana);


IResourceBuilder<PostgresDatabaseResource> inventoryDb = postgres.AddDatabase(ServiceNames.InventoryDb);

IResourceBuilder<ProjectResource> inventory = builder.AddProject<ZeroTrustOAuth_Inventory>(ServiceNames.Inventory)
    .WithHttpHealthCheck("health")
    .WaitFor(inventoryDb)
    .WithReference(inventoryDb)
    .WithReference(identity)
    .WithOtlpRouting(grafana);

IResourceBuilder<YarpResource> gateway = builder
    .AddYarp(ServiceNames.Gateway)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("api/inventory/{**catch-all}", inventory)
            .WithTransformPathRemovePrefix("/api/inventory");
    }).WithOtlpRouting(grafana);

_ =
    builder.AddScalarApiReference("docs", o => o.PreferHttpsEndpoint())
        .WithApiReference(gateway, o =>
            o.AddDocument("inventory")
                .WithOpenApiRoutePattern("/api/inventory/openapi/v1.json")
                .AddServer("/api/inventory"));

await builder.Build().RunAsync();