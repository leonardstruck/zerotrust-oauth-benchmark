namespace Aspire.Hosting.ApplicationModel;

public class OpenTelemetryCollectorResource(string name) : ContainerResource(name)
{
    internal const string GrpcEndpointName = "grpc";
    internal const string HttpEndpointName = "http";

    public EndpointReference GrpcEndpoint => field ??= new EndpointReference(this, GrpcEndpointName);
    public EndpointReference HttpEndpoint => field ??= new EndpointReference(this, HttpEndpointName);
}