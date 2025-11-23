using System;

namespace ZeroTrustOAuth.AppHost.Hosting.Grafana;

public class GrafanaStackResource(string name, string? entrypoint = null)
    : ContainerResource(name, entrypoint)
{
    public const string PrimaryEndpointName = "http";
    public EndpointReference PrimaryEndpoint => field ??= new(this, PrimaryEndpointName);

    public const string OtlpEndpointName = "otlp";
    public EndpointReference OtlpEndpoint => field ??= new(this, OtlpEndpointName);

    public const string OtlpHttpEndpointName = "otlp-http";
    public EndpointReference OtlpHttpEndpoint => field ??= new(this, OtlpHttpEndpointName);
}
