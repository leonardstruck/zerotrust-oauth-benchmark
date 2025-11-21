namespace Aspire.Hosting.ApplicationModel;

/// <summary>
///     Represents a Grafana resource within the application model, enabling integration and management
///     of Grafana instances as containerized resources. Provides access to a primary HTTP endpoint
///     reference for interacting with the Grafana instance.
/// </summary>
/// <remarks>
///     This class is used in conjunction with the distributed application builder to configure and
///     deploy a Grafana instance. It builds on the container resource functionality and defines a
///     primary HTTP endpoint for external access.
/// </remarks>
/// <param name="name">
///     The name assigned to the Grafana resource, used to uniquely identify it within the
///     application model.
/// </param>
public class GrafanaResource(string name) : ContainerResource(name)
{
    internal const string PrimaryEndpointName = "http";

    /// <summary>
    ///     Gets the primary HTTP endpoint for interacting with the Grafana instance
    ///     represented by this resource.
    /// </summary>
    /// <remarks>
    ///     The primary endpoint is used to establish an external access point for
    ///     the Grafana resource. It serves as the main connection interface for
    ///     interacting with the Grafana instance over HTTP.
    /// </remarks>
    public EndpointReference PrimaryEndpoint => field ??= new EndpointReference(this, PrimaryEndpointName);
}