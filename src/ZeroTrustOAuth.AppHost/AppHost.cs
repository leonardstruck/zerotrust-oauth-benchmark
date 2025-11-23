using ZeroTrustOAuth.AppHost.Extensions;

#pragma warning disable ASPIRECERTIFICATES001
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

#region Grafana (LGTM) Stack

// Loki - log aggregation system
IResourceBuilder<ContainerResource> loki = builder
    .AddContainer("loki", "grafana/loki:3.5.8")
    .WithHttpsEndpoint(targetPort: 3100, name: "http")
    .WithArgs("-config.file=/etc/loki/loki.yaml", "-config.expand-env=true")
    .WithBindMount("./Config/loki.yaml", "/etc/loki/loki.yaml", true)
    .WithCertificateKeyPairConfiguration(context =>
    {
        context.EnvironmentVariables["TLS_CERT_PATH"] = context.CertificatePath;
        context.EnvironmentVariables["TLS_KEY_PATH"] = context.KeyPath;

        return Task.CompletedTask;
    });

IResourceBuilder<ContainerResource> prometheus = builder
    .AddContainer("prometheus", "prom/prometheus:v3.7.3")
    .WithHttpEndpoint(targetPort: 9090, name: "http")
    .WithBindMount("./Config/prometheus.yml", "/etc/prometheus/prometheus.yml", true)
    .WithContainerFiles("/data", [new ContainerDirectory() { Name = "prometheus", Owner = 65534 }])
    .WithArgs(
        "--config.file=/etc/prometheus/prometheus.yml",
        "--web.enable-remote-write-receiver",
        "--web.enable-otlp-receiver",
        "--enable-feature=exemplar-storage",
        "--enable-feature=native-histograms",
        "--storage.tsdb.path=/data/prometheus"
    );

// Tempo - distributed tracing backend
IResourceBuilder<ContainerResource> tempo = builder
    .AddContainer("tempo", "grafana/tempo:2.9.0")
    .WithHttpsEndpoint(targetPort: 3200, name: "http")
    .WithHttpsEndpoint(targetPort: 4317, name: "otlp")
    .WithHttpsEndpoint(targetPort: 4318, name: "otlp-http")
    .WithHttpHealthCheck("/ready")
    .WithArgs("-config.file=/etc/tempo.yaml", "-config.expand-env=true")
    .WithBindMount("./Config/tempo.yaml", "/etc/tempo.yaml", true)
    .WithCertificateKeyPairConfiguration(context =>
    {
        context.EnvironmentVariables["TLS_CERT_PATH"] = context.CertificatePath;
        context.EnvironmentVariables["TLS_KEY_PATH"] = context.KeyPath;

        return Task.CompletedTask;
    });

// Grafana - observability dashboard
IResourceBuilder<ContainerResource> grafana = builder
    .AddContainer("grafana", "grafana/grafana:12.3.0")
    .WithHttpsEndpoint(targetPort: 3000)
    .WithBindMount(
        "./Config/grafana-datasource.yaml",
        "/etc/grafana/provisioning/datasources/datasource.yaml",
        true
    )
    .WithEnvironment("TEMPO_URL", tempo.GetEndpoint("http"))
    .WithEnvironment("LOKI_URL", loki.GetEndpoint("http"))
    .WithEnvironment("PROMETHEUS_URL", prometheus.GetEndpoint("http"))
    .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin")
    .WithEnvironment("GF_AUTH_DISABLE_LOGIN_FORM", "true")
    .WithCertificateKeyPairConfiguration(context =>
    {
        context.EnvironmentVariables["GF_SERVER_CERT_FILE"] = context.CertificatePath;
        context.EnvironmentVariables["GF_SERVER_CERT_KEY"] = context.KeyPath;
        context.EnvironmentVariables["GF_SERVER_PROTOCOL"] = "https";

        return Task.CompletedTask;
    })
    .WaitFor(tempo)
    .WaitFor(loki)
    .WaitFor(prometheus);

// Alloy - telemetry pipeline (collector & agents)
IResourceBuilder<ContainerResource> alloy = builder
    .AddContainer("alloy", "grafana/alloy:v1.11.3")
    .WithHttpsEndpoint(targetPort: 12345)
    .WithHttpsEndpoint(targetPort: 4317, name: "otlp")
    .WithHttpsEndpoint(targetPort: 4318, name: "otlp-http")
    .WithHttpHealthCheck("/-/ready")
    .WithArgs("run", "--server.http.listen-addr=0.0.0.0:12345", "/etc/alloy/config.alloy")
    .WithBindMount("./Config/config.alloy", "/etc/alloy/config.alloy", true)
    .WithCertificateKeyPairConfiguration(context =>
    {
        context.EnvironmentVariables["TLS_CERT_PATH"] = context.CertificatePath;
        context.EnvironmentVariables["TLS_KEY_PATH"] = context.KeyPath;

        return Task.CompletedTask;
    })
    .WithOtlpExporter()
    .WithEnvironment("TEMPO_OTLP_ENDPOINT", tempo.GetEndpoint("otlp"))
    .WithEnvironment("LOKI_URL", loki.GetEndpoint("http"))
    .WithEnvironment("PROMETHEUS_URL", prometheus.GetEndpoint("http"))
    .WaitFor(tempo)
    .WaitFor(loki)
    .WaitFor(prometheus);

EndpointReference otlpEndpoint = alloy.GetEndpoint("otlp");

#endregion

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
    .WithOpenTelemetryRouting(otlpEndpoint);

DistributedApplication app = builder.Build();

await app.RunAsync();
