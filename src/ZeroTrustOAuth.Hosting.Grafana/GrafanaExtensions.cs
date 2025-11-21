using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
///     Provides extension methods to configure and add Grafana resources
///     to a distributed application using the Aspire hosting framework.
/// </summary>
[PublicAPI]
public static class GrafanaExtensions
{
    private const int DefaultContainerPort = 3000;

    /// <summary>
    ///     Adds a Grafana resource to the distributed application using the specified parameters.
    /// </summary>
    /// <param name="builder">
    ///     The application builder used to add and configure distributed resources.
    /// </param>
    /// <param name="name">
    ///     The name of the Grafana resource to be added.
    /// </param>
    /// <param name="port">
    ///     An optional port number for the Grafana resource. If not specified, a default value is used.
    /// </param>
    /// <param name="configure">
    ///     An optional delegate to configure additional <see cref="GrafanaSettings" /> such as enabling
    ///     health checks or other resource-specific behavior.
    /// </param>
    /// <returns>
    ///     A resource builder for further configuring the Grafana resource.
    /// </returns>
    public static IResourceBuilder<GrafanaResource> AddGrafana(this IDistributedApplicationBuilder builder,
        [ResourceName] string name, int? port = null, Action<GrafanaSettings>? configure = null)
    {
        GrafanaSettings settings = new();
        configure?.Invoke(settings);

        GrafanaResource resource = new(name);
        IResourceBuilder<GrafanaResource> resourceBuilder = builder.AddResource(resource)
            .WithImage(GrafanaContainerImageTags.Image, GrafanaContainerImageTags.Tag)
            .WithImageRegistry(GrafanaContainerImageTags.Registry)
            .WithHttpEndpoint(port, DefaultContainerPort, GrafanaResource.PrimaryEndpointName);

        if (settings.EnableHealthCheck)
        {
            resourceBuilder.WithHttpHealthCheck("/api/health", endpointName: GrafanaResource.PrimaryEndpointName);
        }

        if (builder.ExecutionContext.IsRunMode)
        {
            resourceBuilder
                .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
                .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin");
        }

        return resourceBuilder;
    }
}