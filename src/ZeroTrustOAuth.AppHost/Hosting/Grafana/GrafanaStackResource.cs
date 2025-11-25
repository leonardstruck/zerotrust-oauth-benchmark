namespace ZeroTrustOAuth.AppHost.Hosting.Grafana;

public class GrafanaStackResource(string name, string? entrypoint = null)
    : ContainerResource(name, entrypoint)
{
    public const string PrimaryEndpointName = "http";

    public const string OtlpEndpointName = "otlp";

    public const string OtlpHttpEndpointName = "otlp-http";
    public EndpointReference PrimaryEndpoint => field ??= new EndpointReference(this, PrimaryEndpointName);
    public EndpointReference OtlpEndpoint => field ??= new EndpointReference(this, OtlpEndpointName);
    public EndpointReference OtlpHttpEndpoint => field ??= new EndpointReference(this, OtlpHttpEndpointName);
}