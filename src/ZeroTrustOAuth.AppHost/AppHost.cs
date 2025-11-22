using ZeroTrustOAuth.AppHost.Extensions;

#pragma warning disable ASPIRECERTIFICATES001
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

#region Grafana (LGTM) Stack

IResourceBuilder<ContainerResource> alloy = builder.AddContainer("alloy", "grafana/alloy:v1.11.3")
    .WithHttpsEndpoint(targetPort: 12345)
    .WithHttpsEndpoint(targetPort: 4317, name: "otlp", env: "OTLP_PORT")
    .WithHttpsEndpoint(targetPort: 4318, name: "otlp-http", env: "OTLP_HTTP_PORT")
    .WithHttpHealthCheck("/-/ready")
    .WithArgs("run", "--server.http.listen-addr=0.0.0.0:12345", "/etc/alloy/config.alloy")
    .WithBindMount("./Config/config.alloy", "/etc/alloy/config.alloy", true)
    .WithCertificateKeyPairConfiguration(context =>
    {
        context.EnvironmentVariables["TLS_CERT_PATH"] = context.CertificatePath;
        context.EnvironmentVariables["TLS_KEY_PATH"] = context.KeyPath;

        return Task.CompletedTask;
    })
    .WithOtlpExporter();

EndpointReference otlpEndpoint = alloy.GetEndpoint("otlp");

#endregion

IResourceBuilder<KeycloakResource> identity = builder.AddKeycloak("identity")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("identity/{**catch-all}", identity);
    })
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOpenTelemetryRouting(otlpEndpoint);

DistributedApplication app = builder.Build();


await app.RunAsync();