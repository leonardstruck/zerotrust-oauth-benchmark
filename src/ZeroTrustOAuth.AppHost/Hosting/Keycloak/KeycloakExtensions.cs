namespace ZeroTrustOAuth.AppHost.Hosting.Keycloak;

public static class KeycloakExtensions
{
    public static IResourceBuilder<KeycloakResource> WithTracing(
        this IResourceBuilder<KeycloakResource> builder
    )
    {
        builder
            .WithEnvironment("KC_TRACING_ENABLED", "true")
            .WithEnvironment("KC_TRACING_ENDPOINT", "${OTEL_EXPORTER_OTLP_ENDPOINT}")
            .WithEnvironment("KC_TRACING_RESOURCE_ATTRIBUTES", "${OTEL_RESOURCE_ATTRIBUTES}")
            .WithEnvironment("KC_TRACING_SAMPLER_TYPE", "${OTEL_TRACES_SAMPLER}")
            .WithEnvironment("KC_TRACING_SERVICE_NAME", "${OTEL_SERVICE_NAME}")
            .WithEnvironment(context =>
            {
                List<string> otelKeys = context.EnvironmentVariables.Keys
                    .Where(k => k.StartsWith("OTEL_", StringComparison.InvariantCultureIgnoreCase)).ToList();
                foreach (string key in otelKeys)
                {
                    context.EnvironmentVariables.Add(
                        key.Replace("OTEL_", "QUARKUS_OTEL_", StringComparison.InvariantCultureIgnoreCase),
                        $"${{{key}}}");
                }

                return Task.CompletedTask;
            })
            .WithCertificateTrustConfiguration(ctx =>
            {
                ctx.EnvironmentVariables["QUARKUS_OTEL_EXPORTER_OTLP_TRUST_CERT_CERTS"] =
                    ctx.CertificateBundlePath;
                return Task.CompletedTask;
            })
            .WithOtlpExporter();


        return builder;
    }
}