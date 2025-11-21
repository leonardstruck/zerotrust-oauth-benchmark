namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a resource for an OpenTelemetry Collector container, providing endpoints for gRPC and HTTP communication.
/// </summary>
public class OpenTelemetryCollectorResource(string name) : ContainerResource(name)
{
    internal const string GrpcEndpointName = "grpc";
    internal const string HttpEndpointName = "http";

    /// <summary>
    /// Gets the gRPC endpoint reference for the OpenTelemetry Collector.
    /// </summary>
    public EndpointReference GrpcEndpoint => field ??= new EndpointReference(this, GrpcEndpointName);

    /// <summary>
    /// Gets the HTTP endpoint reference for the OpenTelemetry Collector.
    /// </summary>
    public EndpointReference HttpEndpoint => field ??= new EndpointReference(this, HttpEndpointName);
}